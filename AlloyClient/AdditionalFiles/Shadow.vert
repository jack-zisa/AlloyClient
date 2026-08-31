#version 330 core

uniform mat4 FullMatrix;
uniform mat4 BillMatrix;

const vec2 shadowPos[6] = vec2[6](
    vec2(-0.5, 0.25),
    vec2(0.5, 0.25),
    vec2(-0.5, -0.25),
    vec2(-0.5, -0.25),
    vec2(0.5, 0.25),
    vec2(0.5, -0.25)
);

const vec2 shadowUV[6] = vec2[6](
    vec2(0.0, 1.0),
    vec2(1.0, 1.0),
    vec2(0.0, 0.0),
    vec2(0.0, 0.0),
    vec2(1.0, 1.0),
    vec2(1.0, 0.0)
);

layout (location = 0) in vec3 iPosScale; // xy = Position, z = Scale
layout (location = 1) in uint iColor;

out vec2 BaseUV;
out flat uint Color;

void main() {
    int verId = gl_VertexID;

    vec4 pos = vec4(shadowPos[verId] * iPosScale.z, 0, 1) * BillMatrix;
    pos.xy += iPosScale.xy;

    gl_Position = pos * FullMatrix;

    BaseUV = shadowUV[verId];
    Color = iColor;
}
