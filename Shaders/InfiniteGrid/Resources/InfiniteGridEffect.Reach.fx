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
    
    float3 PlanePosA  : TEXCOORD0;
    float3 PlanePosB  : TEXCOORD1;

    float3 PlanePosA2 : TEXCOORD2;
    float3 PlanePosB2 : TEXCOORD3;
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

    float4 clipPosA2 = float4(input.Position.xy + TexelSize, -0.5, 1);
    float4 viewPosA2 = mul(clipPosA2, InvProjection);
    float sclA2 = (((clipPosA2.x * InvProjection._14) + (clipPosA2.y * InvProjection._24)) + (-0.5f * InvProjection._34)) + InvProjection._44;
    viewPosA2 = viewPosA2 / sclA2;
    float4 worldPosA2 = mul(viewPosA2, InvView);
    float4 planePosA2 = mul(worldPosA2, InvPlaneMatrix);
    output.PlanePosA2 = planePosA2.xyz;

    float4 clipPosB2 = float4(input.Position.xy + TexelSize, 0.5, 1);
    float4 viewPosB2 = mul(clipPosB2, InvProjection);
    float sclB2 = (((viewPosB2.x * InvProjection._14) + (viewPosB2.y * InvProjection._24)) + (0.5f * InvProjection._34)) + InvProjection._44;
    viewPosB2 = viewPosB2 / sclB2;
    float4 worldPosB2 = mul(viewPosB2, InvView);
    float4 planePosB2 = mul(worldPosB2, InvPlaneMatrix);
    output.PlanePosB2 = planePosB2.xyz;
    
	return output;
}

float HLinesPS(float posx, float mag, float lgfrc)
{
    float size = pow(10, mag);
    posx = posx / size;

    float fracx = frac(posx);
    float ifracx = 1.0 - fracx;
    float frac = max(fracx, ifracx);
    frac = (frac - 0.5) * 2;
    
    frac = max(0, ((frac - 1) * lgfrc) + 1);
    
    float specular = frac;
    return specular;
}

float VLinesPS(float posy, float mag, float lgfrc)
{
    float size = pow(10, mag);
    posy = posy / size;

    float fracy = frac(posy);
    float ifracy = 1.0 - fracy;
    float frac = max(fracy, ifracy);
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

float4 HMainAPS(VertexShaderOutput input) : COLOR
{   
    float3 q = RayCastToPlane(input.PlanePosA, input.PlanePosB);

	// PS 2.0 
    float3 q2 = RayCastToPlane(input.PlanePosA2, input.PlanePosB2);
	float2 fw2 = (float2)(q2 - q);
	if(fw2.x == 0 && fw2.y ==0)
        return float4(0, 0, 0, 0);
    float fw = abs(fw2.x);
	
    float logfw = log10(fw) + 2;
    float logfwfrac = frac(logfw);
    float ilogfw = logfw - logfwfrac;
    
    float specular = HLinesPS(q.x, ilogfw, 10 + (1 - logfwfrac)) * (1 - logfwfrac);
    
    return DiffuseColor * specular;
}

float4 VMainAPS(VertexShaderOutput input) : COLOR
{
    float3 q = RayCastToPlane(input.PlanePosA, input.PlanePosB);

	// PS 2.0 
    float3 q2 = RayCastToPlane(input.PlanePosA2, input.PlanePosB2);
	float2 fw2 = (float2)(q2 - q);
	if(fw2.x == 0 && fw2.y ==0)
        return float4(0, 0, 0, 0);
    float fw = abs(fw2.y);

    float logfw = log10(fw) + 2;
    float logfwfrac = frac(logfw);
    float ilogfw = logfw - logfwfrac;
    
    float specular = VLinesPS(q.y, ilogfw, 10 + (1 - logfwfrac)) * (1 - logfwfrac);
    
    return DiffuseColor * specular;
}

float4 HMainBPS(VertexShaderOutput input) : COLOR
{
    float3 q = RayCastToPlane(input.PlanePosA, input.PlanePosB);

	// PS 2.0 
    float3 q2 = RayCastToPlane(input.PlanePosA2, input.PlanePosB2);
	float2 fw2 = (float2)(q2 - q);
	if(fw2.x == 0 && fw2.y ==0)
        return float4(0, 0, 0, 0);
    float fw = abs(fw2.x);
	
    float logfw = log10(fw) + 2;
    float logfwfrac = frac(logfw);
    float ilogfw = logfw - logfwfrac;
    
    float specular = HLinesPS(q.x, ilogfw+1, 10 + 90 * (1 - logfwfrac));
    
    return DiffuseColor * specular;
}

float4 VMainBPS(VertexShaderOutput input) : COLOR
{
    float3 q = RayCastToPlane(input.PlanePosA, input.PlanePosB);

	// PS 2.0 
    float3 q2 = RayCastToPlane(input.PlanePosA2, input.PlanePosB2);
    float2 fw2 = (float2) (q2 - q);
	if(fw2.x == 0 && fw2.y ==0)
        return float4(0, 0, 0, 0);
    float fw = abs(fw2.y);

    float logfw = log10(fw) + 2;
    float logfwfrac = frac(logfw);
    float ilogfw = logfw - logfwfrac;
    
    float specular = VLinesPS(q.y, ilogfw+1, 10 + 90 * (1 - logfwfrac));
    
    return DiffuseColor * specular;
}

TECHNIQUE(HorzLinesATechnique, MainVS, HMainAPS);
TECHNIQUE(VertLinesATechnique, MainVS, VMainAPS);
TECHNIQUE(HorzLinesBTechnique, MainVS, HMainBPS);
TECHNIQUE(VertLinesBTechnique, MainVS, VMainBPS);

