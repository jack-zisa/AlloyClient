using System;
using Alloy.Engine.Graphics;
using Alloy.Engine.Graphics.Buffers;
using OpenTK.Graphics.OpenGL;

namespace Alloy.UiLib.Rendering;

public static class SpriteRender {

    private const int InstanceBufferSize = 1000;
    private const int IndexBufferSize = InstanceBufferSize * 6; // Most sprites are a quad which has 6 indices
    private const int VertexBufferSize = InstanceBufferSize * 4; // Most sprites are a quad which has 4 vertices

    private static int _indexCount;
    private static ushort[] _indices;
    private static IndexBuffer _indexBuffer;

    private static ushort _vertexCount;
    private static SpriteVertexData[] _vertices;
    private static VertexBuffer<SpriteVertexData> _vertexBuffer;

    private static VertexArrayObject _vao;

    internal static void Init() {
        _indices = new ushort[IndexBufferSize];
        _indexBuffer = new IndexBuffer(IndexBufferSize);

        _vertices = new SpriteVertexData[VertexBufferSize];
        _vertexBuffer = new VertexBuffer<SpriteVertexData>(SpriteVertexData.VertexStride, VertexBufferSize);

        _vao = new VertexArrayObject();

        _vertexBuffer.BindTo(_vao);
        _indexBuffer.BindTo(_vao);

        GL.BindVertexArray(0);
    }

    internal static void StartDraw() {
        _vao.Bind();

        UiRender.UiShader.Apply();

        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.StencilTest);

        _indexCount = 0;
        _vertexCount = 0;
    }

    internal static void Draw(SpriteInstanceData data, ReadOnlySpan<ushort> indices, ReadOnlySpan<VertexUi> vertices) {
        if (_indexCount + indices.Length > IndexBufferSize || _vertexCount + vertices.Length > VertexBufferSize)
            Flush();

        var numVertices = (ushort)0;

        var len = indices.Length;
        for (var i = 0; i < len; i++) {
            _indices[_indexCount + i] = (ushort)(_vertexCount + indices[i]);
            numVertices = Math.Max(indices[i], numVertices);// Get highest vertex index
        }
        _indexCount += len;

        numVertices++;
        for (var i = 0; i < numVertices; i++) {
            _vertices[_vertexCount + i] = new SpriteVertexData(vertices[i], data);
        }
        _vertexCount += numVertices;

        UiRender.LastRenderCount++;
    }

    internal static void EndDraw() {
        Flush();
        GL.BindVertexArray(0);
    }

    private static void Flush() {
        if (_indexCount < 1) return;

        _indexBuffer.SetData(_indices.AsSpan());
        _vertexBuffer.SetData(_vertices.AsSpan());

        GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedShort, 0);

        _indexCount = 0;
        _vertexCount = 0;
    }
}
