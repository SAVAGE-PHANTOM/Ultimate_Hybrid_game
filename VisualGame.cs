using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class VisualGame : GameWindow {
    private readonly GameEngine engine;
    private int shaderProgram;
    private int vao;
    private int vbo;
    private int projectionLocation;
    private Matrix4 projection;
    private KeyboardState? previousKeyboard;
    private MouseState? previousMouse;
    private bool hipfireAimToggled = false;

    public VisualGame(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings) {
        engine = new GameEngine();
    }

    protected override void OnLoad() {
        base.OnLoad();
        GL.ClearColor(0.03f, 0.05f, 0.08f, 1.0f);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        shaderProgram = CreateShaderProgram();
        projection = Matrix4.CreateOrthographicOffCenter(0.0f, engine.WorldWidth, 0.0f, engine.WorldHeight, -1.0f, 1.0f);
        projectionLocation = GL.GetUniformLocation(shaderProgram, "uProjection");

        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 6 * sizeof(float), 2 * sizeof(float));

        engine.StartMatch();
    }

    protected override void OnUpdateFrame(FrameEventArgs args) {
        base.OnUpdateFrame(args);
        if (KeyboardState.IsKeyDown(Keys.Escape)) {
            Close();
            return;
        }

        if (Pressed(Keys.CapsLock)) {
            hipfireAimToggled = !hipfireAimToggled;
        }

        int zoomDelta = 0;
        if (MouseState.ScrollDelta.Y > 0.0f) {
            zoomDelta = 1;
        } else if (MouseState.ScrollDelta.Y < 0.0f) {
            zoomDelta = -1;
        }

        bool adsHeld = MouseState.IsButtonDown(MouseButton.Right);
        bool sprintHeld = KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);

        InputState input = new InputState {
            Up = KeyboardState.IsKeyDown(Keys.W),
            Down = KeyboardState.IsKeyDown(Keys.S),
            Left = KeyboardState.IsKeyDown(Keys.A),
            Right = KeyboardState.IsKeyDown(Keys.D),
            Sprint = sprintHeld,
            Walk = KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl),
            JumpPressed = Pressed(Keys.Space),
            VaultPressed = Pressed(Keys.V),
            CrouchPressed = Pressed(Keys.C),
            PronePressed = Pressed(Keys.Z),
            FireHeld = MouseState.IsButtonDown(MouseButton.Left),
            FirePressed = MousePressed(MouseButton.Left),
            AdsHeld = adsHeld,
            HipfireAimHeld = hipfireAimToggled,
            LeanLeft = KeyboardState.IsKeyDown(Keys.Q),
            LeanRight = KeyboardState.IsKeyDown(Keys.E),
            HoldBreathHeld = adsHeld && sprintHeld,
            ToggleFireModePressed = Pressed(Keys.B),
            ZoomDelta = zoomDelta,
            InventoryPressed = Pressed(Keys.Tab) || Pressed(Keys.I),
            ReloadPressed = Pressed(Keys.R),
            InteractHeld = KeyboardState.IsKeyDown(Keys.F),
            QuickMarkerPressed = MousePressed(MouseButton.Middle),
            ScorestreakUavPressed = Pressed(Keys.G)
        };

        engine.Update((float)args.Time, input);
        Title = engine.GetStatusText();
        previousKeyboard = KeyboardState;
        previousMouse = MouseState;
    }

    protected override void OnRenderFrame(FrameEventArgs args) {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.UseProgram(shaderProgram);
        GL.UniformMatrix4(projectionLocation, false, ref projection);

        DrawArena();
        DrawZone();
        DrawPoiBlocks();
        DrawExtraction();
        DrawEnemies();
        DrawPlayer();
        DrawHudBars();

        SwapBuffers();
    }

    private void DrawArena() {
        DrawRectangle(0.0f, 0.0f, engine.WorldWidth, engine.WorldHeight, new Color4(0.05f, 0.09f, 0.12f, 1.0f));
        DrawRectangle(22.0f, 22.0f, engine.WorldWidth - 44.0f, engine.WorldHeight - 44.0f, new Color4(0.08f, 0.12f, 0.16f, 1.0f));
    }

    private void DrawZone() {
        DrawCircle(engine.Rules.ZoneCenter.X, engine.Rules.ZoneCenter.Y, engine.Rules.CurrentZoneRadius, new Color4(0.18f, 0.43f, 0.67f, 0.10f), 72);
        DrawRing(engine.Rules.ZoneCenter.X, engine.Rules.ZoneCenter.Y, engine.Rules.CurrentZoneRadius, 4.0f, new Color4(0.32f, 0.78f, 0.95f, 0.75f), 72);
    }

    private void DrawPoiBlocks() {
        DrawRectangle(170.0f, 110.0f, 140.0f, 90.0f, new Color4(0.33f, 0.36f, 0.44f, 1.0f));
        DrawRectangle(520.0f, 250.0f, 160.0f, 105.0f, new Color4(0.42f, 0.35f, 0.48f, 1.0f));
        DrawRectangle(900.0f, 440.0f, 130.0f, 110.0f, new Color4(0.44f, 0.31f, 0.30f, 1.0f));
        DrawRectangle(760.0f, 120.0f, 110.0f, 70.0f, new Color4(0.22f, 0.39f, 0.39f, 1.0f));
    }

    private void DrawPlayer() {
        Color4 playerColor = engine.Player.Movement.IsSliding
            ? new Color4(0.98f, 0.78f, 0.24f, 1.0f)
            : new Color4(0.95f, 0.58f, 0.10f, 1.0f);

        DrawCircle(engine.Player.Position.X, engine.Player.Position.Y, engine.Player.Radius, playerColor, 30);
        DrawRing(engine.Player.Position.X, engine.Player.Position.Y, engine.Player.Radius + 5.0f, 2.0f, new Color4(1.0f, 0.9f, 0.6f, 0.35f), 30);
    }

    private void DrawEnemies() {
        foreach (Enemy enemy in engine.Enemies) {
            if (!enemy.IsAlive) {
                DrawRectangle(enemy.Position.X - 12.0f, enemy.Position.Y - 3.0f, 24.0f, 6.0f, new Color4(0.25f, 0.1f, 0.1f, 1.0f));
                continue;
            }

            bool revealed = enemy.IsHighlighted || enemy.IsUavRevealed;
            Color4 color = revealed
                ? new Color4(0.95f, 0.15f, 0.18f, 1.0f)
                : new Color4(0.35f, 0.88f, 0.38f, 1.0f);

            DrawCircle(enemy.Position.X, enemy.Position.Y, 14.0f, color, 28);
            DrawRectangle(enemy.Position.X - 18.0f, enemy.Position.Y + 18.0f, 36.0f, 5.0f, new Color4(0.20f, 0.20f, 0.20f, 1.0f));
            DrawRectangle(enemy.Position.X - 18.0f, enemy.Position.Y + 18.0f, 36.0f * Math.Clamp(enemy.Health / 100.0f, 0.0f, 1.0f), 5.0f, new Color4(0.85f, 0.22f, 0.18f, 1.0f));
        }
    }

    private void DrawExtraction() {
        Color4 beaconColor = engine.ExtractionUnlocked
            ? new Color4(0.30f, 0.92f, 0.78f, 0.24f)
            : new Color4(0.45f, 0.45f, 0.45f, 0.12f);

        DrawCircle(engine.ExtractionPoint.X, engine.ExtractionPoint.Y, engine.ExtractionRadius, beaconColor, 40);
        DrawRing(engine.ExtractionPoint.X, engine.ExtractionPoint.Y, engine.ExtractionRadius + 6.0f, 3.0f, new Color4(0.75f, 0.95f, 0.90f, 0.9f), 40);

        if (engine.ExtractionUnlocked) {
            DrawRectangle(engine.ExtractionPoint.X - 26.0f, engine.ExtractionPoint.Y - 48.0f, 52.0f, 8.0f, new Color4(0.16f, 0.19f, 0.22f, 1.0f));
            DrawRectangle(engine.ExtractionPoint.X - 26.0f, engine.ExtractionPoint.Y - 48.0f, 52.0f * Math.Clamp(engine.GetExtractionProgress(), 0.0f, 1.0f), 8.0f, new Color4(0.26f, 0.90f, 0.72f, 1.0f));
        }
    }

    private void DrawHudBars() {
        DrawHudBar(20.0f, engine.WorldHeight - 28.0f, 220.0f, 10.0f, engine.Player.Health / 100.0f, new Color4(0.90f, 0.22f, 0.18f, 1.0f));
        DrawHudBar(20.0f, engine.WorldHeight - 46.0f, 220.0f, 10.0f, engine.Player.Movement.CurrentStamina / engine.Player.Movement.MaxStamina, new Color4(0.98f, 0.72f, 0.18f, 1.0f));
        DrawHudBar(20.0f, engine.WorldHeight - 64.0f, 220.0f, 10.0f, engine.Rules.CurrentZoneRadius / engine.Rules.CircleRadius, new Color4(0.24f, 0.76f, 0.98f, 1.0f));

        if (engine.IsMatchOver) {
            DrawRectangle(engine.WorldWidth * 0.24f, engine.WorldHeight * 0.43f, engine.WorldWidth * 0.52f, 110.0f, new Color4(0.02f, 0.04f, 0.06f, 0.82f));
            DrawRectangle(engine.WorldWidth * 0.26f, engine.WorldHeight * 0.47f, engine.WorldWidth * 0.48f, 28.0f, engine.ExtractionSuccessful
                ? new Color4(0.24f, 0.82f, 0.63f, 0.90f)
                : new Color4(0.82f, 0.24f, 0.24f, 0.90f));
        }
    }

    private void DrawHudBar(float x, float y, float width, float height, float normalizedValue, Color4 fillColor) {
        DrawRectangle(x, y, width, height, new Color4(0.12f, 0.15f, 0.18f, 0.92f));
        DrawRectangle(x, y, width * Math.Clamp(normalizedValue, 0.0f, 1.0f), height, fillColor);
    }

    private bool Pressed(Keys key) {
        return KeyboardState.IsKeyDown(key) && !(previousKeyboard?.IsKeyDown(key) ?? false);
    }

    private bool MousePressed(MouseButton button) {
        return MouseState.IsButtonDown(button) && !(previousMouse?.IsButtonDown(button) ?? false);
    }

    private void DrawRectangle(float x, float y, float width, float height, Color4 color) {
        float[] vertices = {
            x, y, color.R, color.G, color.B, color.A,
            x + width, y, color.R, color.G, color.B, color.A,
            x, y + height, color.R, color.G, color.B, color.A,
            x + width, y, color.R, color.G, color.B, color.A,
            x + width, y + height, color.R, color.G, color.B, color.A,
            x, y + height, color.R, color.G, color.B, color.A,
        };

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    private void DrawCircle(float centerX, float centerY, float radius, Color4 color, int segments) {
        float[] vertices = new float[(segments + 2) * 6];
        vertices[0] = centerX;
        vertices[1] = centerY;
        vertices[2] = color.R;
        vertices[3] = color.G;
        vertices[4] = color.B;
        vertices[5] = color.A;

        for (int i = 0; i <= segments; i++) {
            float angle = MathHelper.TwoPi * i / segments;
            int index = (i + 1) * 6;
            vertices[index] = centerX + MathF.Cos(angle) * radius;
            vertices[index + 1] = centerY + MathF.Sin(angle) * radius;
            vertices[index + 2] = color.R;
            vertices[index + 3] = color.G;
            vertices[index + 4] = color.B;
            vertices[index + 5] = color.A;
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);
        GL.DrawArrays(PrimitiveType.TriangleFan, 0, segments + 2);
    }

    private void DrawRing(float centerX, float centerY, float radius, float thickness, Color4 color, int segments) {
        for (int i = 0; i < segments; i++) {
            float startAngle = MathHelper.TwoPi * i / segments;
            float endAngle = MathHelper.TwoPi * (i + 1) / segments;
            Vector2 innerStart = new Vector2(centerX + MathF.Cos(startAngle) * (radius - thickness), centerY + MathF.Sin(startAngle) * (radius - thickness));
            Vector2 outerStart = new Vector2(centerX + MathF.Cos(startAngle) * radius, centerY + MathF.Sin(startAngle) * radius);
            Vector2 innerEnd = new Vector2(centerX + MathF.Cos(endAngle) * (radius - thickness), centerY + MathF.Sin(endAngle) * (radius - thickness));
            Vector2 outerEnd = new Vector2(centerX + MathF.Cos(endAngle) * radius, centerY + MathF.Sin(endAngle) * radius);

            float[] vertices = {
                innerStart.X, innerStart.Y, color.R, color.G, color.B, color.A,
                outerStart.X, outerStart.Y, color.R, color.G, color.B, color.A,
                innerEnd.X, innerEnd.Y, color.R, color.G, color.B, color.A,
                outerStart.X, outerStart.Y, color.R, color.G, color.B, color.A,
                outerEnd.X, outerEnd.Y, color.R, color.G, color.B, color.A,
                innerEnd.X, innerEnd.Y, color.R, color.G, color.B, color.A,
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }
    }

    private int CreateShaderProgram() {
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, @"#version 330 core
layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec4 aColor;
uniform mat4 uProjection;
out vec4 vColor;
void main() {
    vColor = aColor;
    gl_Position = uProjection * vec4(aPosition, 0.0, 1.0);
}");
        GL.CompileShader(vertexShader);
        CheckShaderCompile(vertexShader);

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, @"#version 330 core
in vec4 vColor;
out vec4 FragColor;
void main() {
    FragColor = vColor;
}");
        GL.CompileShader(fragmentShader);
        CheckShaderCompile(fragmentShader);

        int program = GL.CreateProgram();
        GL.AttachShader(program, vertexShader);
        GL.AttachShader(program, fragmentShader);
        GL.LinkProgram(program);
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int status);
        if (status == 0) {
            throw new Exception($"Shader program link failed: {GL.GetProgramInfoLog(program)}");
        }

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
        return program;
    }

    private static void CheckShaderCompile(int shader) {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
        if (status == 0) {
            throw new Exception($"Shader compile failed: {GL.GetShaderInfoLog(shader)}");
        }
    }

    protected override void OnUnload() {
        base.OnUnload();
        GL.DeleteBuffer(vbo);
        GL.DeleteVertexArray(vao);
        GL.DeleteProgram(shaderProgram);
    }
}
