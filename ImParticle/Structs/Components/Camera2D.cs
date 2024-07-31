using SFML.Graphics;
using SFML.System;
using SFML.Window;

class Camera2D
{
    private View view = new View(new FloatRect(0, 0, 0, 0));
    private Vector2f position = new Vector2f(0, 0); // camera info
    private Vector2f size = new Vector2f(700, 700);
    private float zoom = 2; // 1 for default

    public void Update(RenderWindow window)
    {
        view.Reset(new FloatRect(position, size));
        view.Zoom(zoom);

        window.SetView(view);
    }

    public Vector2u Size
    {
        get => new Vector2u((uint)size.X, (uint)size.Y);
        set
        {
            size = new Vector2f(value.X, value.Y);
        }
    }

    public Vector2f Position
    {
        get => position;
        set
        {
            position = value;
        }
    }

    public float Zoom
    {
        get => zoom;
        set
        {
            zoom = value;
        }
    }

    public Vector2f CursorToWorld(RenderWindow window, Vector2f mousePixelPos)
    {
        Vector2f worldPos = window.MapPixelToCoords(new Vector2i((int)mousePixelPos.X, (int)mousePixelPos.Y), view);

        return worldPos;
    }
}