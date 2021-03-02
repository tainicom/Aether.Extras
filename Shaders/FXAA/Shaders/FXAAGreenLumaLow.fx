
#define FXAA_PC_CONSOLE 1
#ifdef SM4 // shader model 4.0 (DX11)
#define FXAA_HLSL_4_MG 1
#else
#define FXAA_HLSL_3 1
#endif
#define FXAA_GREEN_AS_LUMA 1

#include "Fxaa3_11.MG.fxh"
#include "Macros.fxh"

DECLARE_TEXTURE(Texture, 0);

//BEGIN_CONSTANTS
float2 InverseViewportSize;
float4 ConsoleSharpness;
float4 ConsoleOpt1;
float4 ConsoleOpt2;
float SubPixelAliasingRemoval;
float EdgeThreshold;
float EdgeThresholdMin;
float ConsoleEdgeSharpness;

float ConsoleEdgeThreshold;
float ConsoleEdgeThresholdMin;

// Must keep this as constant register instead of an immediate
float4 Console360ConstDir = float4(1.0, -1.0, 0.25, -0.25);

//MATRIX_CONSTANTS
float4x4 World;
float4x4 View;
float4x4 Projection;
//END_CONSTANTS

struct VSOutput
{
	float4 position		: SV_Position;
	float2 texCoord		: TEXCOORD0;
};

VSOutput VertexShaderFunction(float4 position	: POSITION0,								
                                        float2 texCoord	: TEXCOORD0)
{
    VSOutput output;
  
    float4 worldPosition = mul(position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.position = mul(viewPosition, Projection);
	output.texCoord = texCoord;
    return output;
}

float4 PixelShaderFunction_FXAA(VSOutput input) : SV_Target0
{
	FxaaTex tex;
#ifdef SM4 // shader model 4.0 (DX11)
	tex.tex = Texture;
	tex.smpl = TextureSampler;
#else
    tex = Texture;
#endif
	
	float4 value = FxaaPixelShader(
		input.texCoord,
		0,	// Not used in PC or Xbox 360
		tex,
		tex,			// *** TODO: For Xbox, can I use additional sampler with exponent bias of -1
		tex,			// *** TODO: For Xbox, can I use additional sampler with exponent bias of -2
		InverseViewportSize,	// FXAA Quality only
		ConsoleSharpness,		// Console only
		ConsoleOpt1,
		ConsoleOpt2,
		SubPixelAliasingRemoval,	// FXAA Quality only
		EdgeThreshold,// FXAA Quality only
		EdgeThresholdMin,
		ConsoleEdgeSharpness,
		ConsoleEdgeThreshold,	// TODO
		ConsoleEdgeThresholdMin, // TODO
		Console360ConstDir
		);		
		
   return value;
}

float4 PixelShaderFunction_Standard(VSOutput input) : SV_Target0
{
	   return SAMPLE_TEXTURE(Texture, input.texCoord);
}

TECHNIQUE( Standard, VertexShaderFunction, PixelShaderFunction_Standard );

technique FXAA
{
    pass 
    {
#ifdef SM4 // shader model 4.0 (DX11)
        VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
        PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction_FXAA();
#else
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction_FXAA();
#endif
    }
}
