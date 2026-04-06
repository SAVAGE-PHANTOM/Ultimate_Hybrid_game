using System;

public class MatchRules {
    // 1. The "Zone" (PUBG style)
    public float CircleRadius = 4000.0f; // 4km map
    public float CurrentZoneRadius = 4000.0f;
    public float ZoneShrinkRate = 120.0f;
    public float DamagePerSecond = 5.0f;

    // 2. The Players (COD/Free Fire hybrid)
    public int MaxPlayers = 100;
    public bool AutoExtractionEnabled = true;

    public void StartMatch() {
        CurrentZoneRadius = CircleRadius;
        Console.WriteLine("Initializing 4x4km Neo-Bermuda Map...");
        Console.WriteLine("Syncing Movement, Ballistics, and Hero Skills...");
        Console.WriteLine("Match Start: Zone radius is " + CurrentZoneRadius + " meters.");
    }

    public void UpdateZone(float deltaTime) {
        if (CurrentZoneRadius > 200.0f) {
            CurrentZoneRadius -= ZoneShrinkRate * deltaTime;
            if (CurrentZoneRadius < 200.0f) {
                CurrentZoneRadius = 200.0f;
            }
        }
    }

    public bool IsOutsideZone(Vector3 position) {
        return position.DistanceTo(Vector3.Zero) > CurrentZoneRadius;
    }
}