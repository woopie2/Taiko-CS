using System.Numerics;
using Raylib_cs;

namespace Taiko_CS.Animation;

public class InstantDisappearAnimation : Animation
{
    private Texture2D texture;
    private bool isAnimating = false;
    private double timeBeforeDisappearing;
    private int alpha = 255;
    private double time = -1;
    private Rectangle src;
    private Rectangle dst;
    private bool hasFinished = false;
    public InstantDisappearAnimation(Texture2D texture, double timeBeforeDisappearing, int x, int y)
    {
        this.texture = texture;
        this.timeBeforeDisappearing = timeBeforeDisappearing;
        src = new Rectangle(0, 0, texture.Width, texture.Height);
        dst = new Rectangle(x, y, texture.Width, texture.Height);
        
    }
    
    public InstantDisappearAnimation(Texture2D texture, double timeBeforeDisappearing, Rectangle src, Rectangle dest)
    {
        this.texture = texture;
        this.timeBeforeDisappearing = timeBeforeDisappearing;
        this.src = src;
        dst = dest;
        
    }
    public override void UpdateAnimation()
    {
        if (!isAnimating)
        {
            return;
        }
        if (time == -1)
        {
            time = Raylib.GetTime();
        }

        if (Raylib.GetTime() - time >= timeBeforeDisappearing)
        {
            isAnimating = false;
            hasFinished = true;
        }
    }

    public override void StartAnimation()
    {
        isAnimating = true;
    }

    public override void Draw()
    {
        if (!isAnimating)
        {
            return;
        }
        
        Raylib.DrawTexturePro(texture, src, dst, new Vector2(0, 0), 0, Color.White);
    }

    public override bool IsFinish()
    {
        return hasFinished;
    }

    public override Animation Clone()
    {
        return new InstantDisappearAnimation(texture, timeBeforeDisappearing, src, dst);
    }
}