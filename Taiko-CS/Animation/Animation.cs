namespace Taiko_CS.Animation;

public abstract class Animation
{
    public abstract void UpdateAnimation();
    public abstract void StartAnimation();
    public abstract void Draw();
    public abstract bool IsFinish();
}