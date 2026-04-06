using System;

public class PubgBallistics {
    // Weapon Stats
    public float MuzzleVelocity = 900.0f; // Fast for Snipers
    public float BulletDropGravity = -9.8f; // Earth gravity
    public float AirDrag = 0.01f;

    public void CalculateBulletFlight(float TimeInAir) {
        // Math for the arc of the bullet
        float CurrentVerticalDrop = 0.5f * BulletDropGravity * (TimeInAir * TimeInAir);
        Console.WriteLine("Bullet Drop calculated at: " + CurrentVerticalDrop + " meters");
    }
}