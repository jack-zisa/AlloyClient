using Alloy.Common.SourceGen;
using Alloy.Engine.Graphics.Buffers;

namespace Alloy.Engine.Graphics;

public sealed class Shader {

    internal record struct UniformInfo(int Location, UniformType Type, int Size);

    public readonly string Name;

    internal readonly int Handle;

    private readonly Dictionary<string, UniformInfo> _uniforms = new();

    public Shader(string path, (string, string)[] defines = null) {
        Name = new DirectoryInfo(path).Name;
        Handle = GL.CreateProgram();

        ShaderHelper.Compile(Handle, Name, path, defines);
        ShaderHelper.LoadUniformProperties(Handle, _uniforms);
    }

    public Shader(ShaderSource source, (string, string)[] defines = null) {
        Name = source.Name;
        Handle = GL.CreateProgram();

        ShaderHelper.Compile(Handle, source.Name, source.Vertex, source.Fragment, defines);
        ShaderHelper.LoadUniformProperties(Handle, _uniforms);
    }

    public void Apply() => GL.UseProgram(Handle);

    public void SetValue(string uniform, Matrix4 matrix) {
        GL.UseProgram(Handle);
        GL.UniformMatrix4f(GetLocation(uniform, UniformType.FloatMat4), 1, true, in matrix);
    }

    public void SetValue(string uniform, float value) {
        GL.UseProgram(Handle);
        GL.Uniform1f(GetLocation(uniform, UniformType.Float), value);
    }

    public void SetValue(string uniform, int value) {
        GL.UseProgram(Handle);
        GL.Uniform1i(GetLocation(uniform, UniformType.Int), value);
    }

    public void SetValue(string uniform, Vector2 value) {
        GL.UseProgram(Handle);
        GL.Uniform2f(GetLocation(uniform, UniformType.FloatVec2), 1, in value);
    }

    public void SetValue(string uniform, Sampler sampler) {
        GL.UseProgram(Handle);
        GL.Uniform1i(GetLocation(uniform, UniformType.Sampler2D), (int)sampler.TextureUnit);
    }

    private int GetLocation(string uniform, UniformType type) {
        if (!_uniforms.TryGetValue(uniform, out var info)) {
            throw new Exception($"Unable to find uniform <{uniform}> in shader <{Name}>");
        }

        if (type != info.Type) {
            throw new Exception($"Value does not match the type of uniform <{uniform}>");
        }

        return info.Location;
    }

    public static Shader FromSource(ShaderSource source, (string, string)[] defines = null) => new Shader(source, defines);
}
