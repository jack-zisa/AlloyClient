namespace Alloy.Engine.Graphics.Buffers;

public readonly struct VertexStride {

    public readonly uint Stride;

    public readonly bool Instanced;

    public readonly ElementFormat[] Layout;

    public VertexStride(ElementFormat[] layout, bool instanced = false) {
        Stride = (uint)layout.Sum(e => e.Bytes);
        Layout = layout;
        Instanced = instanced;
    }

    public void BindAttributes(VertexArrayObject vao) {
        var offset = 0;
        for (var i = 0u; i < Layout.Length; i++) {
            var e = Layout[i];

            GL.EnableVertexAttribArray(e.Location);

            switch (e.Type) {
                case VertexAttribType.Byte:
                case VertexAttribType.UnsignedByte:
                case VertexAttribType.Short:
                case VertexAttribType.UnsignedShort:
                case VertexAttribType.Int:
                case VertexAttribType.UnsignedInt:
                    GL.VertexAttribIPointer(e.Location, (int)e.Format, (VertexAttribIType)e.Type, (int)Stride, offset);
                    break;
                case VertexAttribType.Float:
                case VertexAttribType.HalfFloat:
                    GL.VertexAttribPointer(e.Location, (int)e.Format, (VertexAttribPointerType)e.Type, false, (int)Stride, offset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e.Type), e.Type, null);
            }

            if (Instanced) {
                GL.VertexAttribDivisor(e.Location, 1);
            }

            offset += (int)e.Bytes;
        }
    }
}
