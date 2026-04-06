using System;

public class CodMovement {
    // Movement Stats
    public float BaseSpeed = 600.0f;
    public float SprintMultiplier = 1.5f;
    public float CrouchSpeed = 300.0f;

    // Slide Variables
    public bool IsSliding = false;
    public float SlideForce = 400.0f;
    public float SlideDuration = 1.5f;
    public float SlideCooldown = 2.0f;

    public float CurrentVelocity = 600.0f;
    private float slideTimer = 0.0f;
    private float cooldownTimer = 0.0f;

    public void StartSlide() {
        if (IsSliding || cooldownTimer > 0.0f) {
            return;
        }

        IsSliding = true;
        slideTimer = SlideDuration;
        cooldownTimer = SlideCooldown;
        CurrentVelocity = CrouchSpeed + SlideForce;

        // Trigger animation and velocity boost here
        Console.WriteLine("COD Slide Activated");
    }

    public void Update(float deltaTime) {
        if (cooldownTimer > 0.0f) {
            cooldownTimer -= deltaTime;
            if (cooldownTimer < 0.0f) {
                cooldownTimer = 0.0f;
            }
        }

        if (!IsSliding) {
            return;
        }

        slideTimer -= deltaTime;
        if (slideTimer <= 0.0f) {
            IsSliding = false;
            CurrentVelocity = CrouchSpeed;
            return;
        }

        float slideProgress = slideTimer / SlideDuration;
        CurrentVelocity = CrouchSpeed + SlideForce * slideProgress;
    }
}
