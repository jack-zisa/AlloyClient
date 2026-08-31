namespace Alloy.Engine.Graphics.Buffers;

public sealed unsafe class VertexBuffer<T> where T : unmanaged, IVertexData<T> {

    public readonly int Length;

    public readonly int LengthBytes;

    public readonly VertexStride Stride;

    internal readonly int Handle;

    public VertexBuffer(VertexStride stride, int vertexCount) {
        Length = vertexCount;
        LengthBytes = vertexCount * sizeof(T);
        Stride = stride;

        Span<int> handle = stackalloc int[1];
        GL.GenBuffers(1, handle);
        Handle = handle[0];
        GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);
        GL.BufferData(BufferTarget.ArrayBuffer, LengthBytes, IntPtr.Zero, BufferUsage.DynamicDraw);
    }

    public void SetData(ReadOnlySpan<T> data) {
        if (data.Length > Length) {
            throw new Exception("Data larger than buffer");
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);
        GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(T) * data.Length, data);
    }

    public void BindTo(VertexArrayObject vao) {
        vao.Bind();
        GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);
        Stride.BindAttributes(vao);
    }

    public void Delete() => GL.DeleteBuffer(Handle);
}
