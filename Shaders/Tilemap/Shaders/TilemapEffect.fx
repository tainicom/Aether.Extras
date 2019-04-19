#include "Macros.fxh"


#ifdef SM4
DECLARE_TEXTURE(Texture, 0);
DECLARE_TEXTURE(TextureAtlas, 1);
#else
texture Texture;
sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (Texture);
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = POINT;
	AddressU = Wrap;
	AddressV = Wrap;
};
texture TextureAtlas;
sampler TextureAtlasSampler : register(s1) = sampler_state
{
    Texture = (TextureAtlas);
	MAGFILTER = POINT; //LINEAR;
	MINFILTER = POINT; //LINEAR;
	MIPFILTER = POINT; //LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};
#endif


BEGIN_CONSTANTS

    float4 DiffuseColor     _vs(c0) _cb(c0);
    float3 FogColor         _ps(c0) _cb(c1);
    float4 FogVector        _vs(c5) _cb(c2);
    float2 MapSize;
	float2 InvAtlasSize;

MATRIX_CONSTANTS

    float4x4 WorldViewProj  _vs(c1) _cb(c0);

END_CONSTANTS


#include "Structures.fxh"
#include "Common.fxh"


// Vertex shader: basic.
VSOutputTx VSTilemapFog(VSInputTx vin)
{
    VSOutputTx vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: no fog.
VSOutputTxNoFog VSTilemap(VSInputTx vin)
{
    VSOutputTxNoFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsNoFog;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: vertex color.
VSOutputTx VSTilemapVcFog(VSInputTxVc vin)
{
    VSOutputTx vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord = vin.TexCoord;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: vertex color, no fog.
VSOutputTxNoFog VSTilemapVc(VSInputTxVc vin)
{
    VSOutputTxNoFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsNoFog;
    
    vout.TexCoord = vin.TexCoord;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Pixel shader: basic.
float4 PSTilemapFog(VSOutputTx pin) : SV_Target0
{
    float2 txCoord = pin.TexCoord * MapSize;
    float2 txCoordi = floor(txCoord);
    float2 tx2Coord = (txCoord - txCoordi);

    float4 mapColor = SAMPLE_TEXTURE(Texture, txCoordi/MapSize);
    float2 tileCoord = mapColor.xy * 255;
    float alpha = mapColor.a;
	
    tx2Coord = (tx2Coord + tileCoord) * InvAtlasSize;

    float4 color = SAMPLE_TEXTURE(TextureAtlas, tx2Coord);
    color *= alpha;
    color *= pin.Diffuse;
    
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: no fog.
float4 PSTilemap(VSOutputTxNoFog pin) : SV_Target0
{
    float2 txCoord = pin.TexCoord * MapSize;
    float2 txCoordi = floor(txCoord);
    float2 tx2Coord = (txCoord - txCoordi);

    float4 mapColor = SAMPLE_TEXTURE(Texture, txCoordi/MapSize);
	float2 tileCoord = mapColor.xy * 255;
	float alpha = mapColor.a;
	
    tx2Coord = (tx2Coord + tileCoord) * InvAtlasSize;

    float4 color = SAMPLE_TEXTURE(TextureAtlas, tx2Coord);
	color *= alpha;
    color *= pin.Diffuse;

    return color;
}


// NOTE: The order of the techniques here are
// defined to match the indexing in DualTextureEffect.cs.

TECHNIQUE( TilemapEffectFog,             VSTilemapFog,   PSTilemapFog );
TECHNIQUE( TilemapEffect,                VSTilemap,      PSTilemap );
TECHNIQUE( TilemapEffect_VertexColorFog, VSTilemapVcFog, PSTilemapFog );
TECHNIQUE( TilemapEffect_VertexColor,    VSTilemapVc,    PSTilemap );
