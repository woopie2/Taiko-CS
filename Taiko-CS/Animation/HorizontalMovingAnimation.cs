using Raylib_cs;

namespace Taiko_CS.Animation;

public class HorizontalMovingAnimation : Animation
{
    private Texture2D texture;
    private int startX;
    private int endX;
    private int currentX;
    private int y;
    private double duration;
    private double timeInterval;
    private double currentTime = -1;
    private bool isAnimating;
    
    public HorizontalMovingAnimation(Texture2D texture, int startX, int endX, int y, double duration)
    {
        this.texture = texture;
        this.endX = endX;
        this.startX = startX;
        currentX = startX;
        this.y = y;
        timeInterval = duration / (Math.Max(startX, endX) - Math.Min(startX, endX)) ;
        this.duration = duration;
    }
    
    public override void UpdateAnimation()
    {
        if (!isAnimating)
        {
            return;
        }

        if (IsFinish())
        {
            return;
        }

        if (currentTime == -1)
        {
            currentTime = Raylib.GetTime();
        }

        if (Raylib.GetTime() - currentTime >= timeInterval)
        {
            currentX--;
            currentTime = Raylib.GetTime();
        }
    }

    public override void StartAnimation()
    {
       isAnimating = true;
    }

    public override void Draw()
    {
        Raylib.DrawTexture(texture, currentX, y, Color.White);
    }

    public override bool IsFinish()
    {
        return currentX == endX;
    }

    public override HorizontalMovingAnimation Clone()
    {
        return new HorizontalMovingAnimation(texture, startX, endX, y, duration);
    }
}