#include "Macros.fxh"

BEGIN_CONSTANTS

float4 DiffuseColor;

float2 TexelSize;
float3 PlaneNormal;
float  PlaneD;

MATRIX_CONSTANTS
matrix WorldViewProjection;

matrix InvProjection;
matrix InvView;
matrix InvPlaneMatrix;

END_CONSTANTS


struct VertexShaderInput
{
	float4 Position : POSITION0;
    float3 Normal : NORMAL;
	float4 Texture : TEXCOORD;
};

struct VertexShaderOutput
{
    float4 Position   : SV_POSITION;

    float3 PlanePosA : TEXCOORD0;
    float3 PlanePosB : TEXCOORD1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position, WorldViewProjection);
    
    float4 clipPosA = float4(input.Position.xy, -0.5, 1);
    float4 viewPosA = mul(clipPosA, InvProjection);
    float sclA = (((clipPosA.x * InvProjection._14) + (clipPosA.y * InvProjection._24)) + (-0.5f * InvProjection._34)) + InvProjection._44;
    viewPosA = viewPosA / sclA;
    float4 worldPosA = mul(viewPosA, InvView);
    float4 planePosA = mul(worldPosA, InvPlaneMatrix);
    output.PlanePosA = planePosA.xyz;
    
    float4 clipPosB = float4(input.Position.xy, 0.5, 1);
    float4 viewPosB = mul(clipPosB, InvProjection);
    float sclB = (((clipPosB.x * InvProjection._14) + (clipPosB.y * InvProjection._24)) + (0.5f * InvProjection._34)) + InvProjection._44;
    viewPosB = viewPosB / sclB;
    float4 worldPosB = mul(viewPosB, InvView);
    float4 planePosB = mul(worldPosB, InvPlaneMatrix);
    output.PlanePosB = planePosB.xyz;

	return output;
}

float GridPS(float2 pos, float mag, float lgfrc)
{    
    float size = pow(10, mag);
    pos = pos / size;

    float fracx = frac(pos.x);
    float fracy = frac(pos.y);
    float ifracx = 1.0 - fracx;
    float ifracy = 1.0 - fracy;
    float fracx_ = max(fracx, ifracx);
    float fracy_ = max(fracy, ifracy);
    float frac = max(fracx_, fracy_);
    frac = (frac - 0.5) * 2;
    
    frac = max(0, ((frac - 1) * lgfrc) + 1);
    
    float specular = frac;
    return specular;
}

float3 RayCastToPlane(float3 planePosA, float3 planePosB)
{
    float3 a = planePosA;
    float3 b = planePosB;
    
    float3 ab = b - a;
    float t = (-a.z) / ab.z;

    if (t < 0)
        return float3(0, 0, 0);
    
    t = max(0, t);
    float3 q = a + ab * t;

    return q;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 q = RayCastToPlane(input.PlanePosA, input.PlanePosB);

	// PS 3.0
	float2 fw2 = fwidth((float2) q);
	if(fw2.x == 0 && fw2.y ==0)
        return float4(0, 0, 0, 0);
    float fw = min(fw2.x, fw2.y);
	
	float2 pos = q.xy;

    float logfw = log10(fw)+2;
    float logfwfrac = frac(logfw);
    float ilogfw = logfw - logfwfrac;
    
    float specular1 = GridPS(pos, ilogfw + 1, 10 + 90 * (1-logfwfrac));
    float specular2 = GridPS(pos, ilogfw, 10 + (1 - logfwfrac)) * (1 - logfwfrac);
    float specular = max(specular1, specular2);
    
    return DiffuseColor * specular;
}


TECHNIQUE_9_3(GridTechnique, MainVS, MainPS);

