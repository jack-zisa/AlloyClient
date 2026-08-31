namespace Alloy.Engine.Graphics;

public sealed class VertexArrayObject {

    internal readonly int Handle;

    public VertexArrayObject() {
        Span<int> handle = stackalloc int[1];
        GL.GenVertexArrays(1, handle);
        Handle = handle[0];
    }

    public void Bind() => GL.BindVertexArray(Handle);

    public void Dispose() => GL.DeleteVertexArray(Handle);
}
