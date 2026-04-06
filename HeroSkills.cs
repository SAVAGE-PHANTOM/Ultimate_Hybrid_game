using System;
using System.Collections.Generic;

public struct Vector3 {
    public float X;
    public float Y;
    public float Z;

    public Vector3(float x, float y, float z) {
        X = x;
        Y = y;
        Z = z;
    }

    public float DistanceTo(Vector3 other) {
        float dx = X - other.X;
        float dy = Y - other.Y;
        float dz = Z - other.Z;
        return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public float Magnitude() {
        return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
    }

    public static Vector3 Zero => new Vector3(0, 0, 0);

    public static Vector3 Normalize(Vector3 v) {
        float mag = v.Magnitude();
        return mag > 0.0f ? new Vector3(v.X / mag, v.Y / mag, v.Z / mag) : new Vector3(0, 0, 0);
    }

    public static Vector3 operator *(Vector3 vector, float scalar) {
        return new Vector3(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
    }

    public static Vector3 operator *(float scalar, Vector3 vector) {
        return vector * scalar;
    }
}

public class Enemy {
    public Vector3 Position;
    public bool IsHighlighted = false;
}

public class HeroSkills {
    // Skill Attributes
    public string ActiveSkillName = "Tactical Scan";
    public float SkillCooldown = 45.0f;
    public float ScanRadius = 30.0f;
    public float HighlightDuration = 3.0f;
    public bool IsSkillReady = true;

    private float skillCooldownTimer = 0.0f;
    private readonly List<HighlightedEnemy> highlightedEnemies = new List<HighlightedEnemy>();

    private class HighlightedEnemy {
        public Enemy Enemy;
        public float TimeRemaining;

        public HighlightedEnemy(Enemy enemy, float duration) {
            Enemy = enemy;
            TimeRemaining = duration;
        }
    }

    public void UseActiveSkill(Vector3 playerPosition, IEnumerable<Enemy> enemiesInRange) {
        if (!IsSkillReady) {
            return;
        }

        IsSkillReady = false;
        skillCooldownTimer = SkillCooldown;

        foreach (Enemy enemy in SphereTrace(playerPosition, ScanRadius, enemiesInRange)) {
            StartHighlight(enemy);
        }

        Console.WriteLine("Tactical Scan activated. Enemies highlighted for " + HighlightDuration + " seconds.");
    }

    public void Update(float deltaTime) {
        if (!IsSkillReady) {
            skillCooldownTimer -= deltaTime;
            if (skillCooldownTimer <= 0.0f) {
                skillCooldownTimer = 0.0f;
                IsSkillReady = true;
            }
        }

        for (int i = highlightedEnemies.Count - 1; i >= 0; i--) {
            HighlightedEnemy highlighted = highlightedEnemies[i];
            highlighted.TimeRemaining -= deltaTime;
            if (highlighted.TimeRemaining <= 0.0f) {
                highlighted.Enemy.IsHighlighted = false;
                highlightedEnemies.RemoveAt(i);
            }
        }
    }

    private IEnumerable<Enemy> SphereTrace(Vector3 center, float radius, IEnumerable<Enemy> potentialTargets) {
        List<Enemy> hits = new List<Enemy>();
        foreach (Enemy enemy in potentialTargets) {
            if (enemy.Position.DistanceTo(center) <= radius) {
                hits.Add(enemy);
            }
        }

        return hits;
    }

    private void StartHighlight(Enemy enemy) {
        enemy.IsHighlighted = true;
        highlightedEnemies.Add(new HighlightedEnemy(enemy, HighlightDuration));
    }
}