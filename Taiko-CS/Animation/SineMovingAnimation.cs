using Raylib_cs;

namespace Taiko_CS.Animation;

public class SineMovingAnimation : Animation
{
    private Texture2D texture;
    private int startX;
    private int endX;
    private int currentX;
    private int currentY;
    private int minY;
    private int maxY;
    private double time = -1;
    private double yTime = -1;
    private double timeInterval;
    private double duration;
    private double yCycleDuration;
    private double yTimeInterval;
    private bool isAnimating;
    private bool increaseY = true;
    public SineMovingAnimation(Texture2D texture, int startX, int endX, int minY, int maxY, double duration, double yCycleDuration)
    {
        this.texture = texture;
        this.endX = endX;
        this.startX = startX;
        currentX = startX;
        this.minY = minY;
        this.maxY = maxY;
        currentY = minY;
        timeInterval = duration / (Math.Max(startX, endX) - Math.Min(startX, endX)) ;
        yTimeInterval = yCycleDuration / ((Math.Max(minY, maxY) - Math.Min(minY, maxY)) * 2 - 1);
        this.duration = duration;
        this.yCycleDuration = yCycleDuration;
    }
    
    public SineMovingAnimation(Texture2D texture, int startX, int endX, int minY, int maxY, int currentY, double duration, double yCycleDuration)
    {
        this.texture = texture;
        this.endX = endX;
        this.startX = startX;
        currentX = startX;
        this.minY = minY;
        this.maxY = maxY;
        this.currentY = currentY;
        timeInterval = duration / (Math.Max(startX, endX) - Math.Min(startX, endX)) ;
        yTimeInterval = yCycleDuration / ((Math.Max(minY, maxY) - Math.Min(minY, maxY)) * 2 - 1);
        this.duration = duration;
        this.yCycleDuration = yCycleDuration;
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

        if (time == -1)
        {
            time = Raylib.GetTime();
        }

        if (yTime == -1)
        {
            yTime = Raylib.GetTime();
        }

        if (Raylib.GetTime() - time >= timeInterval)
        {
            currentX--;
            time = Raylib.GetTime();
        }

        if (Raylib.GetTime() - yTime >= yTimeInterval)
        {
            if (currentY >= minY)
            {
                increaseY = true;
            } else if (currentY <= maxY)
            {
                increaseY = false;
            }

            if (increaseY)
            {
                currentY--;
            }
            else
            {
                currentY++;
            }
            yTime = Raylib.GetTime();
        }
    }

    public override void StartAnimation()
    {
        isAnimating = true;
    }

    public override void Draw()
    {
        Raylib.DrawTexture(texture, currentX, currentY, Color.White);
    }

    public override bool IsFinish()
    {
        return currentX == endX;
    }

    public override Animation Clone()
    {
        return new SineMovingAnimation(texture, startX, endX, minY, maxY, currentY, duration, yCycleDuration);
    }
}