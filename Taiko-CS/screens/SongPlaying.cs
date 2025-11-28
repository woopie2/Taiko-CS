using System.ComponentModel;
using System.Numerics;
using Raylib_cs;
using Taiko_CS.Enums;
using Taiko_CS.Animation;

namespace Taiko_CS.screens;

public class SongPlaying : Screen
{
    private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    private Dictionary<string, Sound> sounds = new Dictionary<string, Sound>();
    private List<Sound> playingSounds = new  List<Sound>();
    private bool isRightDonPressed;
    private bool isLeftDonPressed;
    private bool isRightKaPressed;
    private bool isLeftKaPressed;
    private Difficulty difficulty;
    private List<Animation.Animation> runningForegroundAnimations = new List<Animation.Animation>();
    private List<Animation.Animation> runningBackgroundAnimations = new List<Animation.Animation>();
    private bool areAnimationsInitialized = false;
    public SongPlaying(Difficulty difficulty)
    {
        this.difficulty = difficulty;
        LoadTextures();
        LoadSounds();
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
    }

    public override void LoadSounds()
    {
        LoadSound("Don", "Taiko/dong.wav");
        LoadSound("Ka", "Taiko/ka.wav");
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

    public override void UnloadTextures()
    {
        foreach (KeyValuePair<string, Texture2D> texture in textures)
        {
            Raylib.UnloadTexture(texture.Value);
        }
    }

    public override void UnloadSounds()
    {
        foreach (KeyValuePair<string, Sound> sound in sounds)
        {
            Raylib.UnloadSound(sound.Value);
        }
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
    
    private void UpdateBackgroundAnimations()
    {
        List<Animation.Animation> finishedAnimations = new List<Animation.Animation>();
        List<Animation.Animation> restartedAnimations = new List<Animation.Animation>();
        foreach (Animation.Animation animation in runningBackgroundAnimations)
        {
            if (animation.IsFinish())
            {
                finishedAnimations.Add(animation);
                Animation.Animation restartedAnimation = animation.Clone();
                restartedAnimations.Add(restartedAnimation);
                restartedAnimation.StartAnimation();
                continue;
            }
            animation.UpdateAnimation();
            
        }

        foreach (Animation.Animation animation in finishedAnimations)
        {
            runningBackgroundAnimations.Remove(animation);
        }

        foreach (Animation.Animation animation in restartedAnimations)
        {
            runningBackgroundAnimations.Add(animation);
        }
    }
    
    private void DrawBackground()
    {
        Raylib.DrawTexture(textures["UpBackground"], 0 * 492, 0, Color.White);
        Raylib.DrawTexture(textures["UpBackground"], 1 * 492, 0, Color.White);
        Raylib.DrawTexture(textures["UpBackground"], 2 * 492, 0, Color.White);
        Raylib.DrawTexture(textures["UpBackground"], 3 * 492, 0, Color.White);
    }
    
    private void DrawPlayingZone(int x, int y)
    {
        Raylib.DrawTexture(textures["LeftSideBackground"], x, y + 72, Color.White);
        Raylib.DrawTexture(textures["PlayingZoneFrame"], x + textures["LeftSideBackground"].Width, y, Color.White);
        DrawTaiko(isLeftKaPressed, isRightKaPressed, isLeftDonPressed, isRightDonPressed, x + 499 - 180 - 10, y + textures["Taiko"].Height / 2);
        Raylib.DrawTexturePro(textures["LaneUpBackground"], new Rectangle(0, 0, textures["LaneUpBackground"].Width, textures["LaneUpBackground"].Height), new Rectangle(x + textures["LeftSideBackground"].Width, y + 83, textures["PlayingZoneFrame"].Width, textures["LaneUpBackground"].Height) ,new Vector2(0, 0) , 0, Color.White );
        Raylib.DrawTexturePro(textures["LaneDownBackground"], new Rectangle(0, 0, textures["LaneDownBackground"].Width, textures["LaneDownBackground"].Height), new Rectangle(x + textures["LeftSideBackground"].Width, y + 83 + textures["LaneUpBackground"].Height + 6, textures["PlayingZoneFrame"].Width, textures["LaneDownBackground"].Height) ,new Vector2(0, 0) , 0, Color.White );
        DrawDifficultyIcon(x + 10, y);
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
    
    private void DrawTaiko(bool leftKaPressed, bool rightKaPressed, bool leftDonPressed, bool rightDonPressed, int x, int y)
    {
        Raylib.DrawTexture(textures["Taiko"], x, y, Color.White);
        if (leftDonPressed)
        {
            Texture2D taikoDon = textures["TaikoDon"];
            int width = taikoDon.Width / 2;
            int height = taikoDon.Height;
            Animation.Animation leftDonAnimation = new FadeOutAnimation(taikoDon, 0.0625, new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height), 0.125);
            leftDonAnimation.StartAnimation();
            runningForegroundAnimations.Add(leftDonAnimation);
        }

        if (rightDonPressed)
        {
            Texture2D taikoDon = textures["TaikoDon"];
            int width = taikoDon.Width / 2;
            int height = taikoDon.Height;
            Animation.Animation rightDonAnimation = new FadeOutAnimation(taikoDon, 0.0625, new Rectangle(width, 0, width, height), new Rectangle(x + width, y, width, height), 0.125);
            rightDonAnimation.StartAnimation();
            runningForegroundAnimations.Add(rightDonAnimation);
        }

        if (leftKaPressed)
        {
            Texture2D taikoKa = textures["TaikoKa"];
            int width = taikoKa.Width / 2;
            int height = taikoKa.Height;
            Animation.Animation leftKaAnimation = new FadeOutAnimation(taikoKa, 0.0625, new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height),0.125);
            leftKaAnimation.StartAnimation();
            runningForegroundAnimations.Add(leftKaAnimation);
        }

        if (rightKaPressed)
        {
            Texture2D taikoKa = textures["TaikoKa"];
            int width = taikoKa.Width / 2;
            int height = taikoKa.Height;
            Animation.Animation rightKaAnimation = new FadeOutAnimation(taikoKa, 0.0625, new Rectangle(width, 0, width, height), new Rectangle(x + width, y, width, height),0.125);
            rightKaAnimation.StartAnimation();
            runningForegroundAnimations.Add(rightKaAnimation);
        }
    }

    private void InitliazeAnimations()
    {
        Animation.Animation upBackgroundScrolling0 = new HorizontalMovingAnimation(textures["UpBackground"], -492 * 1, -492 * 0, 0, 5);
        runningBackgroundAnimations.Add(upBackgroundScrolling0);
        upBackgroundScrolling0.StartAnimation();
        
        Animation.Animation upBackgroundScrolling1 = new HorizontalMovingAnimation(textures["UpBackground"], 492*0, 1 * 492, 0, 5);
        runningBackgroundAnimations.Add(upBackgroundScrolling1);
        upBackgroundScrolling1.StartAnimation();
        
        Animation.Animation upBackgroundScrolling2 = new HorizontalMovingAnimation(textures["UpBackground"], 492*1, 2 * 492, 0, 5);
        runningBackgroundAnimations.Add(upBackgroundScrolling2);
        upBackgroundScrolling2.StartAnimation();
        
        Animation.Animation upBackgroundScrolling3 = new HorizontalMovingAnimation(textures["UpBackground"], 492*2, 3 * 492, 0, 5);
        runningBackgroundAnimations.Add(upBackgroundScrolling3);
        upBackgroundScrolling3.StartAnimation();
        
        Animation.Animation upBackgroundScrolling4 = new HorizontalMovingAnimation(textures["UpBackground"], 492*3, 4 * 492, 0, 5);
        runningBackgroundAnimations.Add(upBackgroundScrolling4);
        upBackgroundScrolling4.StartAnimation();
        areAnimationsInitialized = true;
    }
    
    public override void Draw()
    {
        if (!areAnimationsInitialized)
        {
            InitliazeAnimations();
        }
        Raylib.ClearBackground(Color.White);
        DrawBackgroundAnimations();
        DrawPlayingZone(0, 276 - 72);
        DrawForegroundAnimations();
        UpdateForegroundAnimations();
        UpdateBackgroundAnimations();
    }

    public override void HandleInput()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.F))
        {
            isLeftDonPressed = true;
            Sound sound = Raylib.LoadSoundAlias(sounds["Don"]);
            playingSounds.Add(sound);
            Raylib.PlaySound(sound);
        }
        else
        {
            isLeftDonPressed = false;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.J))
        {
            isRightDonPressed = true; 
            Sound sound = Raylib.LoadSoundAlias(sounds["Don"]);
            playingSounds.Add(sound);
            Raylib.PlaySound(sound);
        }
        else
        {
            isRightDonPressed = false;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.D))
        {
            isLeftKaPressed = true;
            Sound sound = Raylib.LoadSoundAlias(sounds["Ka"]);
            playingSounds.Add(sound);
            Raylib.PlaySound(sound);
        }
        else
        {
            isLeftKaPressed = false;
        }
        
        if (Raylib.IsKeyPressed(KeyboardKey.K))
        {
            isRightKaPressed = true;
            Sound sound = Raylib.LoadSoundAlias(sounds["Ka"]);
            playingSounds.Add(sound);
            Raylib.PlaySound(sound);
        }
        else
        {
            isRightKaPressed = false;
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
}