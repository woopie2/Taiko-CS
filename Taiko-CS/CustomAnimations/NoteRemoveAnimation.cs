using System.Numerics;
using System.Runtime.CompilerServices;
using Raylib_cs;
using Taiko_CS.Animation;

namespace Taiko_CS.CustomAnimations;

public class NoteRemoveAnimation : Animation.Animation
{
    private bool isAnimating = false;
    private Texture2D texture;
    private Rectangle src;
    private double duration;
    private double scaleFactor;
    private double currentTime = -1;
    private double fadeInValue = 0;
    private double fadeOutValue = 0;
    private float x;
    private float y;
    
    public NoteRemoveAnimation(Texture2D texture, Rectangle src, double duration, double scaleFactor, float x, float y)
    {
        this.texture = texture;
        this.src = src;
        this.duration = duration;
        this.scaleFactor = scaleFactor;
        this.x = x;
        this.y = y;
    }
    public override void UpdateAnimation()
    {
        if (currentTime < 0)
        {
            currentTime = Raylib.GetTime();
        }
    
        // LINEAR progress - no easing
        double progress = EasingFunctions.EaseOutInCubic(Math.Min((Raylib.GetTime() - currentTime) / duration, 1.0));

        // Yellow fades IN: 0-50%
        double fadeInProgress = Math.Min(progress / 0.6, 1);

        // White fades OUT: 50-100%
        double fadeOutProgress = Math.Max((progress - 0.6) / 0.4, 0);

        if (fadeInProgress < 1.0)
        {
            fadeInValue = 255 * fadeInProgress;
            fadeOutValue = 0;
        }
        else
        {
            fadeInValue = 0;
            fadeOutValue = 255 * (1 - fadeOutProgress);
        }
    }
    public override void StartAnimation()
    {
        isAnimating = true;
    }

    public override void Draw()
    {
        if (fadeOutValue == 0)
        {
            Raylib.DrawTexturePro(texture, src, new Rectangle(x, y, src.Width * (float) scaleFactor, src.Height * (float) scaleFactor), Vector2.Zero, 0, Color.White);   
        }
        Raylib.DrawCircle((int) (x + src.Width * scaleFactor / 2), (int) (y + src.Height * scaleFactor / 2), (float) (src.Width * scaleFactor / 2), new Color(255f, 255f, 0f, (float) fadeInValue / 255f));
        Raylib.DrawCircle((int) (x + src.Width * scaleFactor / 2), (int) (y + src.Height * scaleFactor / 2), (float) (src.Width * scaleFactor / 2), new Color(255f, 255f, 255f, (float) fadeOutValue / 255f));
    }

    public override bool IsFinish()
    {
        if (currentTime < 0)
        {
            return false; // Animation hasn't started yet
        }
        return Raylib.GetTime() - currentTime >= duration;
    }

    public override Animation.Animation Clone()
    {
        throw new NotImplementedException();
    }
}