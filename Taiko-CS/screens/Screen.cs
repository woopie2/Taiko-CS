namespace Taiko_CS.screens;

public abstract class Screen
{
    public abstract void LoadTextures();
    public abstract void LoadSounds();
    public abstract void UnloadSounds();
    public abstract void UnloadTextures();
    public abstract void Draw();
    public abstract void HandleInput();
}