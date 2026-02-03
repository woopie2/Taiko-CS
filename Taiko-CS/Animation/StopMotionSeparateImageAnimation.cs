using System.Numerics;
using Raylib_cs;

namespace Taiko_CS.Animation;

public class StopMotionSeparateImageAnimation : Animation
{
    private List<Texture2D> frames;
    private Texture2D currentFrame;
    private int frameIndex = 0;
    private double timeInterval;
    private int x;
    private int y;
    private double currentTime = -1;
    private bool isAnimating = false;
    private double scale = 1;
    public StopMotionSeparateImageAnimation(List<Texture2D> frames, double duration, int x, int y)
    {
        this.frames = frames;
        timeInterval =  duration / frames.Count;
        currentFrame = frames[0];
        this.x = x;
        this.y = y;
    }
    
    public StopMotionSeparateImageAnimation(List<Texture2D> frames, double duration, int x, int y, double scale)
    {
        this.frames = frames;
        timeInterval =  duration / frames.Count;
        currentFrame = frames[0];
        this.x = x;
        this.y = y;
        this.scale = scale;
    }
    
    public override void UpdateAnimation()
    {
        if (!isAnimating)
        {
            return;
        }

        if (currentTime < 0)
        {
            currentTime = Raylib.GetTime();
        }

        if (Raylib.GetTime() - currentTime >= timeInterval)
        {
            frameIndex++;
            if (frameIndex >= frames.Count)
            {
                frameIndex = frames.Count - 1;  // Stay on last frame
                isAnimating = false;  // Stop the animation
            }
            currentFrame = frames[frameIndex];
        }
    }

    public override void StartAnimation()
    {
        isAnimating = true;
    }

    public override void Draw()
    {
        if (currentFrame.Id == 0)  // Unloaded textures have id = 0
        {
            return;
        }
        if (!isAnimating)
        {
            return;
        }
        Raylib.DrawTextureEx(currentFrame, new Vector2(x, y), 0, (float)scale, Color.White);
    }

    public override bool IsFinish()
    {
        return Raylib.GetTime() - currentTime >= timeInterval && frameIndex ==  frames.Count - 1;
    }

    public override Animation Clone()
    {
        return new  StopMotionSeparateImageAnimation(frames, timeInterval * frames.Count, x, y);
    }
}