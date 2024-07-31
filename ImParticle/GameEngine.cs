#region Includes

using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using View = SFML.Graphics.View;

#endregion

internal class GameEngine
{
    /// <summary>
    /// The games target framerate
    /// </summary>
    private int targetFPS = 244;

    public int CurrentFPS = 0;
    private int frameCount = 0;
    private long fpsLastTime = DateTime.Now.Ticks;

    public int CurrentPPS = 0;
    private int physicStepCount = 0;
    private long lastPhysicsStep = DateTime.Now.Ticks;

    // sdl stuff
    public RenderWindow window;
    public Camera2D Camera = new Camera2D();

    public virtual void Initialized() { }

    public virtual void Closing() { }
    public virtual void Resize(SizeEventArgs e) { }

    public virtual void Focus() { }
    public virtual void LostFocus() { }

    public virtual void JoystickButtonPressed(JoystickButtonEventArgs e) { }
    public virtual void JoystickButtonReleased(JoystickButtonEventArgs e) { }
    public virtual void JoystickConnected(JoystickConnectEventArgs e) { }
    public virtual void JoystickDisconnected(JoystickConnectEventArgs e) { }
    public virtual void JoystickMoved(JoystickMoveEventArgs e) { }

    public virtual void KeyPressed(KeyEventArgs e) { }
    public virtual void KeyReleased(KeyEventArgs e) { }

    public virtual void MouseButtonPressed(MouseButtonEventArgs e) { }
    public virtual void MouseButtonReleased(MouseButtonEventArgs e) { }
    public virtual void MouseMoved(MouseMoveEventArgs e) { }
    public virtual void MouseWheelScrolled(MouseWheelScrollEventArgs e) { }
    public virtual void MouseEntered() { }
    public virtual void MouseLeft() { }

    public virtual void SensorChanged(SensorEventArgs e) { }
    public virtual void TextEntered(TextEventArgs e) { }

    public virtual void TouchBegan(TouchEventArgs e) { }
    public virtual void TouchEnded(TouchEventArgs e) { }
    public virtual void TouchMoved(TouchEventArgs e) { }

    public void Start()
    {
        // sdl renderer
        VideoMode mode = new VideoMode(800, 600);
        window = new RenderWindow(mode, "Game Engine");
        window.Closed += (s, e) =>
        {
            Closing();
            window.Close();
        };

        window.Resized += (s, e) =>
        {
            Resize(e);
            Size = new Vector2u(e.Width, e.Height);
        };

        window.GainedFocus += (s, e) => Focus();
        window.LostFocus += (s, e) => LostFocus();

        window.JoystickButtonPressed += (s, e) => JoystickButtonPressed(e);
        window.JoystickButtonReleased += (s, e) => JoystickButtonReleased(e);
        window.JoystickDisconnected += (s, e) => JoystickDisconnected(e);
        window.JoystickMoved += (s, e) => JoystickMoved(e);

        window.KeyPressed += (s, e) => KeyPressed(e);
        window.KeyReleased += (s, e) => KeyReleased(e);

        window.MouseButtonPressed += (s, e) => MouseButtonPressed(e);
        window.MouseButtonReleased += (s, e) => MouseButtonReleased(e);
        window.MouseMoved += (s, e) => MouseMoved(e);
        window.MouseWheelScrolled += (s, e) => MouseWheelScrolled(e);
        window.MouseEntered += (s, e) => MouseEntered();
        window.MouseLeft += (s, e) => MouseLeft();

        window.SensorChanged += (s, e) => SensorChanged(e);
        window.TextEntered += (s, e) => TextEntered(e);

        window.TouchBegan += (s, e) => TouchBegan(e);
        window.TouchEnded += (s, e) => TouchEnded(e);
        window.TouchMoved += (s, e) => TouchMoved(e);

        window.SetActive();

        Initialized();

        // physics thread
        Task.Factory.StartNew(() =>
        {
            long targetTicksPerFrame = TimeSpan.TicksPerSecond / 30;
            long prevTicks = DateTime.Now.Ticks;

            while (window.IsOpen)
            {
                long currTicks = DateTime.Now.Ticks;
                long elapsedTicks = currTicks - prevTicks;

                if (elapsedTicks >= targetTicksPerFrame)
                {
                    // update the camera
                    prevTicks = currTicks;
                    OnFixedUpdate();

                    physicStepCount++;

                    long curTime = DateTime.Now.Ticks;
                    if (curTime - lastPhysicsStep >= TimeSpan.TicksPerSecond)
                    {
                        CurrentPPS = physicStepCount;
                        physicStepCount = 0;
                        lastPhysicsStep = curTime;
                    }
                }

                Thread.Sleep(1);
            }
        });

        {
            long targetTicksPerFrame = TimeSpan.TicksPerSecond / targetFPS;
            long prevTicks = DateTime.Now.Ticks;

            while (window.IsOpen)
            {
                long currTicks = DateTime.Now.Ticks;
                long elapsedTicks = currTicks - prevTicks;

                if (elapsedTicks >= targetTicksPerFrame)
                {
                    // update the camera
                    prevTicks = currTicks;

                    Camera.Update(window); // update the window with the camera info
                    OnUpdate(window); // redraw window

                    frameCount++;

                    long curTime = DateTime.Now.Ticks;
                    if (curTime - fpsLastTime >= TimeSpan.TicksPerSecond)
                    {
                        CurrentFPS = frameCount;
                        frameCount = 0;
                        fpsLastTime = curTime;
                    }
                }

                //Thread.Sleep(1); //gonna just uncap this cuz its no longer limited by physics steps
            }
        }
    }

    protected virtual void OnUpdate(RenderWindow ctx) { }// draw event
    protected virtual void OnFixedUpdate() { }

    #region Easy Game Properties

    public Vector2u Size
    {
        get => Camera.Size;
        set
        {
            Camera.Size = value;
        }
    }

    public String Title
    { set => window.SetTitle(value); }

    #endregion
}