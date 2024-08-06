using SFML.System;

internal class ClientInstance
{
    public Level Level = new Level();
    public FontRepository FontRepos = new FontRepository();
    public TextureRepository TextureRepos = new TextureRepository();
    public PhysicsLevel ParticleMan;
    public GuiData guidata = new GuiData();

    public bool StepPhysics = true;
}