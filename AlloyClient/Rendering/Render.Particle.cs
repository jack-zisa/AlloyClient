using System;
using Alloy.Engine.Graphics;
using Alloy.Engine.Graphics.Buffers;
using AlloyClient.Rendering.VertexData;
using OpenTK.Graphics.OpenGL;

namespace AlloyClient.Rendering;

public static partial class Render {

    public static int LastDrawParticleCount;

    private const int Buffer = 2000;

    private static VertexArrayObject _particleVao;
    private static VertexBuffer<ParticleData> _particleBuffer;

    private static void BuildParticleBuffers() {
        _particleBuffer = new VertexBuffer<ParticleData>(ParticleData.VertexStride, Buffer);
        _particleVao = new VertexArrayObject();
        _particleBuffer.BindTo(_particleVao);
    }

    public static void DrawParticles(ParticleData[] particles, int count) {
        if (count < 1) return;

        LastDrawParticleCount= 0;

        _particleVao.Bind();
        _shaderParticle.Apply();

        var startIndex = 0;
        while (count > Buffer) {
            _particleBuffer.SetData(new ReadOnlySpan<ParticleData>(particles, startIndex, Buffer));
            startIndex += Buffer;
            count -= Buffer;
            FlushBufferParticle(Buffer);
        }

        _particleBuffer.SetData(new ReadOnlySpan<ParticleData>(particles, startIndex, count));
        FlushBufferParticle(count);
    }

    private static void FlushBufferParticle(int count) {
        if (count < 1) return;

        LastDrawParticleCount += count;

        GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, count);
    }

}
