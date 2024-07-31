using SFML.System;

internal class ClientInstance
{
    public Level Level = new Level();
    public FontRepository FontRepos = new FontRepository();
    public TextureRepository TextureRepos = new TextureRepository();
    public ParticleManager ParticleMan;

    public bool StepPhysics = true;
}