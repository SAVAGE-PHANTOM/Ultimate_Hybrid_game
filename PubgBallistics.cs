using System;

public class PubgBallistics {
    public float MuzzleVelocity = 900.0f;
    public float Gravity = 9.8f;
    public float BaseAccuracy = 0.76f;

    public void CalculateBulletFlight(float time) {
        Vector3 direction = new Vector3(1, 0, 0);
        Vector3 unitVector = Vector3.Normalize(direction);
        Vector3 velocity = unitVector * MuzzleVelocity;
        float drop = 0.5f * Gravity * (time * time);

        Console.WriteLine($"[PUBG DNA] Bullet speed: {MuzzleVelocity}m/s | Drop at {time}s: {drop}m");
    }

    public bool ResolveHit(Vector3 origin, Vector3 targetPosition, float effectiveRange, bool targetHighlighted) {
        float distance = origin.DistanceTo(targetPosition);
        if (distance > effectiveRange) {
            return false;
        }

        float flightTime = distance / MuzzleVelocity;
        float dropPenalty = 0.5f * Gravity * flightTime * flightTime;
        float distancePenalty = MathF.Min(0.32f, distance / MathF.Max(1.0f, effectiveRange) * 0.28f);
        float scanBonus = targetHighlighted ? 0.15f : 0.0f;
        float finalAccuracy = Math.Clamp(BaseAccuracy - distancePenalty - dropPenalty * 0.0025f + scanBonus, 0.20f, 0.95f);

        return Random.Shared.NextSingle() <= finalAccuracy;
    }

    public bool ResolveHit(Vector3 origin, Vector3 targetPosition, float effectiveRange, bool targetHighlighted, bool ads, bool hipfireAim, bool holdBreath, bool leanActive) {
        float distance = origin.DistanceTo(targetPosition);
        if (distance > effectiveRange) {
            return false;
        }

        float flightTime = distance / MuzzleVelocity;
        float dropPenalty = 0.5f * Gravity * flightTime * flightTime;
        float distancePenalty = MathF.Min(0.32f, distance / MathF.Max(1.0f, effectiveRange) * 0.28f);
        float scanBonus = targetHighlighted ? 0.15f : 0.0f;

        float adsBonus = ads ? 0.12f : 0.0f;
        float hipAimBonus = hipfireAim ? 0.07f : 0.0f;
        float breathBonus = (ads && holdBreath) ? 0.05f : 0.0f;
        float leanPenalty = leanActive ? 0.05f : 0.0f;

        float finalAccuracy = Math.Clamp(
            BaseAccuracy - distancePenalty - dropPenalty * 0.0025f + scanBonus + adsBonus + hipAimBonus + breathBonus - leanPenalty,
            0.20f,
            0.97f);

        return Random.Shared.NextSingle() <= finalAccuracy;
    }
}
