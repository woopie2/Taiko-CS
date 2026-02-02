namespace Taiko_CS.Animation;

public class EasingFunctions
{
    public static double EaseInCirc(double x) =>  1 - Math.Sqrt(1 - Math.Pow(x, 2));
    public static double EaseInCubic(double x) => x * x * x;

    public static double EaseInOutQuint(double x) => x >= 0.5 ? 16 * x * x * x * x * x : 1 - Math.Pow(-2 * x + 2, 5) / 2;
    public static double EaseOutInQuad(double t)
    {
        if (t < 0.5f)
        {
            // First half: ease out
            t *= 2;
            return -t * (t - 2) / 2;
        }
        else
        {
            // Second half: ease in
            t = (t - 0.5f) * 2;
            return t * t / 2 + 0.5f;
        }
    }
    
    // Cubic EaseOutIn
    public static double EaseOutInCubic(double t)
    {
        if (t < 0.5f)
        {
            // First half: ease out
            t *= 2;
            return (1 - Math.Pow(1 - t, 3)) / 2;
        }
        else
        {
            // Second half: ease in
            t = (t - 0.5f) * 2;
            return Math.Pow(t, 3) / 2 + 0.5f;
        }
    }
    
    // Sine EaseOutIn
    public static double EaseOutInSine(double t)
    {
        if (t < 0.5f)
        {
            // First half: ease out
            return Math.Sin(t * MathF.PI) / 2;
        }
        else
        {
            // Second half: ease in
            return (1 - Math.Cos((t - 0.5f) * MathF.PI)) / 2 + 0.5f;
        }
    }
    
    public static double FastSlowFast(double t)
    {
        // Using cosine for smooth deceleration/acceleration
        // This creates a dip in speed at the middle
        double speedMultiplier = 1 + 0.5 * Math.Cos(2 * Math.PI * t);
        
        // Integrate to get position (this ensures smooth motion)
        return t + 0.5 / (2 * MathF.PI) * Math.Sin(2 * Math.PI * t);
    }
}