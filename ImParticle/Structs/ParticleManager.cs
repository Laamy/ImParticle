using SFML.Graphics;
using SFML.System;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class ParticleSpecies
{
    public Color Colour;
    public DotObject[] Particles;

    // SUB-RULES
    public int GravityStrength = 80;
    public float SlowMultiplier = 0.5f;
    public float RuleDamping = 1.0f;
}

class ParticleManager
{
    public ParticleManager(ClientInstance instance)
    {
        Instance = instance;
    }

    // my version of this video https://www.youtube.com/watch?v=0Kx4Y9TVMGg
    public void Rule(ParticleSpecies species1, ParticleSpecies species2, float g)
    {
        Parallel.For(0, species1.Particles.Length, (i) =>
        {
            var p1 = species1.Particles[i];

            float fx = 0;
            float fy = 0;

            Parallel.For(0, species2.Particles.Length, (i2) =>
            {
                var p2 = species2.Particles[i2];

                float dx = p1.Position.X - p2.Position.X;
                float dy = p1.Position.Y - p2.Position.Y;

                float d = (float)Math.Sqrt(dx * dx + dy * dy);
                if (d > 0 && d < species2.GravityStrength) // 80^2
                {
                    float F = g * 1 / d;
                    fx += F * dx;
                    fy += F * dy;
                }
            });

            fx *= species1.RuleDamping;
            fy *= species1.RuleDamping;

            // velocity
            p1.Velocity.X = (p1.Velocity.X + fx) * species1.SlowMultiplier;
            p1.Velocity.Y = (p1.Velocity.Y + fy) * species1.SlowMultiplier;

            // brightness stuff
            {
                float maxVelocity = 3f;
                float minVelocity = -3f;

                float normalizedVelocityX = (p1.Velocity.X - minVelocity) / (maxVelocity - minVelocity);
                float normalizedVelocityY = (p1.Velocity.Y - minVelocity) / (maxVelocity - minVelocity);
                
                float combinedVelocity = (normalizedVelocityX + normalizedVelocityY) / 2f;

                int alpha = (int)(combinedVelocity * 240) + 15;

                if (alpha < 15) alpha = 15;
                if (alpha > 255) alpha = 255;

                Color colorWithAlpha = new Color(p1.Color.R, p1.Color.G, p1.Color.B, (byte)alpha);

                p1.Color = colorWithAlpha;
            }

            // update based on velocity
            p1.Position.X += p1.Velocity.X;
            p1.Position.Y += p1.Velocity.Y;

            if (p1.Position.X >= WorldSize)
            {
                p1.Velocity.X = 0;
                p1.Position.X = WorldSize;
            }
            if (p1.Position.Y >= WorldSize)
            {
                p1.Velocity.Y = 0;
                p1.Position.Y = WorldSize;
            }

            if (p1.Position.Y <= 0)
            {
                p1.Velocity.Y = 0;
                p1.Position.Y = 0;
            }
            if (p1.Position.X <= 0)
            {
                p1.Velocity.X = 0;
                p1.Position.X = 0;
            }
        });
    }

    private ClientInstance Instance;
    private Random ran = new Random();

    public const int WorldSize = 500;

    public float Random() => ran.Next(0, WorldSize);

    public DotObject[] Create(int num, Color colour)
    {
        DotObject[] group = new DotObject[num];

        for (int i = 0; i < num; ++i)
        {
            var obj = new DotObject(1)
            {
                Color = colour,
                Position = new Vector2f(Random(), Random())
            };

            group[i] = obj;
            Instance.Level.particles.Add(obj);
        }

        return group;
    }

    public List<ParticleSpecies> ParticleSpecies = new List<ParticleSpecies>();
    public Dictionary<(int, int), float> interactionMatrix = new Dictionary<(int, int), float>();

    public void SetInteraction(int speciesIndex1, int speciesIndex2, float g)
    {
        var key = speciesIndex1 < speciesIndex2 ? (speciesIndex1, speciesIndex2) : (speciesIndex2, speciesIndex1);
        interactionMatrix[key] = g;
    }

    public float? GetInteraction(int speciesIndex1, int speciesIndex2)
    {
        var key = speciesIndex1 < speciesIndex2 ? (speciesIndex1, speciesIndex2) : (speciesIndex2, speciesIndex1);
        if (interactionMatrix.TryGetValue(key, out float g))
            return g;

        return null;
    }

    // called 30 times a second at physics step
    public void DoRuleMatrix()
    {
        for (int i = 0; i < ParticleSpecies.Count; i++)
        {
            for (int i2 = 0; i2 < ParticleSpecies.Count; i2++)
            {
            redo:
                float? g = GetInteraction(i, i2);

                if (!g.HasValue)
                {
                    SetInteraction(i, i2, (250 - Random()) / WorldSize*2);
                    Console.WriteLine($"{i}:{i2} {GetInteraction(i, i2)}");
                    goto redo;
                }

                Rule(ParticleSpecies[i], ParticleSpecies[i2], g.Value);
            }
        }
    }
}