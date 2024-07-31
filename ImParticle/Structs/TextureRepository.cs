#region Includes

using SFML.Graphics;
using System.IO;

#endregion

internal class TextureRepository
{
    const string DataPath = "Data\\Assets";

    public Texture GetTexture(string name)
    {
        if (!File.Exists(Path.Combine(DataPath, name)))
            return null; // doesnt exist

        return new Texture(Path.Combine(DataPath, name));
    }
}