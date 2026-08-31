#version 330 core

uniform mat4 FullMatrix;
uniform mat4 BillMatrix;
uniform int RenderPass;

const int OpaquePass = 0;
const int OutlineGlowPass = 1;

const vec2 objPos[6] = vec2[6](
    vec2(-0.5, 0.5),
    vec2(0.5, 0.5),
    vec2(-0.5, -0.5),
    vec2(-0.5, -0.5),
    vec2(0.5, 0.5),
    vec2(0.5, -0.5)
);

const vec2 objUV[6] = vec2[6](
    vec2(0.0, 1.0),
    vec2(1.0, 1.0),
    vec2(0.0, 0.0),
    vec2(0.0, 0.0),
    vec2(1.0, 1.0),
    vec2(1.0, 0.0)
);

layout (location = 0) in vec4 iPosition;
layout (location = 1) in vec4 iUV;
layout (location = 2) in vec4 iScale;
layout (location = 3) in vec4 iRotation;
layout (location = 4) in vec4 iExtra;
layout (location = 5) in vec4 iColor;
layout (location = 6) in vec4 iMask1;
layout (location = 7) in vec4 iMask2;

out OBJECT_OUT {
    vec2 BaseUV;
    vec4 UV;
    vec4 Extra; // x=Type, y=SortId, z=Shade, w=Alpha
    vec4 Color;
    vec4 Mask1;
    vec4 Mask2;
} vsOutput;

const float TypeGameObject = 0.0;
const float TypeText = 3.0;
const float TypeBar = 4.0;
const float TypeEffect = 5.0;

vec2 GetUV(vec2 uv, float flip) {
    uv.x = 0.5 + (0.5 - uv.x) * flip;
    return uv;
}

void main() {
    int verId = gl_VertexID;

    if (RenderPass == OutlineGlowPass && iExtra.x != TypeGameObject){
        gl_Position = vec4(2, 0, 0, 0); // Discard vertex
        return;
    }

    vec4 position = vec4(objPos[verId], 0, 1);
    position.xy *= iScale.xy;

    mat4 rotate = mat4(
        iRotation.y * iRotation.z, iRotation.x * iRotation.z, 0, iScale.z * iRotation.z * -iRotation.w,
        -iRotation.x * iRotation.z, iRotation.y * iRotation.z, 0, iScale.w * iRotation.z,
        0, 0, 1, 0,
        0, 0, 0, 1
    );

    position = position * rotate * BillMatrix;
    position.xyz += iPosition.xyz;
    position = position * FullMatrix;
    position.z = iExtra.y;
    gl_Position = position;

    vsOutput.BaseUV = GetUV(objUV[verId], iRotation.w);
    vsOutput.UV = iUV;
    vsOutput.Extra = iExtra;
    vsOutput.Color = iColor;
    vsOutput.Mask1 = iMask1;
    vsOutput.Mask2 = iMask2;
}
