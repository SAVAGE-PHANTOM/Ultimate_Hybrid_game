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

    private float reloadTimer;

    public Weapon(string name, int magazineSize, float reloadTime, int damage, float effectiveRange) {
        Name = name;
        MagazineSize = magazineSize;
        AmmoInMagazine = magazineSize;
        ReserveAmmo = magazineSize * 3;
        ReloadTime = reloadTime;
        Damage = damage;
        EffectiveRange = effectiveRange;
        IsReloading = false;
        reloadTimer = 0.0f;
    }

    public bool CanReload => !IsReloading && AmmoInMagazine < MagazineSize && ReserveAmmo > 0;

    public bool TryFire() {
        if (IsReloading) {
            Console.WriteLine($"{Name} is reloading and cannot fire.");
            return false;
        }

        if (AmmoInMagazine <= 0) {
            Console.WriteLine($"{Name} is out of ammo. Reload required.");
            return false;
        }

        AmmoInMagazine -= 1;
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
        float distance = origin.DistanceTo(targetPosition);

        if (distance > EffectiveRange) {
            return false;
        }

        float flightTime = distance / ballistics.MuzzleVelocity;
        float drop = 0.5f * ballistics.Gravity * flightTime * flightTime;
        float verticalDifference = targetPosition.Y - origin.Y;
        float tolerance = Math.Min(3.0f, distance * 0.01f + 0.5f);

        return Math.Abs(drop - verticalDifference) <= tolerance;
    }
}
