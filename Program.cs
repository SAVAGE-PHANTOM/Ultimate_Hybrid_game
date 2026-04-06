using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

class Program {
    static void Main() {
        var gameWindowSettings = GameWindowSettings.Default;
        var nativeWindowSettings = new NativeWindowSettings() {
            Size = new Vector2i(1280, 720),
            Title = "Ultimate Hybrid Game Preview"
        };

        using var window = new VisualGame(gameWindowSettings, nativeWindowSettings);
        window.Run();
    }
}