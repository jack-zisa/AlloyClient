using System;
using System.Runtime.InteropServices;
using Alloy.Common;
using Alloy.Engine.Graphics.Buffers;
using Alloy.UiLib.Extra;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Alloy.UiLib.Rendering;

internal readonly record struct SpriteInstanceData(SpriteVertexMatrix Matrix, Color Color, Color ColorOverride, Vector2 Info, Vector4 Scissor, Vector4 Extra1, Vector4 Extra2, ColorTransform ColorTransform);

internal readonly record struct SpriteVertexMatrix(Vector2 Scale, float Rotation, Vector2 Offset, Vector2 Anchor);

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SpriteVertexData(VertexUi vertex, SpriteInstanceData instance) : IVertexData<SpriteVertexData> {

    // Per-vertex
    public Vector2 Position = vertex.Position;
    public Vector2 UV = vertex.UV;
    public uint VertexColor = vertex.Color;

    // Per-instance data, duplicated onto every vertex of the shape
    // (this engine targets GL 3.3 - no SSBO/instance-id indirection available)
    public Vector2 VertexScale = instance.Matrix.Scale;
    public float VertexRotation = instance.Matrix.Rotation;
    public Vector2 VertexOffset = instance.Matrix.Offset;
    public Vector2 VertexAnchor = instance.Matrix.Anchor;
    public uint InstanceColor = instance.Color;
    public uint ColorOverride = instance.ColorOverride;
    public Vector2 Info = instance.Info;
    public Vector4 Scissor = instance.Scissor;
    public Vector4 Extra1 = instance.Extra1;
    public Vector4 Extra2 = instance.Extra2;
    public Vector4 ColorTransformData = instance.ColorTransform;

    public static VertexStride VertexStride { get; } = new([
        new ElementFormat(0, VertexAttribType.Float, FormatType.Vector2),
        new ElementFormat(1, VertexAttribType.Float, FormatType.Vector2),
        new ElementFormat(2, VertexAttribType.UnsignedInt, FormatType.Default),
        new ElementFormat(3, VertexAttribType.Float, FormatType.Vector2),
        new ElementFormat(4, VertexAttribType.Float, FormatType.Default),
        new ElementFormat(5, VertexAttribType.Float, FormatType.Vector2),
        new ElementFormat(6, VertexAttribType.Float, FormatType.Vector2),
        new ElementFormat(7, VertexAttribType.UnsignedInt, FormatType.Default),
        new ElementFormat(8, VertexAttribType.UnsignedInt, FormatType.Default),
        new ElementFormat(9, VertexAttribType.Float, FormatType.Vector2),
        new ElementFormat(10, VertexAttribType.Float, FormatType.Vector4),
        new ElementFormat(11, VertexAttribType.Float, FormatType.Vector4),
        new ElementFormat(12, VertexAttribType.Float, FormatType.Vector4),
        new ElementFormat(13, VertexAttribType.Float, FormatType.Vector4),
    ]);

    public bool Equals(SpriteVertexData other) {
        return Position == other.Position &&
               UV.Equals(other.UV) &&
               VertexColor == other.VertexColor &&
               InstanceColor == other.InstanceColor;
    }

    public override bool Equals(object obj) {
        return obj is SpriteVertexData other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Position, UV, VertexColor, InstanceColor);
    }
}

public struct VertexUi {

    public Vector2 Position;
    public Vector2 UV;
    public Color Color;

    public VertexUi(Vector2 pos, Vector2 uv, Color color) {
        Position = pos;
        UV = uv;
        Color = color;
    }

    public VertexUi(Vector2 pos, Vector2 uv) {
        Position = pos;
        UV = uv;
        Color = new Color(0);
    }

    public VertexUi(Vector2 pos, Color color) {
        Position = pos;
        UV = new Vector2(0f);
        Color = color;
    }

    public VertexUi(Vector2 pos) {
        Position = pos;
        UV = new Vector2(0f);
        Color = new Color(0);
    }
}
