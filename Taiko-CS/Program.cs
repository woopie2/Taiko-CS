using Raylib_cs;
using Taiko_CS.Enums;
using Taiko_CS.screens;

namespace Taiko_CS;

class Program
{
    static void Main(string[] args)
    {
        Raylib.InitWindow(1920, 1080, "Taiko!");
        Raylib.InitAudioDevice();
        Raylib.ToggleBorderlessWindowed();
        Raylib.SetWindowFocused();
        Screen songPlaying = new SongPlaying(Difficulty.ONI); 
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            songPlaying.Draw();
            Raylib.EndDrawing();
            songPlaying.HandleInput();
            songPlaying.HandleAudio();
        }
        songPlaying.UnloadTextures();
        songPlaying.UnloadSounds();
        Raylib.CloseAudioDevice();
    }
}