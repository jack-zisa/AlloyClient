using Alloy.Common.SourceGen;
using Alloy.Engine.Graphics;
using Alloy.Engine.Graphics.Buffers;
using AlloyClient.Assets;
using AlloyClient.Game;
using AlloyClient.Rendering.VertexData;
using Alloy.Engine;
using Alloy.UiLib.Data;
using OpenTK.Mathematics;

namespace AlloyClient.Rendering;

public static partial class Render {
    
    private const int BufferSize = 10000;
    public const int TileBufferSize = Map.VisibleChunks * TileMap.ChunkArea * 4;
    private const int ShadowBufferSize = 4096;

    // Shader Sources
    [Shader("Ground")] private static partial ShaderSource GroundShaderSource { get; }
    [Shader("Shadow")] private static partial ShaderSource ShadowShaderSource { get; }
    [Shader("Model")] private static partial ShaderSource ModelShaderSource { get; }
    [Shader("Object")] private static partial ShaderSource ObjectShaderSource { get; }
    [Shader("Particle")] private static partial ShaderSource ParticleShaderSource { get; }
    
    // Shaders
    private static Shader _shaderGround;
    private static Shader _shaderShadow;
    private static Shader _shaderModel;
    private static Shader _shaderObject;
    private static Shader _shaderParticle;
    
    // Vertex Objects
    private static VertexArrayObject _groundVao;
    private static VertexArrayObject _shadowVao;
    private static VertexArrayObject _objectVao;
    private static VertexArrayObject _modelVao;

    // Buffers
    private static IndexBuffer _modelIndexBuffer;
    private static VertexBuffer<VertexBase> _modelVertexBuffer;

    private static TileData[] _tileData;
    private static VertexBuffer<TileData> _tileBuffer;

    private static ShadowData[] _shadowData;
    private static VertexBuffer<ShadowData> _shadowBuffer;

    private static VertexModel[] _modelData;
    private static VertexBuffer<VertexModel> _modelDataBuffer;

    private static VertexObject[] _entityData;
    private static VertexBuffer<VertexObject> _entityDataBuffer;

    public static void FirstTimeInit(Sampler atlas, BitmapFamily font) {
        // Shaders
        _shaderGround = Shader.FromSource(GroundShaderSource);
        _shaderGround.SetValue("GameTexture", atlas);

        _shaderShadow = Shader.FromSource(ShadowShaderSource);

        _shaderModel = Shader.FromSource(ModelShaderSource);
        _shaderModel.SetValue("GameTexture", atlas);

        _shaderObject = Shader.FromSource(ObjectShaderSource);
        _shaderObject.SetValue("GameTexture", atlas);

        _shaderObject.SetValue("PixelRange", font.PixelRange);
        _shaderObject.SetValue("TextTextureSize", new Vector2(font.Atlas.Width, font.Atlas.Height));
        _shaderObject.SetValue("TextTexture", font.Sampler);

        _shaderParticle = Shader.FromSource(ParticleShaderSource);

        _tileData = new TileData[TileBufferSize];
        _tileBuffer = new VertexBuffer<TileData>(TileData.VertexStride, _tileData.Length);
        _groundVao = new VertexArrayObject();
        _tileBuffer.BindTo(_groundVao);

        _shadowData = new ShadowData[ShadowBufferSize];
        _shadowBuffer = new VertexBuffer<ShadowData>(ShadowData.VertexStride, _shadowData.Length);
        _shadowVao = new VertexArrayObject();
        _shadowBuffer.BindTo(_shadowVao);

        _modelIndexBuffer = new IndexBuffer(ModelData.Indices.Length);
        _modelIndexBuffer.SetData(ModelData.Indices);
        _modelVertexBuffer = new VertexBuffer<VertexBase>(VertexBase.VertexStride, ModelData.Vertices.Length);
        _modelVertexBuffer.SetData(ModelData.Vertices);

        _modelVao = new VertexArrayObject();

        _modelData = new VertexModel[BufferSize];
        _modelDataBuffer = new VertexBuffer<VertexModel>(VertexModel.VertexStride, BufferSize);
        _modelIndexBuffer.BindTo(_modelVao);
        _modelVertexBuffer.BindTo(_modelVao);
        _modelDataBuffer.BindTo(_modelVao);


        _entityData = new VertexObject[BufferSize];
        _entityDataBuffer = new VertexBuffer<VertexObject>(VertexObject.VertexStride, BufferSize);
        _objectVao = new VertexArrayObject();
        _entityDataBuffer.BindTo(_objectVao);

        BuildParticleBuffers();
    }
    
    public static void SetShaderParams(GameTime gameTime, Camera camera) {
        _shaderGround.SetValue("FullMatrix", camera.Matrix);
        _shaderGround.SetValue("GameTime", (float)(gameTime.TotalMs / 1000.0f));
        
        _shaderShadow.SetValue("FullMatrix", camera.Matrix);
        _shaderShadow.SetValue("BillMatrix", camera.BillboardMatrix);
        
        _shaderModel.SetValue("FullMatrix", camera.Matrix);
        
        _shaderObject.SetValue("FullMatrix", camera.Matrix);
        _shaderObject.SetValue("BillMatrix", camera.BillboardMatrix);
        _shaderObject.SetValue("Zoom", Settings.CameraZoom);
        
        _shaderParticle.SetValue("FullMatrix", camera.Matrix);
        _shaderParticle.SetValue("BillMatrix", camera.BillboardMatrix);
    }
}