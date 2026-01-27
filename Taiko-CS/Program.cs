using Raylib_cs;
using Taiko_CS.Chart;
using Taiko_CS.Enums;
using Taiko_CS.screens;

namespace Taiko_CS;

class Program
{
    static void Main(string[] args)
    {
        Raylib.SetConfigFlags(ConfigFlags.VSyncHint | ConfigFlags.BorderlessWindowMode | ConfigFlags.FullscreenMode);
        ChartData dragoonExtreme = Chart.Chart.Parse("./Songs/VIVIVIVID", "VIVIVIVID.tja", Difficulty.ONI);
        Raylib.InitWindow(1920, 1080, "Taiko!");
        Raylib.InitAudioDevice();
        Raylib.SetWindowFocused();
        Screen songPlaying = new SongPlaying(Difficulty.ONI, dragoonExtreme); 
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            songPlaying.Draw();
            Raylib.DrawFPS(0, 0);
            Raylib.EndDrawing();
            songPlaying.HandleInput();
            songPlaying.HandleAudio();
            songPlaying.HandleOthers();
        }
        songPlaying.UnloadTextures();
        songPlaying.UnloadSounds();
        Raylib.CloseAudioDevice();
    }
}