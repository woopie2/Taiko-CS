using System.Numerics;
using Raylib_cs;
using Taiko_CS.Enums;
using Taiko_CS.Animation;
using Taiko_CS.Chart;
using Taiko_CS.CustomAnimations;

namespace Taiko_CS.screens;

public class SongPlaying : Screen
{
    private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    private Dictionary<string, Sound> sounds = new Dictionary<string, Sound>();
    private List<Sound> playingSounds = new List<Sound>();
    private bool isRightDonPressed;
    private bool isLeftDonPressed;
    private bool isRightKaPressed;
    private bool isLeftKaPressed;
    private Difficulty difficulty;
    private List<Animation.Animation> runningForegroundAnimations = new List<Animation.Animation>();
    private List<Animation.Animation> runningBackgroundAnimations = new List<Animation.Animation>();
    private List<Animation.Animation> runningLaneAnimation = new List<Animation.Animation>();
    private Dictionary<int, int> hexAnimationSprites = new Dictionary<int, int>();
    private bool areAnimationsInitialized = false;
    private int gaugePercent = 0;
    bool hexAnimationsRestarted = true;
    private ChartData chartData;
    private Music song;
    private List<Measure> measuresOnScreen = new List<Measure>();
    private int nextMeasureIndex = 0;
    private bool isLastHitRight = false;
    private int lastRemovedNoteIndex = -1;
    private Measure? lastCheckedMeasure = null;
    private double lastDonHitSoundTime = double.MinValue;
    private double lastKaHitSoundTime = double.MinValue;
    private double lastHitTime = -1;
    private RollType currentRollType;
    private double lastRollHitTime = -1;
    private double greatTimeInterval;
    private double okTimeInterval;
    private double badTimeInterval;
    private List<Texture2D> greatHitAnimFrames;
    private List<Texture2D> goodHitAnimFrames;

    public SongPlaying(Difficulty difficulty, ChartData chartData)
    {
        this.difficulty = difficulty;
        this.chartData = chartData;
        LoadTextures();
        LoadSounds();
        Raylib.PlayMusicStream(song);
        Raylib.SetMusicVolume(song, 0.5f);
        Raylib.SeekMusicStream(song, 0);
        if (difficulty is Difficulty.EASY or Difficulty.NORMAL)
        {
            greatTimeInterval = 41.6 / 1000;
            okTimeInterval = 108.3 / 1000;
            badTimeInterval = 125.0 / 1000;
        } else if (difficulty is Difficulty.HARD or Difficulty.ONI or Difficulty.URA)
        {
            greatTimeInterval = 25.0 / 1000;
            okTimeInterval = 75.0 / 1000;
            badTimeInterval = 108.3 / 1000;
        }
    }

    public override void LoadTextures()
    {
        LoadTexture("LeftSideBackground", "6_Taiko/1P_Background_Tokkun.png");
        LoadTexture("PlayingZoneFrame", "6_Taiko/1P_Frame.png");
        LoadTexture("TaikoDon", "6_Taiko/Don.png");
        LoadTexture("TaikoKa", "6_Taiko/Ka.png");
        LoadTexture("Taiko", "6_Taiko/Base.png");
        LoadTexture("LaneUpBackground", "12_Lane/Background_Main.png");
        LoadTexture("LaneDownBackground", "12_Lane/Background_Sub.png");
        LoadTexture("UpBackground", "5_Background/Normal/Up/0/1st_1P.png");
        LoadTexture("Easy", "4_CourseSymbol/Easy.png");
        LoadTexture("Normal", "4_CourseSymbol/Normal.png");
        LoadTexture("Hard", "4_CourseSymbol/Hard.png");
        LoadTexture("Oni", "4_CourseSymbol/Oni.png");
        LoadTexture("Ura", "4_CourseSymbol/Edit.png");
        LoadTexture("DownBackground", "5_Background/Normal/Down/0/0.png");
        LoadTexture("Footer", "8_Footer/0.png");
        LoadTexture("DownBackgroundLights", "5_Background/Normal/Down/0/1.png");
        LoadTexture("Flower", "5_Background/Normal/Up/0/2nd_1P.png");
        LoadTexture("GaugeBase", "7_Gauge/1P_Base.png");
        LoadTexture("GaugeFilled", "7_Gauge/1P.png");
        LoadTexture("GaugeUpdate", "7_Gauge/Gauge_Update.png");
        LoadTexture("Hex2", "5_Background/Normal/Up/0/3rd_2_0_1P.png");
        LoadTexture("Hex2Fill", "5_Background/Normal/Up/0/3rd_2_1_1P.png");
        LoadTexture("HexChara", "5_Background/Normal/Up/0/Chara.png");
        LoadTexture("Hex1", "5_Background/Normal/Up/0/3rd_3_0_1P.png");
        LoadTexture("Hex1Fill", "5_Background/Normal/Up/0/3rd_3_1_1P.png");
        LoadTexture("Hex4", "5_Background/Normal/Up/0/3rd_1_0_1P.png");
        LoadTexture("Hex4Fill", "5_Background/Normal/Up/0/3rd_1_1_1P.png");
        LoadTexture("Notes", "Notes.png");
        LoadTexture("MeasureBar", "Bar.png");
        greatHitAnimFrames = LoadFrames("./ressources/Graphics/5_Game/10_Effects/Hit/Great", 14);
        goodHitAnimFrames = LoadFrames("./ressources/Graphics/5_Game/10_Effects/Hit/Good", 14);
    }

    public override void LoadSounds()
    {
        LoadSound("Don", "Taiko/dong.wav", 0.25f);
        LoadSound("Ka", "Taiko/ka.wav", 0.25f);
        song = Raylib.LoadMusicStream($"{chartData.GetField("LOCATION")}/{chartData.GetField("WAVE")}");
    }

    private void LoadTexture(string textureName, string texturePath)
    {
        string gameTextureFolder = "./ressources/Graphics/5_Game";
        Image image = Raylib.LoadImage($"{gameTextureFolder}/{texturePath}");
        Texture2D texture = Raylib.LoadTextureFromImage(image);
        textures.Add(textureName, texture);
    }

    private void LoadSound(string soundName, string soundPath)
    {
        string gameSoundFolder = "./ressources/Sounds";
        Sound sound = Raylib.LoadSound($"{gameSoundFolder}/{soundPath}");
        sounds.Add(soundName, sound);
    }
    
    private void LoadSound(string soundName, string soundPath, float volume)
    {
        string gameSoundFolder = "./ressources/Sounds";
        Sound sound = Raylib.LoadSound($"{gameSoundFolder}/{soundPath}");
        Raylib.SetSoundVolume(sound, volume);
        sounds.Add(soundName, sound);
    }

    public override void UnloadTextures()
    {
        foreach (KeyValuePair<string, Texture2D> texture in textures)
        {
            Raylib.UnloadTexture(texture.Value);
        }
        
        foreach (Texture2D texture in greatHitAnimFrames)
        {
            Raylib.UnloadTexture(texture);
        }

        foreach (Texture2D texture in goodHitAnimFrames)
        {
            Raylib.UnloadTexture(texture);
        }
    }

    public override void UnloadSounds()
    {
        foreach (KeyValuePair<string, Sound> sound in sounds)
        {
            Raylib.UnloadSound(sound.Value);
        }
        Raylib.UnloadMusicStream(song);
    }

    private void DrawForegroundAnimations()
    {
        foreach (Animation.Animation animation in runningForegroundAnimations)
        {
            animation.Draw();
        }
    }

    private void DrawBackgroundAnimations()
    {
        foreach (Animation.Animation animation in runningBackgroundAnimations)
        {
            animation.Draw();
        }
    }

    private void DrawLaneAnimations()
    {
        foreach (Animation.Animation animation in runningLaneAnimation)
        {
            animation.Draw();
        }
    }
    
    private void UpdateForegroundAnimations()
    {
        List<Animation.Animation> finishedAnimations = new List<Animation.Animation>();
        foreach (Animation.Animation animation in runningForegroundAnimations)
        {
            if (animation.IsFinish())
            {
                finishedAnimations.Add(animation);
                continue;
            }

            animation.UpdateAnimation();

        }

        foreach (Animation.Animation animation in finishedAnimations)
        {
            runningForegroundAnimations.Remove(animation);
        }
    }

    private void UpdateLaneAnimations()
    {
        List<Animation.Animation> finishedAnimations = new List<Animation.Animation>();
        foreach (Animation.Animation animation in runningLaneAnimation)
        {
            if (animation.IsFinish())
            {
                finishedAnimations.Add(animation);
                continue;
            }
            
            animation.UpdateAnimation();
        }

        foreach (Animation.Animation animation in finishedAnimations)
        {
            runningLaneAnimation.Remove(animation);
        }
    }
    private void UpdateBackgroundAnimations()
    {
        List<Animation.Animation> finishedAnimations = new();
        List<Animation.Animation> restartedAnimations = new();

        foreach (var animation in runningBackgroundAnimations)
        {
            if (animation.IsFinish())
                finishedAnimations.Add(animation);
            else
                animation.UpdateAnimation();
        }
        var finishedHexes = finishedAnimations.OfType<hexUpBgAnimation>().ToList();
        foreach (var anim in finishedAnimations.Except(finishedHexes))
        {
            var restarted = anim.Clone();
            restarted.StartAnimation();
            restartedAnimations.Add(restarted);
        }    
        
        foreach (var oldAnim in finishedHexes.OrderBy(a => a.GetId()))
        {
            int oldAnimEndX = oldAnim.GetEndX();
            int locationID = oldAnim.GetLocationId();
            int startPosFactor = oldAnimEndX / 492;
            int endPosFactor = oldAnimEndX / 492 - 1;
                
            if (startPosFactor == -1 && endPosFactor == -2)
            {
                startPosFactor = 4;
                endPosFactor = 3;
            }
                
            int startX = 492 * startPosFactor;
            int endX = 492 * endPosFactor;
                
            var restarted = new hexUpBgAnimation(
                oldAnim.GetHexBaseTexture(),
                oldAnim.GetHexFillTexture(),
                textures["HexChara"],
                oldAnim.getCharaSrcRect(),
                startX,
                endX,
                0,
                -20,
                2,
                10,
                locationID,
                oldAnim.GetCurrentPhase(),
                oldAnim.GetCurrentY(),
                oldAnim.GetId(),
                oldAnim.GetCharaXOffset(),
                oldAnim.GetHexFillXOffset()
            );

            restarted.StartAnimation();
            restartedAnimations.Add(restarted);
        }


        runningBackgroundAnimations.AddRange(restartedAnimations);
        
        foreach (var anim in finishedAnimations)
            runningBackgroundAnimations.Remove(anim);
    }


    private void DrawPlayingZone(int x, int y)
    {
        Raylib.DrawTexture(textures["PlayingZoneFrame"], x + textures["LeftSideBackground"].Width - 1, y, Color.White);
        Raylib.DrawTexturePro(textures["LaneUpBackground"],
            new Rectangle(0, 0, textures["LaneUpBackground"].Width, textures["LaneUpBackground"].Height),
            new Rectangle(x + textures["LeftSideBackground"].Width, y + 83, textures["PlayingZoneFrame"].Width,
                textures["LaneUpBackground"].Height), new Vector2(0, 0), 0, Color.White);
        Raylib.DrawTexturePro(textures["LaneDownBackground"],
            new Rectangle(0, 0, textures["LaneDownBackground"].Width, textures["LaneDownBackground"].Height),
            new Rectangle(x + textures["LeftSideBackground"].Width, y + 83 + textures["LaneUpBackground"].Height + 6,
                textures["PlayingZoneFrame"].Width, textures["LaneDownBackground"].Height), new Vector2(0, 0), 0,
            Color.White);
        DrawHitZone(x + textures["LeftSideBackground"].Width + 10, y + 83 + 40);
        DrawLaneAnimations();
        DrawMeasures(276 - 72);
        Raylib.DrawTexture(textures["LeftSideBackground"], x, y + 72, Color.White);
        DrawTaiko(isLeftKaPressed, isRightKaPressed, isLeftDonPressed, isRightDonPressed, x + 499 - 180 - 10,
            y + textures["Taiko"].Height / 2);
        DrawDifficultyIcon(x + 10, y);
        DrawGauge(x + textures["LeftSideBackground"].Width - 1 + 240, y + 12, gaugePercent);
    }
    
    private void DrawHitZone(int x, int y)
    {
        Rectangle src = new Rectangle(11, 12, 105, 105);
        Raylib.DrawTexturePro(textures["Notes"], src, new Rectangle(x, y - src.Height / 2 * 1.3f + 50, src.Width * 1.3f, src.Height * 1.3f), Vector2.Zero, 0, Color.White);
    }
    
    private void DrawDifficultyIcon(int x, int y)
    {
        Texture2D difficultyIcon = new Texture2D();
        switch (difficulty)
        {
            case Difficulty.EASY:
                difficultyIcon = textures["Easy"];
                break;
            case Difficulty.NORMAL:
                difficultyIcon = textures["Normal"];
                break;
            case Difficulty.HARD:
                difficultyIcon = textures["Hard"];
                break;
            case Difficulty.ONI:
                difficultyIcon = textures["Oni"];
                break;
            case Difficulty.URA:
                difficultyIcon = textures["Ura"];
                break;
        }

        Raylib.DrawTexture(difficultyIcon, x, y + difficultyIcon.Height / 2 + 100, Color.White);
    }

    private void DrawGauge(int x, int y, int percent)
    {
        // Spritesheet textures
        Texture2D baseSheet = textures["GaugeBase"]; // full spritesheet
        Texture2D fillSheet = textures["GaugeFilled"]; // full spritesheet (may be same sheet)

        // measurements you found in Paint.NET
        int gaugeSrcX = 0; // left pixel within sprite sheet for the gauge graphic
        int gaugeSrcY = 0; // top pixel where the 43px-tall gauge begins (change this!)
        int gaugeSrcHeight = 43; // the gauge height you measured
        int gaugeSrcFullWidth = 695; // the full width of the gauge image in the sheet

        // destination size you want on-screen
        float destBaseWidth = 1050f;
        float scaleX = destBaseWidth / (float)gaugeSrcFullWidth;
        float destHeight = gaugeSrcHeight * scaleX;

        // compute how many source pixels to sample for the fill (float division)
        float fillSrcWidth = 1f + (gaugeSrcFullWidth - 1f) * (percent / 50f);
        fillSrcWidth = Math.Clamp(fillSrcWidth, 0f, gaugeSrcFullWidth);

        // Draw the base (cropped to the gauge region inside the sheet)
        Raylib.DrawTexturePro(
            baseSheet,
            new Rectangle(gaugeSrcX, gaugeSrcY, gaugeSrcFullWidth, gaugeSrcHeight), // source: cropped gauge frame
            new Rectangle(x, y, destBaseWidth, destHeight), // dest: scaled
            Vector2.Zero,
            0f,
            Color.White
        );

        // Draw the filled portion — *only* sample the left slice of the gauge region
        Raylib.DrawTexturePro(
            fillSheet,
            new Rectangle(gaugeSrcX, gaugeSrcY, fillSrcWidth, gaugeSrcHeight), // source: cropped left slice
            new Rectangle(x, y, fillSrcWidth * scaleX, destHeight), // dest: scaled the same
            Vector2.Zero,
            0f,
            Color.White
        );
    }


    private void DrawTaiko(bool leftKaPressed, bool rightKaPressed, bool leftDonPressed, bool rightDonPressed, int x,
        int y)
    {
        double taikoHitViewierTime = 0.1875;
        Raylib.DrawTexture(textures["Taiko"], x, y, Color.White);
        if (leftDonPressed)
        {
            Texture2D taikoDon = textures["TaikoDon"];
            int width = taikoDon.Width / 2;
            int height = taikoDon.Height;
            Animation.Animation leftDonAnimation = new InstantDisappearAnimation(taikoDon, taikoHitViewierTime,
                new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height));
            leftDonAnimation.StartAnimation();
            runningForegroundAnimations.Add(leftDonAnimation);
        }

        if (rightDonPressed)
        {
            Texture2D taikoDon = textures["TaikoDon"];
            int width = taikoDon.Width / 2;
            int height = taikoDon.Height;

            Animation.Animation rightDonAnimation = new InstantDisappearAnimation(taikoDon, taikoHitViewierTime,
                new Rectangle(width, 0, width, height), new Rectangle(x + width, y, width, height));
            rightDonAnimation.StartAnimation();
            runningForegroundAnimations.Add(rightDonAnimation);
        }

        if (leftKaPressed)
        {
            Texture2D taikoKa = textures["TaikoKa"];
            int width = taikoKa.Width / 2;
            int height = taikoKa.Height;
            Animation.Animation leftKaAnimation = new InstantDisappearAnimation(taikoKa, taikoHitViewierTime,
                new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height));
            leftKaAnimation.StartAnimation();
            runningForegroundAnimations.Add(leftKaAnimation);
        }

        if (rightKaPressed)
        {
            Texture2D taikoKa = textures["TaikoKa"];
            int width = taikoKa.Width / 2;
            int height = taikoKa.Height;
            Animation.Animation rightKaAnimation = new InstantDisappearAnimation(taikoKa, taikoHitViewierTime,
                new Rectangle(width, 0, width, height), new Rectangle(x + width, y, width, height));
            rightKaAnimation.StartAnimation();
            runningForegroundAnimations.Add(rightKaAnimation);
        }
    }

    private void InitliazeAnimations()
    {
        int upBackgroundScrollingTime = 10;
        double lightCycleDuration = 0.25;
        double flowerYCycleDuration = 2;
        Animation.Animation upBackgroundScrolling0 =
            new HorizontalMovingAnimation(textures["UpBackground"], 492 * 4, 492 * 3, 0, upBackgroundScrollingTime);
        runningBackgroundAnimations.Add(upBackgroundScrolling0);
        upBackgroundScrolling0.StartAnimation();

        Animation.Animation upBackgroundScrolling1 = new HorizontalMovingAnimation(textures["UpBackground"], 492 * 0,
            -1 * 492, 0, upBackgroundScrollingTime);
        runningBackgroundAnimations.Add(upBackgroundScrolling1);
        upBackgroundScrolling1.StartAnimation();

        Animation.Animation upBackgroundScrolling2 =
            new HorizontalMovingAnimation(textures["UpBackground"], 492 * 1, 0 * 492, 0, upBackgroundScrollingTime);
        runningBackgroundAnimations.Add(upBackgroundScrolling2);
        upBackgroundScrolling2.StartAnimation();

        Animation.Animation upBackgroundScrolling3 =
            new HorizontalMovingAnimation(textures["UpBackground"], 492 * 2, 1 * 492, 0, upBackgroundScrollingTime);
        runningBackgroundAnimations.Add(upBackgroundScrolling3);
        upBackgroundScrolling3.StartAnimation();

        Animation.Animation upBackgroundScrolling4 =
            new HorizontalMovingAnimation(textures["UpBackground"], 492 * 3, 2 * 492, 0, upBackgroundScrollingTime);
        runningBackgroundAnimations.Add(upBackgroundScrolling4);
        upBackgroundScrolling4.StartAnimation();

        Animation.Animation upBackgroundFlower0 = new SineMovingAnimation(textures["Flower"], 492 * 3, 492 * 2, 0, -20,
            upBackgroundScrollingTime, flowerYCycleDuration);
        runningBackgroundAnimations.Add(upBackgroundFlower0);
        upBackgroundFlower0.StartAnimation();

        Animation.Animation upBackgroundFlower1 = new SineMovingAnimation(textures["Flower"], 492 * 2, 492 * 1, 0, -20,
            upBackgroundScrollingTime, flowerYCycleDuration);
        runningBackgroundAnimations.Add(upBackgroundFlower1);
        upBackgroundFlower1.StartAnimation();

        Animation.Animation upBackgroundFlower2 = new SineMovingAnimation(textures["Flower"], 492 * 1, 492 * 0, 0, -20,
            upBackgroundScrollingTime, flowerYCycleDuration);
        runningBackgroundAnimations.Add(upBackgroundFlower2);
        upBackgroundFlower2.StartAnimation();

        Animation.Animation upBackgroundFlower3 = new SineMovingAnimation(textures["Flower"], 492 * 0, 492 * -1, 0, -20,
            upBackgroundScrollingTime, flowerYCycleDuration);
        runningBackgroundAnimations.Add(upBackgroundFlower3);
        upBackgroundFlower3.StartAnimation();

        Animation.Animation upBackgroundFlower4 = new SineMovingAnimation(textures["Flower"], 492 * 4, 492 * 3, 0, -20,
            upBackgroundScrollingTime, flowerYCycleDuration);
        runningBackgroundAnimations.Add(upBackgroundFlower4);
        upBackgroundFlower4.StartAnimation();

        Animation.Animation downBackgroundLights = new LoopingFadeInFadeOutAnimation(textures["DownBackgroundLights"],
            new Rectangle(0, 0, textures["DownBackgroundLights"].Width, textures["DownBackgroundLights"].Height),
            new Rectangle(0, 276 - 72 + 336, 1920, (276 - 72 + 336) - (1080 - 66)), lightCycleDuration, 110, 125);
        runningBackgroundAnimations.Add(downBackgroundLights);
        downBackgroundLights.StartAnimation();

        Animation.Animation upBackgroundHex1 = new hexUpBgAnimation(
            textures["Hex4"],
            textures["Hex4Fill"],
            textures["HexChara"],
            new Rectangle(56, 13, 90, 252),
            492 * 1,
            492 * 0,
            0,
            -20,
            flowerYCycleDuration,
            upBackgroundScrollingTime,
            4,
            0);
        
        runningBackgroundAnimations.Add(upBackgroundHex1);
        hexAnimationSprites.Add(0, 4);
        upBackgroundHex1.StartAnimation();
        
        Animation.Animation upBackgroundHex2 = new hexUpBgAnimation(
            textures["Hex2"],
            textures["Hex2Fill"],
            textures["HexChara"],
            new Rectangle(56, 13, 90, 252),
            492 * 2,
            492 * 1,
            0,
            -20,
            flowerYCycleDuration,
            upBackgroundScrollingTime,
            2,
            1);
        
        runningBackgroundAnimations.Add(upBackgroundHex2);
        hexAnimationSprites.Add(1, 2);
        upBackgroundHex2.StartAnimation();
        
        Animation.Animation upBackgroundHex3 = new hexUpBgAnimation(
            textures["Hex1"],
            textures["Hex1Fill"],
            textures["HexChara"],
            new Rectangle(273, 29, 99, 146),
            492 * 3,
            492 * 2,
            0,
            -20,
            flowerYCycleDuration,
            upBackgroundScrollingTime,
            1,
            2);
        
        runningBackgroundAnimations.Add(upBackgroundHex3);
        hexAnimationSprites.Add(2, 1);
        upBackgroundHex3.StartAnimation();
        
        Animation.Animation upBackgroundHex4 = new hexUpBgAnimation(
            textures["Hex4"],
            textures["Hex4Fill"],
            textures["HexChara"],
            new Rectangle(739, 58, 133, 140),
            492 * 4,
            492 * 3,
            0,
            -20,
            flowerYCycleDuration,
            upBackgroundScrollingTime,
            4,
            3);
        
        runningBackgroundAnimations.Add(upBackgroundHex4);
        hexAnimationSprites.Add(3, 4);
        upBackgroundHex4.StartAnimation();
        
        
        Animation.Animation upBackgroundHex5 = new hexUpBgAnimation(
            textures["Hex2"],
            textures["Hex2Fill"],
            textures["HexChara"],
            new Rectangle(56, 13, 90, 252),
            492 * 0,
            492 * -1,
            0,
            -20,
            flowerYCycleDuration,
            upBackgroundScrollingTime,
            2,
            4);
        
        runningBackgroundAnimations.Add(upBackgroundHex5);
        hexAnimationSprites.Add(4, 2);
        upBackgroundHex5.StartAnimation();
        
        areAnimationsInitialized = true;
    }

    private void DrawBackground(int y)
    {
        Raylib.DrawTexturePro(textures["DownBackground"],
            new Rectangle(0, 0, textures["DownBackground"].Width, textures["DownBackground"].Height),
            new Rectangle(0, y, 1920, y - (1080 - 66)), new Vector2(0, 0), 0, Color.White);
    }

    private void DrawFooter(int y)
    {
        Raylib.DrawTexture(textures["Footer"], 0, y, Color.White);
    }

    private void DrawMeasures(int y)
    {
        List<Measure> measuresToDraw = new List<Measure>(measuresOnScreen);
        measuresToDraw.Sort((m, m2) => m.GetNotes()[0].ScrollSpeed.CompareTo(m2.GetNotes()[0].ScrollSpeed));
        foreach (Measure measure in measuresToDraw)
        {
            measure.Draw(y + 83 + 40, textures["Notes"], Raylib.GetMusicTimePlayed(song), textures["MeasureBar"]);
        }
    }

    private Measure? GetNearestMeasure()
    {
        if (measuresOnScreen.Count == 0)
        {
            return null;
        }
        Measure min = measuresOnScreen[0];
        foreach (Measure m in measuresOnScreen)
        {
            if (m.GetMeasureStartX(Raylib.GetMusicTimePlayed(song)) <
                min.GetMeasureStartX(Raylib.GetMusicTimePlayed(song)))
            {
                min = m;
            }
        }

        return min;
    }

    private Note? GetNearestNoteFromHitPoint(Measure measure)
    {
        if (measure.activeNotes.Count == 0)
        {
            return null;
        }
        Note nearest = measure.activeNotes[0];
        if (Measure.HIT_X - nearest.X < 0)
        {
            return null;
        }
        foreach (Note note in measure.activeNotes)
        {
            if (note.noteType is NoteType.None)
            {
                continue;
            }
            if (Measure.HIT_X - note.X < Measure.HIT_X - nearest.X && Measure.HIT_X - note.X > 0)
            {
                nearest = note;
            }
        }

        return nearest;
    }
    
    private void UpdateCurrentRollType()
    {
        Measure measure =  GetNearestMeasure();
        if (measure == null)
        {
            return;
        }
        Note note = GetNearestNoteFromHitPoint(measure);
        if (note == null)
        {
            return;
        }
        if (note.rollType != RollType.NONE && note.noteType != NoteType.EndOfRoll)
        {
            currentRollType = note.rollType;
            return;
        }
        if (note.noteType == NoteType.EndOfRoll)
        {
            currentRollType = RollType.NONE;
        }
    }
    
    private void UpdateAutoPlayHit(List<Note> notes, Note? noteToHit)
    {
        if (currentRollType != RollType.NONE)
        {
            if (lastRollHitTime < 0)
            {
                lastRollHitTime = Raylib.GetTime();
            } else if (Raylib.GetTime() - lastRollHitTime  >= 0.05)
            {
                if (isLastHitRight)
                {
                    HitLeftDon(); 
                } else
                {
                    HitRightDon();
                }
                isLastHitRight = !isLastHitRight;
                lastRollHitTime = Raylib.GetTime();
            }
        }
        foreach (Note note in notes)
        {
            Measure measure = GetNearestMeasure();
            if (measure == null)
            {
                return;
            }
            if (lastHitTime > 0 &&  currentRollType == RollType.NONE && note.timeInMeasure + measure.songStartTime - lastHitTime >= 0.2)
            {
                isLastHitRight = false;
            }
            switch (note.noteType)
            {
                case NoteType.Don:
                    if (isLastHitRight)
                    {
                        HitLeftDon();
                    }
                    else
                    {
                        HitRightDon();
                    }
                    HitNote(noteToHit);
                    lastHitTime = note.timeInMeasure + measure.songStartTime;
                    isLastHitRight = !isLastHitRight;
                    
                    break;
                case NoteType.Ka:
                    if (isLastHitRight)
                    {
                        HitLeftKa();
                    }
                    else
                    {
                        HitRightKa();
                    }
                    HitNote(noteToHit);
                    lastHitTime = note.timeInMeasure + measure.songStartTime;
                    isLastHitRight = !isLastHitRight;
                    break;
                case NoteType.BigDon:
                    HitLeftDon();
                    HitRightDon();
                    lastHitTime = note.timeInMeasure + measure.songStartTime;
                    isLastHitRight = !isLastHitRight;
                    HitNote(noteToHit);
                    break;
                case NoteType.BigKa:
                    HitLeftKa();
                    HitRightKa();
                    lastHitTime = note.timeInMeasure + measure.songStartTime;
                    isLastHitRight = !isLastHitRight;
                    HitNote(noteToHit);
                    break;
            }
        }
            
            
    }
    
    public override void Draw()
    {
        if (!areAnimationsInitialized)
        {
            InitliazeAnimations();
        }
        Raylib.UpdateMusicStream(song);
        Raylib.ClearBackground(Color.White);
        DrawBackground(276 - 72 + 336);
        DrawFooter(1080 - 66);
        DrawBackgroundAnimations();
        UpdateForegroundAnimations();
        UpdateLaneAnimations();
        DrawPlayingZone(0, 276 - 72);
        DrawForegroundAnimations();
        UpdateBackgroundAnimations();
    }

    private void HitNote(Note? noteToHit)
    {

        if (noteToHit == null)
        {
            return;
        }
        if (GetNearestMeasure().activeNotes.Count == 0)
        {
            return;
        }
        double songTime = Raylib.GetMusicTimePlayed(song);
        Measure nearestMeasure = GetNearestMeasure();
        if (nearestMeasure == null)
        {
            return;
        }
        Console.WriteLine(Math.Abs(songTime - (noteToHit.timeInMeasure + nearestMeasure.songStartTime)));
        double noteGreatTimeInterval = greatTimeInterval + 60 / noteToHit.BPM * 0.001;
        double noteOkTimeInterval = okTimeInterval + 60 / noteToHit.BPM * 0.001;
        double noteBadTimeInterval = badTimeInterval + 60 / noteToHit.BPM * 0.001;
        if (Math.Abs(songTime - (noteToHit.timeInMeasure + nearestMeasure.songStartTime)) <= noteGreatTimeInterval)
        {
            StopMotionSeparateImageAnimation animation = new StopMotionSeparateImageAnimation(greatHitAnimFrames, 1.5, 385, 180);
            runningLaneAnimation.Add(animation);
            animation.StartAnimation();

        } else if (Math.Abs(songTime - (noteToHit.timeInMeasure + nearestMeasure.songStartTime)) <= noteOkTimeInterval)
        {
            StopMotionSeparateImageAnimation animation = new StopMotionSeparateImageAnimation(goodHitAnimFrames, 1.5, 385, 180);
            runningLaneAnimation.Add(animation);
            animation.StartAnimation();
        } else if (Math.Abs(songTime - (noteToHit.timeInMeasure + nearestMeasure.songStartTime)) <= noteBadTimeInterval)
        {
            
        }
    }
    
    private void HitRightDon()
    {
        isRightDonPressed = true;
        Sound sound = Raylib.LoadSoundAlias(sounds["Don"]);
        playingSounds.Add(sound);
        if (Raylib.GetTime() - lastDonHitSoundTime >= 0.001)
        {
            Raylib.PlaySound(sound);
            lastDonHitSoundTime = Raylib.GetTime();
        }
    }

    private void HitLeftDon()
    {
        isLeftDonPressed = true;
        Sound sound = Raylib.LoadSoundAlias(sounds["Don"]);
        playingSounds.Add(sound);
        if (Raylib.GetTime() - lastDonHitSoundTime >= 0.001)
        {
            Raylib.PlaySound(sound);
            lastDonHitSoundTime = Raylib.GetTime();
        }
    }

    private void HitLeftKa()
    {
        isLeftKaPressed = true;
        Sound sound = Raylib.LoadSoundAlias(sounds["Ka"]);
        playingSounds.Add(sound);
        if (Raylib.GetTime() - lastKaHitSoundTime >= 0.001)
        {
            Raylib.PlaySound(sound);
            lastKaHitSoundTime = Raylib.GetTime();
        }
    }

    private void HitRightKa()
    {
        isRightKaPressed = true;
        Sound sound = Raylib.LoadSoundAlias(sounds["Ka"]);
        playingSounds.Add(sound);
        if (Raylib.GetTime() - lastKaHitSoundTime >= 0.001)
        {
            Raylib.PlaySound(sound);
            lastKaHitSoundTime = Raylib.GetTime();
        }
    }
    
    public override void HandleInput()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.F))
        {
            HitLeftDon();
        }
        else
        {
            isLeftDonPressed = false;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.J))
        {
            HitRightDon();
        }
        else
        {
            isRightDonPressed = false;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.D))
        {
            HitLeftKa();
        }
        else
        {
            isLeftKaPressed = false;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.K))
        {
           HitRightKa();
        }
        else
        {
            isRightKaPressed = false;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KpSubtract))
        {
            gaugePercent--;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KpAdd))
        {
            gaugePercent++;
        }
    }

    public override void HandleAudio()
    {
        for (int i = playingSounds.Count - 1; i >= 0; i--)
        {
            if (!Raylib.IsSoundPlaying(playingSounds[i]))
            {
                Raylib.UnloadSoundAlias(playingSounds[i]);
                playingSounds.RemoveAt(i);
            }
        }
    }

    public override void HandleOthers()
    {
        double songTime = Raylib.GetMusicTimePlayed(song);
    
        // Check ALL measures that need to be added
        for (int i = nextMeasureIndex; i < chartData.measures.Count; i++)
        {
            Measure measure = chartData.measures[i];
            if (measuresOnScreen.Contains(measure))
                continue;
            // Add measure if its spawn time has passed
            if (measure.GetMeasureStartX((float) songTime) <= Measure.SPAWN_X )
            {
                measuresOnScreen.Add(measure);
            }
        }
    
        // Update nextMeasureIndex to skip measures we've already checked
        while (nextMeasureIndex < chartData.measures.Count && 
               measuresOnScreen.Contains(chartData.measures[nextMeasureIndex]))
        {
            nextMeasureIndex++;
        }
        
        // Update and cleanup
        List<Measure> finishedMeasures = new List<Measure>();
        List<Note> removedNotes = new List<Note>();
        Measure nearestMeasure = GetNearestMeasure();
        if (nearestMeasure == null)
        {
            return;
        }
        Note? noteToHit = null;
        if (nearestMeasure.activeNotes.Count > 0)
        {
            noteToHit =  nearestMeasure.activeNotes[0];
        }
        foreach (Measure measure in measuresOnScreen)
        {
            measure.Update(songTime, chartData.measures.IndexOf(measure), ref removedNotes);
            if (measure.IsMeasureFinished((float) songTime))
                finishedMeasures.Add(measure);
        }

        foreach (var measure in finishedMeasures)
            measuresOnScreen.Remove(measure);
        UpdateCurrentRollType();
        UpdateAutoPlayHit(removedNotes, noteToHit);
    }

    private List<Texture2D> LoadFrames(string folderPath, int frameCount)
    {
        List<Texture2D> frames = new List<Texture2D>();
        for (int i = 0; i <= frameCount; i++)
        {
            frames.Add(Raylib.LoadTexture($"{folderPath}/{i}.png"));
        }

        return frames;
    }
}