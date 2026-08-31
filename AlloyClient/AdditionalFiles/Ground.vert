#version 330 core

uniform mat4 FullMatrix;
uniform float GameTime;

const vec2 tilePos[6] = vec2[6](
    vec2(0, 1),
    vec2(1, 1),
    vec2(0, 0),
    vec2(0, 0),
    vec2(1, 1),
    vec2(1, 0)
);

const vec2 tileUV[6] = vec2[6](
    vec2(0.0, 1.0),
    vec2(1.0, 1.0),
    vec2(0.0, 0.0),
    vec2(0.0, 0.0),
    vec2(1.0, 1.0),
    vec2(1.0, 0.0)
);

layout (location = 0) in vec4 iPosition;
layout (location = 1) in vec4 iUV;
layout (location = 2) in vec4 iAnimate;
layout (location = 3) in vec4 iMask;
layout (location = 4) in vec4 iTemp;

out GROUND_OUTPUT {
    vec2 baseUV;
    vec2 coreUV;
    vec4 UV;
    vec4 Mask;
    float Swizzle;
} vsOutput;

void main() {
    int verId = gl_VertexID;

    vec4 inputPosition = vec4(tilePos[verId], 0, 1);
    inputPosition.xy = (inputPosition.xy - 0.5) * 1.002 + 0.5;
    inputPosition.xy += iPosition.xy;
    gl_Position = inputPosition * FullMatrix;

    vsOutput.baseUV = tileUV[verId];
    vsOutput.coreUV.x = tileUV[verId].x + iPosition.z + sin(GameTime * iAnimate.x) + GameTime * iAnimate.z;
    vsOutput.coreUV.y = tileUV[verId].y + iPosition.w + sin(GameTime * iAnimate.y) + GameTime * iAnimate.w;
    vsOutput.UV = iUV;
    vsOutput.Mask = iMask;
    vsOutput.Swizzle = iTemp.x;
}
