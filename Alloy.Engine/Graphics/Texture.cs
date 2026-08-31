using Alloy.Common;
using ReFuel.Stb;

namespace Alloy.Engine.Graphics;

public sealed class Texture {

    // non-DSA glBindTexture binds to whatever unit glActiveTexture last touched,
    // and nothing re-selects a unit after startup, so uploads were landing on
    // whichever Sampler happened to be active last (minimap updates were stealing
    // the font atlas's unit every frame). Give uploads their own unit so they
    // can't step on a live binding.
    private const int UploadTextureUnit = 15;

    public readonly int Width;

    public readonly int Height;

    internal readonly int Handle;

    public Texture(string file) : this(File.ReadAllBytes(file)) { }

    public Texture(ReadOnlySpan<byte> data) : this(StbImage.Load(data, StbiImageFormat.Rgba)) { }

    public Texture(StbImage image) : this(image.AsSpan<Color>(), image.Width, image.Height) { }

    public Texture(ReadOnlySpan<Color> data, int width, int height) {
        Width = width;
        Height = height;

        Span<int> handle = stackalloc int[1];
        GL.GenTextures(1, handle);
        Handle = handle[0];

        GL.ActiveTexture(TextureUnit.Texture0 + UploadTextureUnit);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
    }

    public void SetData(ReadOnlySpan<Color> data, Vector4i rect) {
        GL.ActiveTexture(TextureUnit.Texture0 + UploadTextureUnit);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, rect.X, rect.Y, rect.Z, rect.W, PixelFormat.Rgba, PixelType.UnsignedByte, data);
    }

    public void SetData(ReadOnlySpan<Color> data, int width, int height) {
        GL.ActiveTexture(TextureUnit.Texture0 + UploadTextureUnit);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
    }
}
