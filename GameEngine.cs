using System;
using System.Collections.Generic;

public class Player {
    public string Name;
    public float Health = 100.0f;
    public Vector3 Position;
    public CodMovement Movement;
    public Weapon EquippedWeapon;
    public HeroSkills Skills;
    public bool IsAlive => Health > 0.0f;

    public Player(string name) {
        Name = name;
        Position = new Vector3(0, 0, 0);
        Movement = new CodMovement();
        EquippedWeapon = new Weapon("Raptor AR", 30, 2.0f, 35, 850.0f);
        Skills = new HeroSkills();
    }

    public void Update(float deltaTime) {
        Movement.Update(deltaTime);
        EquippedWeapon.Update(deltaTime);
        Skills.Update(deltaTime);
        if (Health < 0.0f) {
            Health = 0.0f;
        }
    }

    public void Walk(float deltaTime) {
        Movement.Move(deltaTime, 0.6f, false);
        Position = new Vector3(Position.X + Movement.CurrentVelocity * deltaTime, Position.Y, Position.Z);
    }

    public void Sprint(float deltaTime) {
        Movement.Move(deltaTime, 1.0f, true);
        Position = new Vector3(Position.X + Movement.CurrentVelocity * deltaTime, Position.Y, Position.Z);
    }

    public void ApplySlide() {
        Movement.ApplySlide();
        Position = new Vector3(Position.X + Movement.CurrentVelocity * 0.2f, Position.Y, Position.Z);
    }

    public bool FireAt(Enemy target, PubgBallistics ballistics) {
        if (!EquippedWeapon.TryFire()) {
            if (EquippedWeapon.CanReload) {
                EquippedWeapon.Reload();
            }
            return false;
        }

        bool hit = EquippedWeapon.CalculateHit(Position, target.Position, ballistics);
        if (hit) {
            target.IsHighlighted = false;
            Console.WriteLine($"Target hit at {Position.DistanceTo(target.Position):0.0}m. Enemy eliminated.");
            return true;
        }

        Console.WriteLine($"Shot missed at {Position.DistanceTo(target.Position):0.0}m.");
        return false;
    }
}

public class GameEngine {
    public Player Player;
    public List<Enemy> Enemies = new List<Enemy>();
    public MatchRules Rules = new MatchRules();
    public PubgBallistics Ballistics = new PubgBallistics();
    private float elapsedTime = 0.0f;
    private readonly float matchDuration = 18.0f;
    private bool skillUsed = false;
    private bool firedEnemy1 = false;
    private bool slideUsed = false;
    private bool firedEnemy2 = false;
    private bool firedEnemy3 = false;
    private bool reloadUsed = false;

    public bool IsMatchOver => elapsedTime >= matchDuration || !Player.IsAlive;
    public bool ExtractionSuccessful { get; private set; }
    public float ElapsedTime => elapsedTime;

    public GameEngine() {
        Player = new Player("Spectre");

        Enemies.Add(new Enemy { Position = new Vector3(120, 0, 0) });
        Enemies.Add(new Enemy { Position = new Vector3(260, 0, 0) });
        Enemies.Add(new Enemy { Position = new Vector3(410, 0, 0) });
    }

    public void StartMatch() {
        Rules.StartMatch();
        Console.WriteLine($"Player ready: {Player.Name}. Health: {Player.Health}. Weapon: {Player.EquippedWeapon.Name}.");
    }

    public void Update(float deltaTime) {
        if (IsMatchOver) {
            return;
        }

        Step(deltaTime);
        elapsedTime += deltaTime;

        if (IsMatchOver) {
            ExtractionSuccessful = Player.IsAlive;
            if (ExtractionSuccessful) {
                Console.WriteLine("Extraction sequence engaged. The VTOL arrives and the mission ends in success.");
            } else {
                Console.WriteLine("The player has been knocked out before extraction.");
            }

            Console.WriteLine($"Match ended at {elapsedTime:0.0}s. Remaining health: {Player.Health:0.0}.");
        }
    }

    private void Step(float deltaTime) {
        if (!skillUsed && elapsedTime >= 1.0f) {
            Player.Skills.UseActiveSkill(Player.Position, Enemies);
            skillUsed = true;
        }

        if (elapsedTime >= 1.5f && elapsedTime < 2.0f) {
            Player.Sprint(deltaTime);
        }

        if (!firedEnemy1 && elapsedTime >= 2.0f) {
            Player.FireAt(Enemies[0], Ballistics);
            firedEnemy1 = true;
        }

        if (!slideUsed && elapsedTime >= 4.0f) {
            Player.ApplySlide();
            slideUsed = true;
        }

        if (!firedEnemy2 && elapsedTime >= 7.0f) {
            Player.FireAt(Enemies[1], Ballistics);
            firedEnemy2 = true;
        }

        if (elapsedTime >= 10.0f && elapsedTime < 10.5f) {
            Player.Sprint(deltaTime);
        }

        if (!firedEnemy3 && elapsedTime >= 12.0f) {
            Player.FireAt(Enemies[2], Ballistics);
            firedEnemy3 = true;
        }

        if (!reloadUsed && elapsedTime >= 14.0f) {
            Player.EquippedWeapon.Reload();
            reloadUsed = true;
        }

        if (Rules.IsOutsideZone(Player.Position)) {
            Player.Health -= Rules.DamagePerSecond * deltaTime;
            Console.WriteLine($"Outside the blue zone. Taking {Rules.DamagePerSecond * deltaTime:0.0} damage.");
        }

        Rules.UpdateZone(deltaTime);
        Player.Update(deltaTime);
    }

    public string GetStatusText() {
        return $"Time: {elapsedTime:0.0}s  Health: {Player.Health:0.0}  Stamina: {Player.Movement.CurrentStamina:0.0}  Zone: {Rules.CurrentZoneRadius:0.0}";
    }
}
