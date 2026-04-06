using System;

public struct Vector3 {
    public float X;
    public float Y;
    public float Z;

    public Vector3(float x, float y, float z) {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3 operator *(Vector3 a, float scalar) => new Vector3(a.X * scalar, a.Y * scalar, a.Z * scalar);
    public static Vector3 operator *(float scalar, Vector3 a) => a * scalar;

    public float Magnitude() => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

    public Vector3 Normalized() {
        float mag = Magnitude();
        return mag > 0.0f ? new Vector3(X / mag, Y / mag, Z / mag) : new Vector3(0, 0, 0);
    }
}

public class PubgBallistics {
    public float MuzzleVelocity = 900.0f;
    public float BulletDropGravity = -9.8f;
    public float AirDrag = 0.01f;

    public Vector3 Position;
    public Vector3 Velocity;
    public bool IsActive = false;

    public void Fire(Vector3 startPosition, Vector3 forwardDirection) {
        Position = startPosition;
        Velocity = forwardDirection.Normalized() * MuzzleVelocity;
        IsActive = true;
    }

    public void Tick(float deltaTime) {
        if (!IsActive) {
            return;
        }

        Velocity = new Vector3(Velocity.X, Velocity.Y, Velocity.Z + BulletDropGravity * deltaTime);

        float dragFactor = 1.0f - AirDrag * deltaTime;
        if (dragFactor < 0.0f) {
            dragFactor = 0.0f;
        }
        Velocity = Velocity * dragFactor;

        Position += Velocity * deltaTime;
    }
}