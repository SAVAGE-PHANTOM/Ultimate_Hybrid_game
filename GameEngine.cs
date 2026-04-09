using System;
using System.Collections.Generic;

public sealed class InputState {
    public bool Up;
    public bool Down;
    public bool Left;
    public bool Right;
    public bool Sprint;
    public bool Walk;
    public bool JumpPressed;
    public bool VaultPressed;
    public bool CrouchPressed;
    public bool PronePressed;
    public bool FireHeld;
    public bool FirePressed;
    public bool AdsHeld;
    public bool HipfireAimHeld;
    public bool LeanLeft;
    public bool LeanRight;
    public bool HoldBreathHeld;
    public bool ToggleFireModePressed;
    public int ZoomDelta;
    public bool InventoryPressed;
    public bool ReloadPressed;
    public bool InteractHeld;
    public bool QuickMarkerPressed;
    public bool ScorestreakUavPressed;
}

public class Player {
    public string Name;
    public float Health = 100.0f;
    public Vector3 Position;
    public CodMovement Movement;
    public Weapon EquippedWeapon;
    public HeroSkills Skills;
    public int Eliminations;
    public float Radius = 16.0f;
    public bool IsAlive => Health > 0.0f;

    private Vector3 moveDirection = new Vector3(1, 0, 0);
    public bool IsCrouched { get; private set; }
    public bool IsProne { get; private set; }
    public bool IsJumping { get; private set; }
    private float jumpTimer = 0.0f;

    public Player(string name, Vector3 spawnPosition) {
        Name = name;
        Position = spawnPosition;
        Movement = new CodMovement();
        EquippedWeapon = new Weapon("Raptor AR", 30, 1.9f, 34, 340.0f);
        Skills = new HeroSkills();
    }

    public void Update(float deltaTime) {
        Movement.Update(deltaTime);
        EquippedWeapon.Update(deltaTime);
        Skills.Update(deltaTime);
        Health = Math.Clamp(Health, 0.0f, 100.0f);

        if (jumpTimer > 0.0f) {
            jumpTimer -= deltaTime;
            if (jumpTimer <= 0.0f) {
                jumpTimer = 0.0f;
                IsJumping = false;
            }
        }
    }

    public void Move(Vector3 desiredDirection, float deltaTime, bool sprint, bool walk, bool ads, float minX, float minY, float maxX, float maxY) {
        if (desiredDirection.Magnitude() > 0.01f) {
            moveDirection = Vector3.Normalize(desiredDirection);
        }

        float inputMagnitude = desiredDirection.Magnitude() > 0.01f ? 1.0f : 0.0f;
        if (walk) {
            inputMagnitude *= 0.55f;
        }

        bool allowSprint = sprint && inputMagnitude > 0.0f && !ads;
        Movement.Move(deltaTime, inputMagnitude, allowSprint);

        Vector3 movementVector = moveDirection;
        if (inputMagnitude <= 0.0f && !Movement.IsSliding) {
            movementVector = Vector3.Zero;
        }

        float stanceMultiplier = 1.0f;
        if (IsProne) {
            stanceMultiplier = 0.55f;
        } else if (IsCrouched) {
            stanceMultiplier = 0.78f;
        }

        float adsMultiplier = ads ? 0.75f : 1.0f;
        Position += movementVector * (Movement.CurrentVelocity * deltaTime);
        Position += movementVector * (Movement.CurrentVelocity * (stanceMultiplier * adsMultiplier - 1.0f) * deltaTime);
        Position = new Vector3(
            Math.Clamp(Position.X, minX, maxX),
            Math.Clamp(Position.Y, minY, maxY),
            0.0f);
    }

    public void ApplySlide() {
        Movement.ApplySlide();
    }

    public void Jump() {
        if (IsJumping || IsProne) {
            return;
        }

        IsJumping = true;
        jumpTimer = 0.25f;
    }

    public void ToggleCrouch() {
        IsCrouched = !IsCrouched;
        if (IsCrouched) {
            IsProne = false;
        }
    }

    public void ToggleProne() {
        IsProne = !IsProne;
        if (IsProne) {
            IsCrouched = false;
        }
    }

    public bool FireAt(Enemy target, PubgBallistics ballistics, bool ads, bool hipfireAim, bool holdBreath, bool leanActive) {
        if (!EquippedWeapon.TryFire()) {
            if (EquippedWeapon.CanReload) {
                EquippedWeapon.Reload();
            }
            return false;
        }

        bool hit = EquippedWeapon.CalculateHit(Position, target, ballistics, ads, hipfireAim, holdBreath, leanActive);
        if (hit) {
            target.Health -= EquippedWeapon.Damage;
            Console.WriteLine($"Hit {target.Name} at {Position.DistanceTo(target.Position):0.0}m for {EquippedWeapon.Damage} damage.");
            if (!target.IsAlive) {
                Eliminations += 1;
                target.IsHighlighted = false;
                Console.WriteLine($"{target.Name} eliminated.");
            }

            return true;
        }

        Console.WriteLine($"Shot missed at {Position.DistanceTo(target.Position):0.0}m.");
        return false;
    }
}

public class GameEngine {
    public Player Player { get; }
    public List<Enemy> Enemies { get; } = new List<Enemy>();
    public MatchRules Rules { get; } = new MatchRules();
    public PubgBallistics Ballistics { get; } = new PubgBallistics();
    public Vector3 ExtractionPoint { get; } = new Vector3(1060.0f, 620.0f, 0.0f);
    public float ExtractionRadius { get; } = 34.0f;
    public float WorldWidth { get; } = 1200.0f;
    public float WorldHeight { get; } = 720.0f;
    public bool IsMatchOver { get; private set; }
    public bool ExtractionUnlocked { get; private set; }
    public bool ExtractionSuccessful { get; private set; }
    public float ElapsedTime { get; private set; }
    public string MatchResult { get; private set; } = "Drop in and survive.";
    public bool InventoryOpen { get; private set; }
    public int Score { get; private set; }
    public int Streak { get; private set; }
    public bool UavReady => Score >= 400 && !uavActive;
    public bool UavActive => uavActive;

    private readonly Queue<string> eventLog = new Queue<string>();
    private float extractionProgress = 0.0f;
    private float extractionGoal = 2.0f;
    private readonly float matchDuration = 150.0f;
    private bool uavActive = false;
    private float uavTimer = 0.0f;
    private readonly float uavDuration = 8.0f;

    public GameEngine() {
        Player = new Player("Spectre", new Vector3(120.0f, 140.0f, 0.0f));
        SpawnEnemies();
    }

    public void StartMatch() {
        Rules.StartMatch();
        LogEvent("Controls: WASD move, LShift sprint, LCtrl walk, Space jump, LMB fire, RMB ADS, Q/E lean, CapsLock aim, R reload, F interact.");
        LogEvent($"Player ready: {Player.Name}. Weapon: {Player.EquippedWeapon.Name}.");
    }

    public void Update(float deltaTime, InputState input) {
        if (IsMatchOver) {
            return;
        }

        ElapsedTime += deltaTime;

        HandlePlayerInput(deltaTime, input);
        UpdateEnemies(deltaTime);
        UpdateExtraction(deltaTime, input);
        ResolveZoneDamage(deltaTime);

        Player.Update(deltaTime);
        Rules.UpdateZone(deltaTime);

        if (!Player.IsAlive) {
            EndMatch(false, "You were eliminated before extraction.");
        } else if (ElapsedTime >= matchDuration) {
            EndMatch(false, "The storm closed before extraction.");
        }
    }

    public IReadOnlyCollection<string> GetRecentEvents() {
        return eventLog.ToArray();
    }

    public Enemy? GetNearestAliveEnemy() {
        Enemy? best = null;
        float bestDistance = float.MaxValue;
        foreach (Enemy enemy in Enemies) {
            if (!enemy.IsAlive) {
                continue;
            }

            float distance = Player.Position.DistanceTo(enemy.Position);
            if (distance < bestDistance) {
                bestDistance = distance;
                best = enemy;
            }
        }

        return best;
    }

    public string GetStatusText() {
        string objective = ExtractionUnlocked ? "Reach extraction and hold F" : $"Eliminate 4 raiders to unlock extraction ({Player.Eliminations}/4)";
        string streakText = UavReady ? "UAV READY (G)" : (uavActive ? $"UAV {uavTimer:0.0}s" : $"Score {Score} Streak {Streak}");
        return $"Ultimate Hybrid Game | HP {Player.Health:0} | STM {Player.Movement.CurrentStamina:0} | Ammo {Player.EquippedWeapon.AmmoInMagazine}/{Player.EquippedWeapon.ReserveAmmo} | {streakText} | Zone {Rules.CurrentZoneRadius:0} | {objective}";
    }

    public float GetExtractionProgress() {
        return extractionProgress / extractionGoal;
    }

    private void HandlePlayerInput(float deltaTime, InputState input) {
        if (input.InventoryPressed) {
            InventoryOpen = !InventoryOpen;
            LogEvent(InventoryOpen ? "Inventory opened." : "Inventory closed.");
        }

        if (InventoryOpen) {
            if (input.ReloadPressed) {
                Player.EquippedWeapon.Reload();
            }
            return;
        }

        Vector3 moveDirection = new Vector3(
            (input.Right ? 1.0f : 0.0f) - (input.Left ? 1.0f : 0.0f),
            (input.Up ? 1.0f : 0.0f) - (input.Down ? 1.0f : 0.0f),
            0.0f);

        if (input.JumpPressed) {
            Player.Jump();
        }

        if (input.CrouchPressed) {
            Player.ToggleCrouch();
        }

        if (input.PronePressed) {
            Player.ToggleProne();
        }

        if (input.VaultPressed) {
            LogEvent("Vault is not implemented yet.");
        }

        Player.Move(moveDirection, deltaTime, input.Sprint, input.Walk, input.AdsHeld, 24.0f, 24.0f, WorldWidth - 24.0f, WorldHeight - 24.0f);

        if (input.ToggleFireModePressed) {
            string mode = Player.EquippedWeapon.ToggleFiringMode();
            LogEvent($"Firing mode: {mode}");
        }

        if (input.ReloadPressed) {
            Player.EquippedWeapon.Reload();
        }

        if (input.QuickMarkerPressed) {
            bool readyBeforeUse = Player.Skills.IsSkillReady;
            Player.Skills.UseActiveSkill(Player.Position, Enemies);
            if (readyBeforeUse) {
                LogEvent("Quick marker: scan pulse.");
            }
        }

        if (input.ScorestreakUavPressed) {
            TryActivateUav();
        }

        bool requestFire = Player.EquippedWeapon.IsAutomatic ? input.FireHeld : input.FirePressed;
        if (requestFire) {
            Enemy? target = GetNearestAliveEnemy();
            if (target != null) {
                bool wasAlive = target.IsAlive;
                bool leanActive = input.LeanLeft || input.LeanRight;
                Player.FireAt(target, Ballistics, input.AdsHeld, input.HipfireAimHeld, input.HoldBreathHeld, leanActive);
                if (wasAlive && !target.IsAlive) {
                    LogEvent($"{target.Name} down. Total eliminations: {Player.Eliminations}.");
                    OnElimination();
                }
            }
        }
    }

    private void UpdateEnemies(float deltaTime) {
        UpdateUav(deltaTime);

        foreach (Enemy enemy in Enemies) {
            if (!enemy.IsAlive) {
                continue;
            }

            Vector3 toPlayer = Player.Position - enemy.Position;
            float distance = toPlayer.Magnitude();
            Vector3 direction = distance > 0.01f ? Vector3.Normalize(toPlayer) : Vector3.Zero;
            float speed = enemy.IsHighlighted ? 88.0f : 106.0f;

            if (distance > 110.0f) {
                enemy.Position += direction * (speed * deltaTime);
            }

            if (enemy.AttackCooldown > 0.0f) {
                enemy.AttackCooldown -= deltaTime;
            }

            if (distance <= 128.0f && enemy.AttackCooldown <= 0.0f) {
                float damage = enemy.IsHighlighted ? 5.0f : 8.0f;
                Player.Health -= damage;
                enemy.AttackCooldown = 1.0f + Random.Shared.NextSingle() * 0.6f;
                LogEvent($"{enemy.Name} hit you for {damage:0}.");
            }
        }

        if (!ExtractionUnlocked && Player.Eliminations >= 4) {
            ExtractionUnlocked = true;
            LogEvent("Extraction unlocked. Move to the VTOL beacon.");
        }
    }

    private void OnElimination() {
        Score += 100;
        Streak += 1;
        if (Streak == 3) {
            LogEvent("Scorestreak earned: UAV (press G).");
        }
    }

    private void TryActivateUav() {
        if (!UavReady) {
            if (uavActive) {
                LogEvent("UAV already active.");
            }
            return;
        }

        uavActive = true;
        uavTimer = uavDuration;
        Score = Math.Max(0, Score - 400);
        LogEvent("UAV online. Enemies revealed.");
        foreach (Enemy enemy in Enemies) {
            if (enemy.IsAlive) {
                enemy.IsUavRevealed = true;
            }
        }
    }

    private void UpdateUav(float deltaTime) {
        if (!uavActive) {
            return;
        }

        uavTimer -= deltaTime;
        if (uavTimer > 0.0f) {
            return;
        }

        uavTimer = 0.0f;
        uavActive = false;
        foreach (Enemy enemy in Enemies) {
            enemy.IsUavRevealed = false;
        }
        LogEvent("UAV expired.");
    }

    private void UpdateExtraction(float deltaTime, InputState input) {
        if (!ExtractionUnlocked) {
            extractionProgress = 0.0f;
            return;
        }

        bool inZone = Player.Position.DistanceTo(ExtractionPoint) <= ExtractionRadius;
        if (inZone && input.InteractHeld) {
            extractionProgress += deltaTime;
            if (extractionProgress >= extractionGoal) {
                EndMatch(true, "Extraction complete. Mission success.");
            }
        } else {
            extractionProgress = MathF.Max(0.0f, extractionProgress - deltaTime * 1.2f);
        }
    }

    private void ResolveZoneDamage(float deltaTime) {
        if (!Rules.IsOutsideZone(Player.Position)) {
            return;
        }

        float damage = Rules.DamagePerSecond * deltaTime;
        Player.Health -= damage;
        LogEvent($"Blue zone burn: {damage:0.0} damage.");
    }

    private void EndMatch(bool success, string result) {
        IsMatchOver = true;
        ExtractionSuccessful = success;
        MatchResult = result;
        LogEvent(result);
    }

    private void SpawnEnemies() {
        Vector3[] spawnPoints = new[] {
            new Vector3(280.0f, 180.0f, 0.0f),
            new Vector3(420.0f, 300.0f, 0.0f),
            new Vector3(680.0f, 180.0f, 0.0f),
            new Vector3(840.0f, 280.0f, 0.0f),
            new Vector3(980.0f, 460.0f, 0.0f),
            new Vector3(760.0f, 560.0f, 0.0f),
            new Vector3(460.0f, 540.0f, 0.0f)
        };

        for (int i = 0; i < spawnPoints.Length; i++) {
            Enemies.Add(new Enemy {
                Name = $"Raider {i + 1}",
                Position = spawnPoints[i]
            });
        }
    }

    private void LogEvent(string message) {
        Console.WriteLine(message);
        eventLog.Enqueue(message);
        while (eventLog.Count > 6) {
            eventLog.Dequeue();
        }
    }
}
