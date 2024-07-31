#region Includes

using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;

#endregion

internal class SolidObject : Object
{
    // base
    public Vector2f Size;

    public Color? Color;
    public Texture Texture;

    public List<string> Tags;

    public override void Draw(RenderWindow e)
    {
        RectangleShape shape = new RectangleShape(Size);
        shape.Position = Position;

        if (Color != null)
            shape.FillColor = (Color)Color;
        else shape.Texture = Texture;

        e.Draw(shape);
    }
}