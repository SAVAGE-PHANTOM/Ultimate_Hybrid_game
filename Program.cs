using System;
using System.IO;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace Ultimate_Hybrid_game;

internal static class Program {
    private static void Main() {
        Console.WriteLine("Launching Ultimate Hybrid Game prototype...");
        TryInitializeFirebase();

        GameWindowSettings gameWindowSettings = GameWindowSettings.Default;
        NativeWindowSettings nativeWindowSettings = new NativeWindowSettings {
            Size = new Vector2i(1280, 768),
            Title = "Ultimate Hybrid Game",
            APIVersion = new Version(3, 3)
        };

        using VisualGame window = new VisualGame(gameWindowSettings, nativeWindowSettings);
        window.Run();
    }

    private static void TryInitializeFirebase() {
        if (!File.Exists("google-services.json")) {
            Console.WriteLine("Firebase config not found. Running in offline prototype mode.");
            return;
        }

        Console.WriteLine("Firebase config detected. Backend wiring is staged, but this prototype currently runs offline.");
    }
}
