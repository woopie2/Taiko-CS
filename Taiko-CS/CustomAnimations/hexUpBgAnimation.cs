using System.Numerics;
using Raylib_cs;
using Taiko_CS.Animation;
using Taiko_CS.Enums;

namespace Taiko_CS.CustomAnimations;

public class hexUpBgAnimation : Animation.Animation
{
    public Texture2D hexBase;
    public Texture2D hexFill;
    public Texture2D hexChara;
    public Rectangle charaSrc;
    public int startX;
    public int endX;
    private int currentX;
    private int charaXOffset;
    private int charaXLocation;
    private int charaYLocation;
    private int hexFillXOffset;
    public int locationID;
    private int charaXOffsetStart;
    private int hexFillXOffsetStart;
    private int minY;
    private int maxY;
    private double yCycleDuration;
    private int currentY;
    private double duration;
    private bool isAnimating;
    private double movementTimeInterval;
    private double currentMovementTime = -1;
    private double charaAppearCurrentTime = -1;
    private double charaDisappearCurrentTime = 0;
    private double charaHexFillAnimationCurrentTime = -1;
    private double charaHexFillAnimationTimeInterval;
    private double startTime = -1;
    private bool hasSwitchingStarted = false;
    private double disappearDelay = 6f;
    private double appearDelay = 3f;
    private double yTime = -1;
    private double yTimeInterval;
    private bool increaseY = true;
    private int id;
    public enum Phase { WaitingToDisappear, Disappearing, WaitingToAppear, Appearing }
    private Phase currentPhase = Phase.WaitingToDisappear;
    private double phaseStartTime = -1;
    
    public hexUpBgAnimation(Texture2D hexBase, Texture2D hexFill, Texture2D hexChara, Rectangle charaSrc, int startX, int endX, int minY, int maxY, double yCycleDuration, double duration, int locationID, int id) {
        this.hexBase = hexBase;
        this.hexFill = hexFill;
        this.hexChara = hexChara;
        this.charaSrc = charaSrc;
        this.startX = startX;
        this.endX = endX;
        currentX = startX;
        this.duration = duration;
        movementTimeInterval = duration / (Math.Max(startX, endX) - Math.Min(startX, endX));
        charaHexFillAnimationTimeInterval = 0.001;
        this.minY = minY;
        this.maxY = maxY;
        this.yCycleDuration = yCycleDuration;
        yTimeInterval = yCycleDuration / ((Math.Max(minY, maxY) - Math.Min(minY, maxY)) * 2 - 1);
        currentY = minY;
        this.locationID = locationID;
        Console.WriteLine($"locationID : {locationID} \n startX : {startX} endX : {endX}");
        switch (locationID)
        {
            case 1:
                charaXLocation = 100;
                charaYLocation = 75;
                hexFillXOffset = 138;
                break;
            case 2:
                charaXLocation = 225;
                charaYLocation = 90;
                hexFillXOffset = -138;
                break;
            case 4:
                charaXLocation = 240;
                charaYLocation = 128;
                hexFillXOffset = -138;
                break;
        }

        this.id = id;
    }
    
    public hexUpBgAnimation(Texture2D hexBase, Texture2D hexFill, Texture2D hexChara, Rectangle charaSrc, int startX, int endX, int minY, int maxY, double yCycleDuration, double duration, int locationID, Phase startPhase, int currentY, int id) {
        this.hexBase = hexBase;
        this.hexFill = hexFill;
        this.hexChara = hexChara;
        this.charaSrc = charaSrc;
        this.startX = startX;
        this.endX = endX;
        currentX = startX;
        this.duration = duration;
        movementTimeInterval = duration / (Math.Max(startX, endX) - Math.Min(startX, endX));
        charaHexFillAnimationTimeInterval = 0.001;
        this.minY = minY;
        this.maxY = maxY;
        this.yCycleDuration = yCycleDuration;
        yTimeInterval = yCycleDuration / ((Math.Max(minY, maxY) - Math.Min(minY, maxY)) * 2 - 1);
        currentY = minY;
        this.locationID = locationID;
        switch (locationID)
        {
            case 1:
                charaXLocation = 100;
                charaYLocation = 75;
                hexFillXOffset = 138;
                break;
            case 2:
                charaXLocation = 225;
                charaYLocation = 90;
                hexFillXOffset = -138;
                break;
            case 4:
                charaXLocation = 240;
                charaYLocation = 128;
                hexFillXOffset = -138;
                break;
        }
        currentPhase = startPhase;
        this.currentY = currentY;
        this.id = id;
    }
    
        public override void UpdateAnimation()
    {
        if (!isAnimating) return;

        double now = Raylib.GetTime();
        if (yTime < 0) yTime = now;
        
        if (now - yTime >= yTimeInterval)
        {
            if (currentY == minY)
            {
                increaseY = true;
            }

            if (currentY == maxY)
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
            yTime = now;
        }
        
        // ---------------- Movement independent of switching ----------------
        if (currentMovementTime < 0) currentMovementTime = now;
        if (now - currentMovementTime >= movementTimeInterval)
        {
            currentX--;
            currentMovementTime = now;
        }

        // Initialize phaseStartTime on first call
        if (phaseStartTime < 0)
            phaseStartTime = now;

        // ---------------- Phase handling ----------------
        switch (currentPhase)
        {
            case Phase.WaitingToDisappear:
                if (now >= phaseStartTime + disappearDelay)
                {
                    charaHexFillAnimationCurrentTime = now;
                    charaXOffsetStart = charaXOffset;
                    hexFillXOffsetStart = hexFillXOffset;
                    currentPhase = Phase.Disappearing;
                }
                break;

            case Phase.Disappearing:
                if (now - charaHexFillAnimationCurrentTime >= charaHexFillAnimationTimeInterval)
                {
                    charaXOffset += locationID == 1 ? 1 : -1;
                    hexFillXOffset += locationID == 1 ? -1 : 1;
                    charaHexFillAnimationCurrentTime = now;
                }

                if (Math.Abs(charaXOffset - charaXOffsetStart) >= 138)
                {
                    currentPhase = Phase.WaitingToAppear;
                    phaseStartTime = now;
                    charaHexFillAnimationCurrentTime = -1;
                }
                break;

            case Phase.WaitingToAppear:
                if (now >= phaseStartTime + appearDelay)
                {
                    charaHexFillAnimationCurrentTime = now;
                    charaXOffsetStart = charaXOffset;
                    hexFillXOffsetStart = hexFillXOffset;
                    currentPhase = Phase.Appearing;
                }
                break;

            case Phase.Appearing:
                if (now - charaHexFillAnimationCurrentTime >= charaHexFillAnimationTimeInterval)
                {
                    charaXOffset += locationID == 1 ? -1 : 1;
                    hexFillXOffset += locationID == 1 ? 1 : -1;
                    charaHexFillAnimationCurrentTime = now;
                }

                if (Math.Abs(charaXOffset - charaXOffsetStart) >= 138)
                {
                    currentPhase = Phase.WaitingToDisappear;
                    phaseStartTime = now;
                    charaHexFillAnimationCurrentTime = -1;
                }
                break;
        }
    }

    public override void StartAnimation()
    {
        isAnimating = true;
    }

    public override void Draw()
    {
        Raylib.DrawTexturePro(hexChara, charaSrc, new Rectangle(currentX + charaXOffset + charaXLocation, currentY + charaYLocation, charaSrc.Width, charaSrc.Height), Vector2.Zero, 0, Color.White);
        Raylib.DrawTexture(hexFill, currentX + hexFillXOffset, currentY, Color.White);
        Raylib.DrawTexture(hexBase, currentX, currentY, Color.White);
    }

    public override bool IsFinish()
    {
        return currentX <= endX;
    }

    public override Animation.Animation Clone()
    {
        return new hexUpBgAnimation(hexBase, hexFill, hexChara, charaSrc, startX, endX, minY, maxY, yCycleDuration, duration, locationID, id);
    }

    public int GetStartX()
    {
        return startX;
    }

    public int GetEndX()
    {
        return endX;
    }

    public int GetLocationId()
    {
        return locationID;
    }

    public Phase GetCurrentPhase()
    {
        return currentPhase;
    }

    public int GetCurrentY()
    {
        return  currentY;
    }

    public int GetId()
    {
        return id;
    }

    public Texture2D GetHexFillTexture()
    {
        return hexFill;
    }

    public Texture2D GetHexBaseTexture()
    {
        return hexBase;
    }

    public Rectangle getCharaSrcRect()
    {
        return charaSrc;
    }
}