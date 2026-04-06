using System;

public class MatchRules {
    // 1. The "Zone" (PUBG style)
    public float CircleRadius = 4000.0f; // 4km map
    public float DamagePerSecond = 5.0f;

    // 2. The Players (COD/Free Fire hybrid)
    public int MaxPlayers = 100;
    public bool AutoExtractionEnabled = true;

    public void StartMatch() {
        Console.WriteLine("Initializing 4x4km Neo-Bermuda Map...");
        Console.WriteLine("Syncing Movement, Ballistics, and Hero Skills...");
    }
}