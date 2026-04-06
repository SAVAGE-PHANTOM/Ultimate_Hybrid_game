using System;

public class PubgBallistics {
    public float MuzzleVelocity = 900.0f;
    public float Gravity = -9.8f;

    // A helper to fix the "Normalized" error
    public Vector3 GetNormalized(Vector3 v) {
        return Vector3.Normalize(v);
    }

    public void CalculateBulletFlight(float time) {
        // Create a starting direction (looking forward on X axis)
        Vector3 direction = new Vector3(1, 0, 0);
        
        // FIX: Use Vector3.Normalize(direction) instead of direction.Normalized
        Vector3 unitVector = Vector3.Normalize(direction);

        // FIX: For multiplication, put the Float FIRST or use Vector3.Multiply
        Vector3 velocity = unitVector * MuzzleVelocity;
        
        float drop = 0.5f * Gravity * (time * time);
        
        Console.WriteLine($"[PUBG DNA] Bullet speed: {MuzzleVelocity}m/s | Drop at {time}s: {drop}m");
    }
}