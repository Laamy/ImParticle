#region Includes

using SFML.Graphics;
using System.Collections.Generic;
using System.Threading.Tasks;

#endregion

internal class Level
{
    // TODO make layers
    public List<Object> Background = new List<Object>();
    public List<DotObject> particles = new List<DotObject>();
    public List<Object> UI = new List<Object>();

    public void Draw(RenderWindow e)
    {
        foreach (Object child in Background)
            child.Draw(e);

        foreach (Object part in particles)
            part.Draw(e);

        foreach (Object child in UI)
            child.Draw(e);
    }
}