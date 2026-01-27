using System.Numerics;
using Microsoft.VisualBasic;
using Raylib_cs;

namespace Taiko_CS.Animation;

public class StopMotionSingleImageAnimation : Animation
{
    private Texture2D texture;
    private List<Rectangle> framesSrc;
    private bool isAnimating  = false;
    private int currentFrameIndex;
    private double timeInterval;
    private double currentTime = -1;
    private float x;
    private float y;
    private float width;
    private float height;
    public StopMotionSingleImageAnimation(Texture2D texture, List<Rectangle> framesSrc, float x, float y, double duration, float width, float height)
    {
        this.texture = texture;
        this.timeInterval = duration / framesSrc.Count;
        this.x = x;
        this.y = y;
        this.framesSrc = framesSrc;
        this.width = width;
        this.height = height;
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
            currentTime = Raylib.GetTime();
            currentFrameIndex++;
            if (currentFrameIndex >= framesSrc.Count)
            {
                currentFrameIndex = framesSrc.Count - 1;
            }
        }
    }

    public override void StartAnimation()
    {
        isAnimating = true;
    }

    public override void Draw()
    {
        Raylib.DrawTexturePro(texture, framesSrc[currentFrameIndex],  new Rectangle(x, y, width, height), Vector2.Zero, 0, Color.White);
    }

    public override bool IsFinish()
    {
        return Raylib.GetTime() - currentTime >= timeInterval && currentFrameIndex >= framesSrc.Count - 1;
    }

    public override Animation Clone()
    {
        throw new NotImplementedException();
    }
}