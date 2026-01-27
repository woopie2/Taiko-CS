using System.Numerics;
using Raylib_cs;

namespace Taiko_CS.Animation;

public class CircleAnimation : Animation
{
    private Texture2D texture;
    private Rectangle src;
    private bool isAnimating = false;
    private double currentDegree = 0;
    private double maxDegree; 
    private double duration;
    private double elapsedTime = 0;
    private double yCenter;
    private double xCenter;
    private double r;
    private double x;
    private double y;
    private double lastTime = -1;
    private double startAngle;
    private float widthFactor;
    private float heightFactor;

    public CircleAnimation(Texture2D texture, Rectangle src, double xStart, double yStart, 
        double xEnd, double yEnd, double arcHeight, double duration)
    {
        this.texture = texture;
        this.src = src;
        this.duration = duration;
        

        double midX = (xStart + xEnd) / 2.0;
        double midY = (yStart + yEnd) / 2.0;
        

        double distance = Math.Sqrt(Math.Pow(xEnd - xStart, 2) + Math.Pow(yEnd - yStart, 2));
        

        this.r = (arcHeight * arcHeight + (distance / 2) * (distance / 2)) / (2 * arcHeight);
        
        double dx = xEnd - xStart;
        double dy = yEnd - yStart;
        double perpX = -dy / distance;
        double perpY = dx / distance;
        
        double centerOffset = r - arcHeight;
        this.xCenter = midX + perpX * centerOffset;
        this.yCenter = midY + perpY * centerOffset;
        
        this.startAngle = Math.Atan2(yStart - yCenter, xStart - xCenter);
        double endAngle = Math.Atan2(yEnd - yCenter, xEnd - xCenter);
        
        this.maxDegree = endAngle - startAngle;
        
        if (this.maxDegree > Math.PI) this.maxDegree -= 2 * Math.PI;
        if (this.maxDegree < -Math.PI) this.maxDegree += 2 * Math.PI;
        
        x = xStart;
        y = yStart;
        widthFactor = 1;
        heightFactor = 1;
    } 
    
    public CircleAnimation(Texture2D texture, Rectangle src, double xStart, double yStart, 
        double xEnd, double yEnd, double arcHeight, double duration, float widthFactor, float heightFactor)
    {
        this.texture = texture;
        this.src = src;
        this.duration = duration;
        

        double midX = (xStart + xEnd) / 2.0;
        double midY = (yStart + yEnd) / 2.0;
        

        double distance = Math.Sqrt(Math.Pow(xEnd - xStart, 2) + Math.Pow(yEnd - yStart, 2));
        

        this.r = (arcHeight * arcHeight + (distance / 2) * (distance / 2)) / (2 * arcHeight);
        
        double dx = xEnd - xStart;
        double dy = yEnd - yStart;
        double perpX = -dy / distance;
        double perpY = dx / distance;
        
        double centerOffset = r - arcHeight;
        this.xCenter = midX + perpX * centerOffset;
        this.yCenter = midY + perpY * centerOffset;
        
        this.startAngle = Math.Atan2(yStart - yCenter, xStart - xCenter);
        double endAngle = Math.Atan2(yEnd - yCenter, xEnd - xCenter);
        
        this.maxDegree = endAngle - startAngle;
        
        if (this.maxDegree > Math.PI) this.maxDegree -= 2 * Math.PI;
        if (this.maxDegree < -Math.PI) this.maxDegree += 2 * Math.PI;
        
        x = xStart;
        y = yStart;
        this.widthFactor = widthFactor;
        this.heightFactor = heightFactor;
    } 
    
    public override void UpdateAnimation()
    {
        if (lastTime < 0)
        {
            lastTime = Raylib.GetTime();
            return;
        }

        double currentTime = Raylib.GetTime();
        double deltaTime = currentTime - lastTime;
        lastTime = currentTime;
        
        elapsedTime += deltaTime;
        
        // Calculate progress (0 to 1)
        double progress = Math.Min(elapsedTime / duration, 1.0);
        
        // Calculate current angle
        double currentAngle = startAngle + maxDegree * progress;
        
        // Update position
        x = xCenter + r * Math.Cos(currentAngle);
        y = yCenter + r * Math.Sin(currentAngle);
    }

    public override void StartAnimation()
    {
        isAnimating = true;
    }

    public override void Draw()
    {
        Raylib.DrawTexturePro(texture, src, new Rectangle((float) x, (float)y, src.Width * widthFactor, src.Height * heightFactor), Vector2.Zero, 0, Color.White);
    }

    public override bool IsFinish()
    {
        return elapsedTime >= duration;
    }

    public override Animation Clone()
    {
        throw new NotImplementedException();
    }
}