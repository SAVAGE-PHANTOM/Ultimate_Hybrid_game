using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

public class VisualGame : GameWindow {
    private readonly GameEngine engine;
    private int shaderProgram;
    private int vao;
    private int vbo;
    private int projectionLocation;
    private Matrix4 projection;
    private readonly float worldWidth = 1200.0f;
    private readonly float worldHeight = 600.0f;

    public VisualGame(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings) {
        engine = new GameEngine();
    }

    protected override void OnLoad() {
        base.OnLoad();
        GL.ClearColor(0.05f, 0.08f, 0.12f, 1.0f);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        shaderProgram = CreateShaderProgram();
        projection = Matrix4.CreateOrthographicOffCenter(0.0f, worldWidth, 0.0f, worldHeight, -1.0f, 1.0f);
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
        engine.Update((float)args.Time);
        if (engine.IsMatchOver) {
            Close();
        }
    }

    protected override void OnRenderFrame(FrameEventArgs args) {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.UseProgram(shaderProgram);
        GL.UniformMatrix4(projectionLocation, false, ref projection);

        DrawArena();
        DrawEnvironments();
        DrawPlayer();
        DrawEnemies();

        SwapBuffers();
    }

    private void DrawArena() {
        DrawRectangle(0, 0, worldWidth, worldHeight, new Color4(0.08f, 0.12f, 0.18f, 1.0f));
        float zone = MathF.Max(100.0f, engine.Rules.CurrentZoneRadius / 4.0f);
        DrawRectangle(0, 0, zone * 1.2f, worldHeight, new Color4(0.08f, 0.18f, 0.14f, 0.4f));
    }

    private void DrawEnvironments() {
        DrawRectangle(200, 0, 120, 80, new Color4(0.4f, 0.4f, 0.5f, 1.0f));
        DrawRectangle(520, 0, 140, 100, new Color4(0.45f, 0.38f, 0.52f, 1.0f));
        DrawRectangle(820, 0, 160, 90, new Color4(0.5f, 0.32f, 0.35f, 1.0f));
    }

    private void DrawPlayer() {
        var position = engine.Player.Position;
        float px = MathF.Min(worldWidth - 35.0f, MathF.Max(10.0f, position.X));
        float py = worldHeight * 0.45f;
        DrawRectangle(px, py, 30.0f, 30.0f, new Color4(0.95f, 0.6f, 0.1f, 1.0f));
    }

    private void DrawEnemies() {
        foreach (var enemy in engine.Enemies) {
            float ex = MathF.Min(worldWidth - 25.0f, MathF.Max(10.0f, enemy.Position.X));
            float ey = worldHeight * 0.55f;
            var color = enemy.IsHighlighted ? new Color4(0.9f, 0.15f, 0.15f, 1.0f) : new Color4(0.4f, 0.8f, 0.35f, 1.0f);
            DrawRectangle(ex, ey, 24.0f, 24.0f, color);
        }
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
            string info = GL.GetProgramInfoLog(program);
            throw new Exception($"Shader program link failed: {info}");
        }

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
        return program;
    }

    private static void CheckShaderCompile(int shader) {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
        if (status == 0) {
            string info = GL.GetShaderInfoLog(shader);
            throw new Exception($"Shader compile failed: {info}");
        }
    }

    protected override void OnUnload() {
        base.OnUnload();
        GL.DeleteBuffer(vbo);
        GL.DeleteVertexArray(vao);
        GL.DeleteProgram(shaderProgram);
    }
}
