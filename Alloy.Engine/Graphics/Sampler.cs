namespace Alloy.Engine.Graphics;

public sealed class Sampler {

    internal readonly int Handle;

    internal readonly int TextureHandle;

    internal uint TextureUnit;

    public Sampler(Texture texture) {
        Handle = GenSampler();
        TextureHandle = texture.Handle;
        SetFilter(TextureFilter.Nearest);
    }

    public Sampler(Texture texture, uint textureUnit) {
        Handle = GenSampler();
        TextureHandle = texture.Handle;
        SetFilter(TextureFilter.Nearest);
        Bind(textureUnit);
    }

    public Sampler(Texture texture, TextureFilter filter) {
        Handle = GenSampler();
        TextureHandle = texture.Handle;
        SetFilter(filter);
    }

    public Sampler(Texture texture, TextureFilter filter, uint textureUnit) {
        Handle = GenSampler();
        TextureHandle = texture.Handle;
        Bind(textureUnit);
        SetFilter(filter);
    }

    private static int GenSampler() {
        Span<int> handle = stackalloc int[1];
        GL.GenSamplers(1, handle);
        return handle[0];
    }

    public void Bind(uint textureUnit) {
        if (textureUnit > 15) {
            throw new ArgumentOutOfRangeException(nameof(textureUnit), textureUnit, null);
        }


        TextureUnit = textureUnit;
        GL.ActiveTexture((OpenTK.Graphics.OpenGL.TextureUnit)((int)OpenTK.Graphics.OpenGL.TextureUnit.Texture0 + (int)textureUnit));
        GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
        GL.BindSampler(textureUnit, Handle);
    }

    public void SetFilter(TextureFilter filter) {
        GL.SamplerParameterIi(Handle, SamplerParameterI.TextureMagFilter, in filter.MagFilter);
        GL.SamplerParameterIi(Handle, SamplerParameterI.TextureMinFilter, in filter.MinFilter);
    }

    public void Delete() => GL.DeleteSampler(Handle);

}
