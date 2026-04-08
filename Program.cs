using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System.IO;
using System.Text.Json;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

class Program {
    static void Main() {
        // Initialize Firebase
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile("google-services.json"),
        });

        Console.WriteLine("Firebase is ready for the game!");

        // Load Firebase configuration from google-services.json
        LoadFirebaseConfig();

        var gameWindowSettings = GameWindowSettings.Default;
        var nativeWindowSettings = new NativeWindowSettings() {
            Size = new Vector2i(1280, 720),
            Title = "Ultimate Hybrid Game Preview"
        };

        using var window = new VisualGame(gameWindowSettings, nativeWindowSettings);
        window.Run();
    }

    static void LoadFirebaseConfig() {
        string configPath = "google-services.json";
        if (File.Exists(configPath)) {
            try {
                string jsonContent = File.ReadAllText(configPath);
                using (JsonDocument doc = JsonDocument.Parse(jsonContent)) {
                    JsonElement root = doc.RootElement;
                    // Extract Firebase configuration
                    if (root.TryGetProperty("project_info", out var projectInfo)) {
                        if (projectInfo.TryGetProperty("project_id", out var projectId)) {
                            Console.WriteLine($"Firebase Project ID: {projectId.GetString()}");
                        }
                    }
                    Console.WriteLine("Firebase configuration loaded successfully.");
                }
            } catch (Exception ex) {
                Console.WriteLine($"Warning: Could not load Firebase config: {ex.Message}");
            }
        } else {
            Console.WriteLine("Note: google-services.json not found. Firebase configuration not loaded.");
        }
    }
}