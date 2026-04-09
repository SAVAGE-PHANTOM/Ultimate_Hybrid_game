using System;

public class MatchRules {
    public float CircleRadius = 520.0f;
    public float CurrentZoneRadius = 520.0f;
    public float ZoneShrinkRate = 5.5f;
    public float DamagePerSecond = 8.0f;
    public Vector3 ZoneCenter = new Vector3(600.0f, 360.0f, 0.0f);

    public int MaxPlayers = 100;
    public bool AutoExtractionEnabled = true;

    public void StartMatch() {
        CurrentZoneRadius = CircleRadius;
        Console.WriteLine("Initializing 4x4km Neo-Bermuda Map...");
        Console.WriteLine("Syncing Movement, Ballistics, and Hero Skills...");
        Console.WriteLine("Match Start: Zone radius is " + CurrentZoneRadius + " meters.");
    }

    public void UpdateZone(float deltaTime) {
        if (CurrentZoneRadius > 120.0f) {
            CurrentZoneRadius -= ZoneShrinkRate * deltaTime;
            if (CurrentZoneRadius < 120.0f) {
                CurrentZoneRadius = 120.0f;
            }
        }
    }

    public bool IsOutsideZone(Vector3 position) {
        return position.DistanceTo(ZoneCenter) > CurrentZoneRadius;
    }
}
