using System;

public class Weapon {
    public string Name;
    public int MagazineSize;
    public int AmmoInMagazine;
    public int ReserveAmmo;
    public float ReloadTime;
    public int Damage;
    public float EffectiveRange;
    public bool IsReloading;
    public float FireInterval;
    public bool IsAutomatic { get; private set; } = true;

    private float reloadTimer;
    private float fireTimer;

    public Weapon(string name, int magazineSize, float reloadTime, int damage, float effectiveRange) {
        Name = name;
        MagazineSize = magazineSize;
        AmmoInMagazine = magazineSize;
        ReserveAmmo = magazineSize * 3;
        ReloadTime = reloadTime;
        Damage = damage;
        EffectiveRange = effectiveRange;
        IsReloading = false;
        FireInterval = 0.11f;
        reloadTimer = 0.0f;
        fireTimer = 0.0f;
    }

    public bool CanReload => !IsReloading && AmmoInMagazine < MagazineSize && ReserveAmmo > 0;

    public string ToggleFiringMode() {
        IsAutomatic = !IsAutomatic;
        FireInterval = IsAutomatic ? 0.11f : 0.24f;
        return IsAutomatic ? "Auto" : "Semi";
    }

    public bool TryFire() {
        if (IsReloading) {
            Console.WriteLine($"{Name} is reloading and cannot fire.");
            return false;
        }

        if (AmmoInMagazine <= 0) {
            Console.WriteLine($"{Name} is out of ammo. Reload required.");
            return false;
        }

        if (fireTimer > 0.0f) {
            return false;
        }

        AmmoInMagazine -= 1;
        fireTimer = FireInterval;
        Console.WriteLine($"{Name} fired. Remaining ammo: {AmmoInMagazine}/{ReserveAmmo}");
        return true;
    }

    public void Reload() {
        if (!CanReload) {
            return;
        }

        IsReloading = true;
        reloadTimer = ReloadTime;
        Console.WriteLine($"Reloading {Name} ({ReloadTime:0.0}s)...");
    }

    public void Update(float deltaTime) {
        if (fireTimer > 0.0f) {
            fireTimer -= deltaTime;
            if (fireTimer < 0.0f) {
                fireTimer = 0.0f;
            }
        }

        if (!IsReloading) {
            return;
        }

        reloadTimer -= deltaTime;
        if (reloadTimer <= 0.0f) {
            int needed = MagazineSize - AmmoInMagazine;
            int loaded = Math.Min(needed, ReserveAmmo);
            AmmoInMagazine += loaded;
            ReserveAmmo -= loaded;
            IsReloading = false;
            Console.WriteLine($"{Name} reloaded to {AmmoInMagazine}/{ReserveAmmo}.");
        }
    }

    public bool CalculateHit(Vector3 origin, Vector3 targetPosition, PubgBallistics ballistics) {
        return ballistics.ResolveHit(origin, targetPosition, EffectiveRange, false);
    }

    public bool CalculateHit(Vector3 origin, Enemy target, PubgBallistics ballistics) {
        return ballistics.ResolveHit(origin, target.Position, EffectiveRange, target.IsHighlighted || target.IsUavRevealed);
    }

    public bool CalculateHit(Vector3 origin, Enemy target, PubgBallistics ballistics, bool ads, bool hipfireAim, bool holdBreath, bool leanActive) {
        return ballistics.ResolveHit(origin, target.Position, EffectiveRange, target.IsHighlighted || target.IsUavRevealed, ads, hipfireAim, holdBreath, leanActive);
    }
}
