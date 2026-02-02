using System.ComponentModel;
using System.Numerics;
using Raylib_cs;

namespace Taiko_CS.Animation;

public class ScaleAnimation : Animation
{
    private bool isAnimating = false;
    private Texture2D texture;
    private double x;
    private double y;
    private double baseScale;
    private double finalScale;
    private double currentScale;
    private double timeInterval;
    private double time = -1;
    private double duration;
    
    public ScaleAnimation(Texture2D texture, double x, double y, double baseScale, double finalScale, double duration)
    {
        this.texture = texture;
        this.x = x;
        this.y = y;
        this.baseScale = baseScale;
        currentScale = baseScale;
        this.finalScale = finalScale;
        this.timeInterval = duration / ((finalScale - baseScale) / 0.1);
        this.duration = duration;
    }
    
    public override void UpdateAnimation()
    {
    
        if (!isAnimating)
        {
            return;
        }

        if (time < 0)
        {
            time = Raylib.GetTime();
        }

        double elapsed = Raylib.GetTime() - time;
        double progress = Math.Min(elapsed / timeInterval * 0.1, 1.0);
        currentScale = baseScale + (finalScale - baseScale) * progress;
        
    }

    public override void StartAnimation()
    {
        isAnimating = true;
    }

    public override void Draw()
    {
    
        float drawX = (float)(x - texture.Width * currentScale / 2);
        float drawY = (float)(y - texture.Height * currentScale / 2);
    
        Raylib.DrawTextureEx(
            texture, 
            new Vector2(drawX, drawY), 
            0, 
            (float)currentScale, 
            Color.White
        );
    }
    
    public override bool IsFinish()
    {
        return currentScale >= finalScale;
    }

    public override Animation Clone()
    {
        throw new NotImplementedException();
    }
}