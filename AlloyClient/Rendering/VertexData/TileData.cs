using System;
using System.Runtime.InteropServices;
using Alloy.Engine.Graphics.Buffers;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace AlloyClient.Rendering.VertexData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TileData(Vector4 posOffset, Vector4 uv, Vector4 animate, Vector4 mask) : IVertexData<TileData> {

    public Vector4 Position = posOffset;
    public Vector4 UV = uv;
    public Vector4 Animate = animate;
    public Vector4 Mask = mask;
    public Vector4 Temp;

    public static VertexStride VertexStride { get; } = new([
        new ElementFormat(0, VertexAttribType.Float, FormatType.Vector4),
        new ElementFormat(1, VertexAttribType.Float, FormatType.Vector4),
        new ElementFormat(2, VertexAttribType.Float, FormatType.Vector4),
        new ElementFormat(3, VertexAttribType.Float, FormatType.Vector4),
        new ElementFormat(4, VertexAttribType.Float, FormatType.Vector4),
    ], true);

    public override int GetHashCode() => HashCode.Combine(Position, UV, Animate, Mask, Temp);

    public override string ToString() {
        return "{{TextureCoordinate:" + UV + "}}";
    }

    public static bool operator ==(TileData left, TileData right) {
        return left.UV == right.UV &&
               left.Position == right.Position &&
               left.Animate == right.Animate &&
               left.Mask == right.Mask &&
               left.Temp == right.Temp;
    }

    public static bool operator !=(TileData left, TileData right) {
        return !(left == right);
    }

    public override bool Equals(object obj) {
        return obj != null && !(obj.GetType() != GetType()) && this == (TileData)obj;
    }

    public bool Equals(TileData other) {
        return Position.Equals(other.Position) && UV.Equals(other.UV) && Animate.Equals(other.Animate) && Mask.Equals(other.Mask) && Temp.Equals(other.Temp);
    }
}
