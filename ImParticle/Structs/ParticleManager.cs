using SFML.Graphics;
using SFML.System;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class ParticleSpecies
{
    public Color Colour;
    public DotObject[] Particles;
}

class ParticleManager
{
    public ParticleManager(ClientInstance instance)
    {
        Instance = instance;
    }

    private ClientInstance Instance;
    private Random ran = new Random();

    public const int WorldSize = 1000;

    public float Random() => ran.Next(0, WorldSize);

    public DotObject[] Create(int num, Color colour)
    {
        DotObject[] group = new DotObject[num];

        for (int i = 0; i < num; ++i)
        {
            var obj = new DotObject(2)
            {
                Color = colour,
                Position = new Vector2f(Random(), Random())
            };

            group[i] = obj;
            Instance.Level.particles.Add(obj);
        }

        return group;
    }

    // my version of this video https://www.youtube.com/watch?v=0Kx4Y9TVMGg
    public void Rule(ParticleSpecies species1, ParticleSpecies species2, float g)
    {
        Parallel.For(0, species1.Particles.Length, (i) =>
        {
            var p1 = species1.Particles[i];

            float fx = 0;
            float fy = 0;

            for (int i2 = 0; i2 < species2.Particles.Length; ++i2)
            {
                var p2 = species2.Particles[i2];

                float dx = p1.Position.X - p2.Position.X;
                float dy = p1.Position.Y - p2.Position.Y;

                float d = (float)Math.Sqrt(dx * dx + dy * dy);
                if (d > 0 && d < 80) // 80^2
                {
                    float F = g * 1 / d;
                    fx += F * dx;
                    fy += F * dy;
                }
            }

            // velocity
            p1.Velocity.X = (p1.Velocity.X + fx) * 0.5f;
            p1.Velocity.Y = (p1.Velocity.Y + fy) * 0.5f;

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
                    SetInteraction(i, i2, (250 - Random()) / 1000);
                    Console.WriteLine($"{i}:{i2} {GetInteraction(i, i2)}");
                    goto redo;
                }

                Rule(ParticleSpecies[i], ParticleSpecies[i2], g.Value);
            }
        }
    }
}