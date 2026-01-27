using System.Numerics;
using Raylib_cs;

namespace Taiko_CS.Animation;

public class FadeOutAnimation : Animation
{
    private Texture2D texture;
    private bool isAnimating;
    private double duration;
    private double timeInterval;
    private int alpha = 255;
    private double time = -1;
    private double holdTime = -1;
    private int x;
    private int y;
    private Rectangle src;
    private Rectangle dst;
    private bool isHoldFinished;
    public FadeOutAnimation(Texture2D texture, double duration, int x, int y)
    {
        this.texture = texture;
        this.duration = duration;
        timeInterval = duration / 255;
        this.x = x;
        this.y = y;
        isAnimating = true;
        src = new Rectangle(0, 0, texture.Width, texture.Height);
        dst = new Rectangle(x, y, texture.Width, texture.Height);
        
    }
    
    public FadeOutAnimation(Texture2D texture, double duration, Rectangle src, Rectangle dest)
    {
        this.texture = texture;
        this.duration = duration;
        timeInterval =  duration / 255;
        this.src = src;
        dst = dest;
        isHoldFinished = true;
    }
    
    public FadeOutAnimation(Texture2D texture, double duration, Rectangle src, Rectangle dest, double holdTime)
    {
        this.texture = texture;
        this.duration = duration;
        timeInterval =  duration / 255;
        this.src = src;
        dst = dest;
        this.holdTime = holdTime;
    }

    public override void StartAnimation()
    {
        isAnimating = holdTime == -1;
    }

    public override void UpdateAnimation()
    {
                
        if (time == -1)
        {
            time = Raylib.GetTime();
        }
        
        if (holdTime != -1)
        {
            if (Raylib.GetTime() - time >= holdTime)
            {
                isAnimating = true;
                isHoldFinished = true;
            }
        }
        
        if (!isAnimating)
        {
            return;
        }
        

        if (!isHoldFinished)
        {
            return;
        }

        bool isUpdating = false;
        double currentTime = Raylib.GetTime();
        while (currentTime - time >= timeInterval)
        {
            isUpdating = true;
            alpha--;
            if (alpha == 0)
            {
                break;
            }
            time += timeInterval;
        }

        if (isUpdating)
        {
            time = Raylib.GetTime();   
        }
    }

    public override void Draw()
    {
        Raylib.DrawTexturePro(texture, src, dst, new Vector2(0, 0), 0, new Color(255, 255, 255, alpha));
    }

    public override bool IsFinish()
    {
        return alpha == 0;
    }

    public override Animation Clone()
    {
        return new FadeOutAnimation(texture, duration, src, dst);
    }
}