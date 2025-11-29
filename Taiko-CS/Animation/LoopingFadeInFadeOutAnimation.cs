using System.Numerics;
using Raylib_cs;

namespace Taiko_CS.Animation;

public class LoopingFadeInFadeOutAnimation : Animation
{
    private Texture2D texture;
    private Rectangle src;
    private Rectangle dst;
    private double cycleDuration;
    private double timeBeforeChange;
    private double time = -1;
    private int alpha;
    private int maxAlpha;
    private int minAlpha;
    private bool increaseAlpha = true;
    private bool isAnimating = false;
    
    public LoopingFadeInFadeOutAnimation(Texture2D texture, int x, int y, double cycleDuration, int minAlpha, int maxAlpha)
    {
        this.texture = texture;
        src = new Rectangle(0, 0, texture.Width, texture.Height);
        dst = new Rectangle(x, y, texture.Width, texture.Height);
        this.cycleDuration = cycleDuration;
        timeBeforeChange = cycleDuration / (((maxAlpha - minAlpha) * 2) - 1);
        this.maxAlpha = maxAlpha;
        this.minAlpha = minAlpha;
        alpha = minAlpha;
    }
    
    public LoopingFadeInFadeOutAnimation(Texture2D texture, Rectangle src, Rectangle dst, double cycleDuration, int minAlpha, int maxAlpha)
    {
        this.texture = texture;
        this.src = src;
        this.dst = dst;
        this.cycleDuration = cycleDuration;
        timeBeforeChange = cycleDuration / (((maxAlpha - minAlpha) * 2) - 1);
        this.maxAlpha = maxAlpha;
        this.minAlpha = minAlpha;
        alpha = minAlpha;
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

        if (Raylib.GetTime() - time >= timeBeforeChange)
        {
            if (alpha == maxAlpha)
            {
                increaseAlpha = false;
            }

            if (alpha == minAlpha)
            {
                increaseAlpha = true;
            }
            
            if (increaseAlpha && alpha < maxAlpha)
            {
                alpha++;
            } else if (!increaseAlpha && alpha > minAlpha)
            {
                alpha--;
            }
            time = Raylib.GetTime();
        }
    }

    public override void StartAnimation()
    {
        isAnimating = true;
    }

    public override void Draw()
    {
        Raylib.DrawTexturePro(texture, src, dst, new Vector2(0, 0), 0, new Color(255, 255, 255, alpha));
    }

    public override bool IsFinish()
    {
        return false;
    }

    public override Animation Clone()
    {
        return new LoopingFadeInFadeOutAnimation(texture, src, dst, cycleDuration, minAlpha, maxAlpha);
    }
}