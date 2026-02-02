using System.Numerics;
using Microsoft.VisualBasic;
using Raylib_cs;
using Taiko_CS.Enums;

namespace Taiko_CS.Animation;

public class StopMotionSingleImageAnimation : Animation
{
    public Texture2D texture;
    private List<Rectangle> framesSrc;
    private bool isAnimating  = false;
    private int currentFrameIndex;
    private double progressInterval;
    private double currentTime = -1;
    private float x;
    private float y;
    private float width;
    private float height;
    private double duration;
    private EasingType easingType;
    private Color color = Color.White;
    private BlendMode blendMode = BlendMode.Alpha;
    public StopMotionSingleImageAnimation(Texture2D texture, List<Rectangle> framesSrc, float x, float y, double duration, float width, float height)
    {
        this.texture = texture;
        this.progressInterval = 1.0 / framesSrc.Count;
        this.x = x;
        this.y = y;
        this.framesSrc = framesSrc;
        this.width = width;
        this.height = height;
        this.duration = duration;
    }
    
    public StopMotionSingleImageAnimation(Texture2D texture, List<Rectangle> framesSrc, float x, float y, double duration, float width, float height, Color color)
    {
        this.texture = texture;
        this.progressInterval = 1.0 / framesSrc.Count;
        this.x = x;
        this.y = y;
        this.framesSrc = framesSrc;
        this.width = width;
        this.height = height;
        this.duration = duration;
        this.color = color;
    }
    
    public StopMotionSingleImageAnimation(Texture2D texture, List<Rectangle> framesSrc, float x, float y, double duration, float width, float height, Color color, BlendMode blendMode)
    {
        this.texture = texture;
        this.progressInterval = 1.0 / framesSrc.Count;
        this.x = x;
        this.y = y;
        this.framesSrc = framesSrc;
        this.width = width;
        this.height = height;
        this.duration = duration;
        this.color = color;
        this.blendMode = blendMode;
    }
    
    public StopMotionSingleImageAnimation(Texture2D texture, List<Rectangle> framesSrc, float x, float y, double duration, float width, float height, EasingType easingType)
    {
        this.texture = texture;
        this.progressInterval = 1.0 / framesSrc.Count;
        this.x = x;
        this.y = y;
        this.framesSrc = framesSrc;
        this.width = width;
        this.height = height;
        this.duration = duration;
        this.easingType = easingType;
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
        
        double progress = Math.Min((Raylib.GetTime() - currentTime) / duration, 1.0);
        if (easingType == EasingType.EaseInOutQuin) {}
        {
            progress = Math.Min(EasingFunctions.EaseOutInSine(progress), 1);
        }
        currentFrameIndex = (int) (progress *  (framesSrc.Count - 1));
    }

    public override void StartAnimation()
    {
        isAnimating = true;
    }

    public override void Draw()
    {
        Raylib.BeginBlendMode(blendMode);
        Raylib.DrawTexturePro(texture, framesSrc[currentFrameIndex],  new Rectangle(x, y, width, height), Vector2.Zero, 0, color);
        Raylib.EndBlendMode();
    }

    public override bool IsFinish()
    {
        if (currentTime < 0)
        {
            return false; // Animation hasn't started yet
        }
        return (Raylib.GetTime() - currentTime) >= duration;
    }

    public override Animation Clone()
    {
        throw new NotImplementedException();
    }
}