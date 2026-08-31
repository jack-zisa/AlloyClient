#version 330 core

uniform mat4 FullMatrix;
uniform mat4 BillMatrix;

const vec2 particlePos[6] = vec2[6](
    vec2(-0.1, 0.1),
    vec2(0.1, 0.1),
    vec2(-0.1, -0.1),
    vec2(-0.1, -0.1),
    vec2(0.1, 0.1),
    vec2(0.1, -0.1)
);

const vec2 particleUV[6] = vec2[6](
    vec2(0.0, 1.0),
    vec2(1.0, 1.0),
    vec2(0.0, 0.0),
    vec2(0.0, 0.0),
    vec2(1.0, 1.0),
    vec2(1.0, 0.0)
);

layout (location = 0) in vec4 iPosition;
layout (location = 1) in vec4 iColor;

out vec2 BaseUV;
out vec4 Color;
out float Depth;

void main() {
    int verId = gl_VertexID;

    vec4 pos = vec4(particlePos[verId] * iPosition.w, 0, 1.0) * BillMatrix;

    BaseUV = particleUV[verId];
    Color = iColor;

    vec4 depth = vec4(iPosition.xy, 0, 1) * FullMatrix;

    pos.xyz += iPosition.xyz;
    pos = pos * FullMatrix;
    pos.z = 0.5f + 0.4f * depth.y;

    gl_Position = pos;
}
