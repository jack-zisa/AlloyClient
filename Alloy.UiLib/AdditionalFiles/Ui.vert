#version 330 core

uniform mat4 ViewMatrix;

layout (location = 0) in vec2 Position;
layout (location = 1) in vec2 UVCoords;
layout (location = 2) in uint VertexColor;
layout (location = 3) in vec2 VertexScale;
layout (location = 4) in float VertexRotation;
layout (location = 5) in vec2 VertexOffset;
layout (location = 6) in vec2 VertexAnchor;
layout (location = 7) in uint InstanceColor;
layout (location = 8) in uint ColorOverride;
layout (location = 9) in vec2 Info;
layout (location = 10) in vec4 Scissor;
layout (location = 11) in vec4 Extra1;
layout (location = 12) in vec4 Extra2;
layout (location = 13) in vec4 ColorTransform;

out VS_OUT {
    vec4 Position1;
    vec2 Info;
    vec2 UVCoords;
    vec4 Scissor;
    vec4 Extra1;
    vec4 Extra2;
    vec4 ColorTransform;
    vec4 Color;
    vec4 Override;
} vsOutput;

vec4 unpackColorVert(uint color) {
    return vec4(
        float(color & 0x000000FFu) / 255.0,
        float((color & 0x0000FF00u) >> 8u) / 255.0,
        float((color & 0x00FF0000u) >> 16u) / 255.0,
        float((color & 0xFF000000u) >> 24u) / 255.0
    );
}

void main() {
    float rotation = VertexRotation;
    vec2 pos = Position + VertexAnchor;
    float x = (pos.x * cos(rotation) - pos.y * sin(rotation) - VertexAnchor.x) * VertexScale.x + VertexOffset.x;
    float y = (pos.x * sin(rotation) + pos.y * cos(rotation) - VertexAnchor.y) * VertexScale.y + VertexOffset.y;
    pos = vec2(x, y);

    gl_Position = vec4(pos, 0, 1) * ViewMatrix;
    vsOutput.Position1 = gl_Position;
    // uint/bool mix() is GLSL 4.50+ only - use a ternary instead
    vsOutput.Color = unpackColorVert((VertexColor == 0u) ? InstanceColor : VertexColor);
    vsOutput.Override = unpackColorVert(ColorOverride);
    vsOutput.Info = Info;
    vsOutput.UVCoords = UVCoords;
    vsOutput.Scissor.xy = (vec4(Scissor.x, Scissor.y, 0, 1) * ViewMatrix).xy;
    vsOutput.Scissor.zw = (vec4(Scissor.z, Scissor.w, 0, 1) * ViewMatrix).xy;
    vsOutput.Extra1 = Extra1;
    vsOutput.Extra2 = Extra2;
    vsOutput.ColorTransform = ColorTransform;
}
