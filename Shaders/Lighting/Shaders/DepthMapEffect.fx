#include "Macros.fxh"

float4x4 World;
float4x4 View;
float4x4 Projection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float2 Depth : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    output.Depth.xy = input.Position.zw;

    return output;
}
struct PixelShaderOutput
{
    half4 Depth : COLOR0;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;

    output.Depth = input.Depth.x / input.Depth.y;
    return output;
}

TECHNIQUE( Standard, VertexShaderFunction, PixelShaderFunction );
