using System;

public class CodMovement {
    // Movement Stats
    public float BaseSpeed = 600.0f;
    public float SprintMultiplier = 1.5f;
    
    // Slide Variables
    public bool IsSliding = false;
    public float SlideVelocity = 1200.0f; 
    public float SlideDecay = 0.5f; // How fast the slide slows down
    public float SlideCooldown = 2.0f;

    public void StartSlide() {
        if (!IsSliding) {
            IsSliding = true;
            // Trigger animation and velocity boost here
            Console.WriteLine("COD Slide Activated");
        }
    }
}