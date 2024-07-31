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

    public override void Initialized()
    {
        Instance.ParticleMan = new ParticleManager(Instance);

        // lets add some text/debug objects
        {
            debugOverlay = new SolidText()
            {
                Position = new Vector2f(-250, 10),
                Size = 16,
                Color = Color.White,
                Font = Instance.FontRepos.GetFont("Arial"),
                Text = "Frames: 0"
            };

            Instance.Level.UI.Add(debugOverlay);

            Instance.Level.UI.Add(new SolidText()
            {
                Position = new Vector2f(-250, 150),
                Size = 16,
                Color = Color.White,
                Font = Instance.FontRepos.GetFont("Arial"),
                Text = "KEYBINDS:\n" +
                "R - Reset interaction matrices\n" +
                "Scroll - Zoom in & out\n" +
                "Drag RClick - Move camera"
            });
        }

        // bounds
        {
            // particle bounds
            Instance.Level.Background.Add(new SolidObject()
            {
                Position = new Vector2f(-3, -3),
                Size = new Vector2f(ParticleManager.WorldSize + 6, ParticleManager.WorldSize + 6),
                Color = new Color(0x20, 0x20, 0x20)
            });

            Instance.Level.Background.Add(new SolidObject()
            {
                Position = new Vector2f(-263, -3),
                Size = new Vector2f(250, 250),
                Color = new Color(0x20, 0x20, 0x20)
            });
        }

        // red orange
        Instance.ParticleMan.ParticleSpecies.Add(new ParticleSpecies() { Particles = Instance.ParticleMan.Create(2500, Color.Red) });
        Instance.ParticleMan.ParticleSpecies.Add(new ParticleSpecies() { Particles = Instance.ParticleMan.Create(2500, new Color(255, 165, 0)) });

        //
        Instance.ParticleMan.ParticleSpecies.Add(new ParticleSpecies() { Particles = Instance.ParticleMan.Create(2500, Color.Yellow) });
        Instance.ParticleMan.ParticleSpecies.Add(new ParticleSpecies() { Particles = Instance.ParticleMan.Create(2500, Color.Green) });
    }

    public override void KeyPressed(KeyEventArgs e)
    {
        if (e.Code == Keyboard.Key.R)
            Instance.ParticleMan.interactionMatrix = new Dictionary<(int, int), float>(); // reset
    }

    protected override void OnFixedUpdate()
    {
        Instance.ParticleMan.DoRuleMatrix();
    }

    protected override void OnUpdate(RenderWindow ctx)
    {
        ctx.Clear(new Color(0x10, 0x10, 0x10)); // clear buffer ready for next frame
        ctx.DispatchEvents(); // handle window events

        Instance.Level.Draw(ctx); // draw scene

        debugOverlay.Text =
            $"Frames: {CurrentFPS}\n" +
            $"PhysicSteps: {CurrentPPS}\n" +
            $"\n" +
            $"SPECIES: {Instance.ParticleMan.ParticleSpecies.Count}\n" +
            $"ATOMS: {Instance.Level.particles.Count}\n";

        ctx.Display(); // swap buffers
    }

    #region Camera Dragging & zooming
    
    bool moving = false;
    Vector2f initMousePos;

    public override void MouseMoved(MouseMoveEventArgs e)
    {
        if (moving)
        {
            Vector2f curMousePos = new Vector2f(e.X, e.Y);
            Vector2f offset = curMousePos - initMousePos;

            Camera.Position -= offset * Camera.Zoom;

            initMousePos = curMousePos;
        }
    }

    public override void MouseButtonPressed(MouseButtonEventArgs e)
    {
        if (e.Button == Mouse.Button.Right)
        {
            moving = true;
            initMousePos = new Vector2f(e.X, e.Y);
        }
    }

    public override void MouseButtonReleased(MouseButtonEventArgs e)
    {
        if (e.Button == Mouse.Button.Right)
        {
            moving = false;
        }
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