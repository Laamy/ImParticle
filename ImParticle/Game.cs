#region Includes

using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

#endregion

// TODO: gridify the world so I can process less things!!!!
internal class Game : GameEngine
{
    public ClientInstance Instance = new ClientInstance();

    public Game()
    {
        // we've finished so start the app
        Start();
    }

    public SolidText debugOverlay;
    public SolidObject worldBounds;

    public override void Initialized()
    {
        Instance.ParticleMan = new PhysicsLevel(Instance);

        // lets add some text/debug objects
        {
            debugOverlay = new SolidText()
            {
                Position = new Vector2f(-250, 10),
                Size = 16,
                Color = Color.White,
                Font = Instance.FontRepos.GetFont("Arial"),
                Text = ""
            };

            Instance.Level.UI.Add(debugOverlay);

            Instance.Level.UI.Add(new SolidText()
            {
                Position = new Vector2f(-250, 150),
                Size = 16,
                Color = Color.White,
                Font = Instance.FontRepos.GetFont("Arial"),
                Text = ""
            });
        }

        // bounds
        {
            // particle bounds
            worldBounds = new SolidObject()
            {
                Position = new Vector2f(-3, -3),
                Size = new Vector2f(PhysicsLevel.WorldSize + 6, PhysicsLevel.WorldSize + 6),
                Color = new Color(0x20, 0x20, 0x20)
            };

            Instance.Level.Background.Add(worldBounds);

            Instance.Level.Background.Add(new SolidObject()
            {
                Position = new Vector2f(-260, -3),
                Size = new Vector2f(250, 275),
                Color = new Color(0x20, 0x20, 0x20)
            });
        }

        //2500
        Instance.ParticleMan.ParticleSpecies.Add(new ParticleSpecies()
        {
            Particles = Instance.ParticleMan.Create(2, Color.Red),
            info = new ParticleInfo()
            {
                RuleDamping = 1
            }
        });
    }

    public override void KeyPressed(KeyEventArgs e)
    {
        if (e.Code == Keyboard.Key.R)
        {
            Dictionary<(int, int), float> matrices = new Dictionary<(int, int), float>(Instance.ParticleMan.interactMatrices);

            foreach (var node in Instance.ParticleMan.interactMatrices)
            {
                if (!Instance.ParticleMan.ParticleSpecies[node.Key.Item1].info.SpecialVariant &&
                    !Instance.ParticleMan.ParticleSpecies[node.Key.Item2].info.SpecialVariant
                )
                    matrices.Remove(node.Key);
            }

            Instance.ParticleMan.interactMatrices = new Dictionary<(int, int), float>(matrices);
        }

        if (e.Code == Keyboard.Key.Space)
        {
            Instance.StepPhysics = !Instance.StepPhysics;

            if (Instance.StepPhysics)
                worldBounds.Color = new Color(0x20, 0x20, 0x20);
            else
                worldBounds.Color = new Color(0x25, 0x20, 0x20);
        }

        if (e.Code == Keyboard.Key.Escape)
        {
            window.Close();
        }
    }

    protected override void OnFixedUpdate()
    {
        if (PushFromCursor)
        {

            Instance.ParticleMan.Rule(Instance.guidata.CursorPos, new ParticleInfo());
        }

        if (Instance.StepPhysics)
            Instance.ParticleMan.DoRuleMatrix();
    }

    protected override void OnUpdate(RenderWindow ctx)
    {
        ctx.Clear(new Color(0x10, 0x10, 0x10)); // clear buffer ready for next frame
        ctx.DispatchEvents(); // handle window events

        Instance.Level.Draw(ctx); // draw scene

        // visualize effect distance
        {
            CircleShape shape = new CircleShape();

            shape.FillColor = new Color(255, 255, 255, 128); // temp colour
            shape.Radius = 20;
            shape.Position = Camera.CursorToWorld(ctx, Instance.guidata.CursorPos) - new Vector2f(shape.Radius, shape.Radius);

            ctx.Draw(shape);
        }

        debugOverlay.Text =
            $"Frames: {CurrentFPS}\n" +
            $"PhysicSteps: {CurrentPPS}\n" +
            $"\n" +
            $"SPECIES: {Instance.ParticleMan.ParticleSpecies.Count}\n" +
            $"ATOMS: {Instance.Level.Particles.Count}\n";

        ctx.Display(); // swap buffers
    }

    #region Camera Dragging & zooming

    bool moving = false;
    Vector2f initMousePos;

    public override void MouseMoved(MouseMoveEventArgs e)
    {
        Instance.guidata.CursorPos = new Vector2f(e.X, e.Y);

        if (moving)
        {
            Vector2f curMousePos = new Vector2f(e.X, e.Y);
            Vector2f offset = curMousePos - initMousePos;

            Camera.Position -= offset * Camera.Zoom;

            initMousePos = curMousePos;
        }
    }

    bool PushFromCursor = false;

    public override void MouseButtonPressed(MouseButtonEventArgs e)
    {
        if (e.Button == Mouse.Button.Right)
        {
            moving = true;
            initMousePos = new Vector2f(e.X, e.Y);
        }

        if (e.Button == Mouse.Button.Left)
            PushFromCursor = true;
    }

    public override void MouseButtonReleased(MouseButtonEventArgs e)
    {
        if (e.Button == Mouse.Button.Right)
        {
            moving = false;
        }

        if (e.Button == Mouse.Button.Left)
            PushFromCursor = false;
    }

    // zooming
    public override void MouseWheelScrolled(MouseWheelScrollEventArgs e)
    {
        float zoomAmount = 0.1f;

        if (e.Delta > 0)
            Camera.Zoom /= (1 + zoomAmount);
        else if (e.Delta < 0)
            Camera.Zoom *= (1 + zoomAmount);
    }

    #endregion
}