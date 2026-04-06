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

    // Stamina and sprinting
    public float MaxStamina = 100.0f;
    public float CurrentStamina = 100.0f;
    public float SprintDrainRate = 18.0f;
    public float StaminaRecoverRate = 12.0f;
    public bool IsSprinting = false;

    public void ApplySlide() {
        StartSlide();
    }

    public void StartSlide() {
        if (IsSliding || cooldownTimer > 0.0f || CurrentStamina < 20.0f) {
            return;
        }

        IsSliding = true;
        slideTimer = SlideDuration;
        cooldownTimer = SlideCooldown;
        CurrentVelocity = CrouchSpeed + SlideForce;
        CurrentStamina = Math.Max(0.0f, CurrentStamina - 20.0f);

        Console.WriteLine("COD Slide Activated");
    }

    public void Move(float deltaTime, float inputMagnitude, bool sprint) {
        if (IsSliding) {
            return;
        }

        float targetSpeed = BaseSpeed * Math.Max(0.2f, inputMagnitude);
        if (sprint && CurrentStamina > 0.0f) {
            IsSprinting = true;
            CurrentVelocity = targetSpeed * SprintMultiplier;
            CurrentStamina -= SprintDrainRate * deltaTime;
            if (CurrentStamina < 0.0f) {
                CurrentStamina = 0.0f;
            }
        } else {
            IsSprinting = false;
            CurrentVelocity = targetSpeed;
            CurrentStamina += StaminaRecoverRate * deltaTime;
            if (CurrentStamina > MaxStamina) {
                CurrentStamina = MaxStamina;
            }
        }
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
