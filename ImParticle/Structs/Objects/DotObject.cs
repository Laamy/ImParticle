#region Includes

using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;

#endregion

internal class DotObject : Object
{
    public DotObject(float radius)
    {
        Radius = radius;
        shape = new CircleShape(radius);
    }

    // base
    private float _radius = 2;
    public float Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            shape = new CircleShape(_radius);
        }
    }

    public Color Color;
    public Vector2f Velocity = new Vector2f(0, 0);

    CircleShape shape;

    // TODO: render in batch or on the GPU directly
    public override void Draw(RenderWindow e)
    {
        shape.Position = Position - new Vector2f(Radius, Radius);
        shape.FillColor = Color;

        e.Draw(shape);
    }
}