//-----------------------------------------------------------------------------
// BasicEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "Macros.fxh"


DECLARE_TEXTURE(Texture, 0);


BEGIN_CONSTANTS

    float4 DiffuseColor             _vs(c0)  _ps(c1)  _cb(c0);
    float3 EmissiveColor            _vs(c1)  _ps(c2)  _cb(c1);
    float3 SpecularColor            _vs(c2)  _ps(c3)  _cb(c2);
    float  SpecularPower            _vs(c3)  _ps(c4)  _cb(c2.w);

    float3 DirLight0Direction       _vs(c4)  _ps(c5)  _cb(c3);
    float3 DirLight0DiffuseColor    _vs(c5)  _ps(c6)  _cb(c4);
    float3 DirLight0SpecularColor   _vs(c6)  _ps(c7)  _cb(c5);

    float3 DirLight1Direction       _vs(c7)  _ps(c8)  _cb(c6);
    float3 DirLight1DiffuseColor    _vs(c8)  _ps(c9)  _cb(c7);
    float3 DirLight1SpecularColor   _vs(c9)  _ps(c10) _cb(c8);

    float3 DirLight2Direction       _vs(c10) _ps(c11) _cb(c9);
    float3 DirLight2DiffuseColor    _vs(c11) _ps(c12) _cb(c10);
    float3 DirLight2SpecularColor   _vs(c12) _ps(c13) _cb(c11);

    float3 EyePosition              _vs(c13) _ps(c14) _cb(c12);

    float3 FogColor                          _ps(c0)  _cb(c13);
    float4 FogVector                _vs(c14)          _cb(c14);

    float4x4 World                  _vs(c19)          _cb(c15);
    float3x3 WorldInverseTranspose  _vs(c23)          _cb(c19);

MATRIX_CONSTANTS

    float4x4 WorldViewProj          _vs(c15)          _cb(c0);

END_CONSTANTS


#include "Structures.fxh"
#include "Common.fxh"
#include "Lighting.fxh"


// Vertex shader: basic.
VSOutput VSBasic(VSInput vin)
{
    VSOutput vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    return vout;
}


// Vertex shader: basic + fog.
VSOutputFog VSBasicFog(VSInput vin)
{
    VSOutputFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsFog;
    
    return vout;
}


// Vertex shader: vertex color.
VSOutput VSBasicVc(VSInputVc vin)
{
    VSOutput vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: vertex color + fog.
VSOutputFog VSBasicVcFog(VSInputVc vin)
{
    VSOutputFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsFog;
    
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: texture.
VSOutputTx VSBasicTx(VSInputTx vin)
{
    VSOutputTx vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: texture + fog.
VSOutputTxFog VSBasicTxFog(VSInputTx vin)
{
    VSOutputTxFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: texture + vertex color.
VSOutputTx VSBasicTxVc(VSInputTxVc vin)
{
    VSOutputTx vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord = vin.TexCoord;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: texture + vertex color + fog.
VSOutputTxFog VSBasicTxVcFog(VSInputTxVc vin)
{
    VSOutputTxFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: vertex lighting + fog.
VSOutputFog VSBasicVertexLightingFog(VSInputNm vin)
{
    VSOutputFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 3);
    SetCommonVSOutputParamsFog;
    
    return vout;
}


// Vertex shader: vertex lighting + vertex color + fog.
VSOutputFog VSBasicVertexLightingVcFog(VSInputNmVc vin)
{
    VSOutputFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 3);
    SetCommonVSOutputParamsFog;
    
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: vertex lighting + texture + fog.
VSOutputTxFog VSBasicVertexLightingTxFog(VSInputNmTx vin)
{
    VSOutputTxFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 3);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: vertex lighting + texture + vertex color + fog.
VSOutputTxFog VSBasicVertexLightingTxVcFog(VSInputNmTxVc vin)
{
    VSOutputTxFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 3);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: one light + fog.
VSOutputFog VSBasicOneLightFog(VSInputNm vin)
{
    VSOutputFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 1);
    SetCommonVSOutputParamsFog;
    
    return vout;
}


// Vertex shader: one light + vertex color + fog.
VSOutputFog VSBasicOneLightVcFog(VSInputNmVc vin)
{
    VSOutputFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 1);
    SetCommonVSOutputParamsFog;
    
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: one light + texture + fog.
VSOutputTxFog VSBasicOneLightTxFog(VSInputNmTx vin)
{
    VSOutputTxFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 1);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: one light + texture + vertex color + fog.
VSOutputTxFog VSBasicOneLightTxVcFog(VSInputNmTxVc vin)
{
    VSOutputTxFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 1);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: pixel lighting + fog.
VSOutputPixelLighting VSBasicPixelLightingFog(VSInputNm vin)
{
    VSOutputPixelLighting vout;
    
    CommonVSOutputPixelLighting cout = ComputeCommonVSOutputPixelLighting(vin.Position, vin.Normal);
    SetCommonVSOutputParamsPixelLighting;

    vout.Diffuse = float4(1, 1, 1, DiffuseColor.a);
    
    return vout;
}


// Vertex shader: pixel lighting + vertex color + fog.
VSOutputPixelLighting VSBasicPixelLightingVcFog(VSInputNmVc vin)
{
    VSOutputPixelLighting vout;
    
    CommonVSOutputPixelLighting cout = ComputeCommonVSOutputPixelLighting(vin.Position, vin.Normal);
    SetCommonVSOutputParamsPixelLighting;
    
    vout.Diffuse.rgb = vin.Color.rgb;
    vout.Diffuse.a = vin.Color.a * DiffuseColor.a;
    
    return vout;
}


// Vertex shader: pixel lighting + texture + fog.
VSOutputPixelLightingTx VSBasicPixelLightingTxFog(VSInputNmTx vin)
{
    VSOutputPixelLightingTx vout;
    
    CommonVSOutputPixelLighting cout = ComputeCommonVSOutputPixelLighting(vin.Position, vin.Normal);
    SetCommonVSOutputParamsPixelLighting;
    
    vout.Diffuse = float4(1, 1, 1, DiffuseColor.a);
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: pixel lighting + texture + vertex color + fog.
VSOutputPixelLightingTx VSBasicPixelLightingTxVcFog(VSInputNmTxVc vin)
{
    VSOutputPixelLightingTx vout;
    
    CommonVSOutputPixelLighting cout = ComputeCommonVSOutputPixelLighting(vin.Position, vin.Normal);
    SetCommonVSOutputParamsPixelLighting;
    
    vout.Diffuse.rgb = vin.Color.rgb;
    vout.Diffuse.a = vin.Color.a * DiffuseColor.a;
    vout.TexCoord = vin.TexCoord;
    
    return vout;
}


// Pixel shader: basic.
float4 PSBasic(VSOutput pin) : SV_Target0
{
    return pin.Diffuse;
}


// Pixel shader: basic + fog.
float4 PSBasicFog(VSOutputFog pin) : SV_Target0
{
    float4 color = pin.Diffuse;
    
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: texture.
float4 PSBasicTx(VSOutputTx pin) : SV_Target0
{
    return SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
}


// Pixel shader: texture + fog.
float4 PSBasicTxFog(VSOutputTxFog pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
    
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: vertex lighting.
float4 PSBasicVertexLighting(VSOutputFog pin) : SV_Target0
{
    float4 color = pin.Diffuse;
    
    AddSpecular(color, pin.Specular.rgb);
    
    return color;
}


// Pixel shader: vertex lighting + fog.
float4 PSBasicVertexLightingFog(VSOutputFog pin) : SV_Target0
{
    float4 color = pin.Diffuse;

    AddSpecular(color, pin.Specular.rgb);
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: vertex lighting + texture.
float4 PSBasicVertexLightingTx(VSOutputTxFog pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
    
    AddSpecular(color, pin.Specular.rgb);
    
    return color;
}


// Pixel shader: vertex lighting + texture + fog.
float4 PSBasicVertexLightingTxFog(VSOutputTxFog pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
    
    AddSpecular(color, pin.Specular.rgb);
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: pixel lighting + fog.
float4 PSBasicPixelLightingFog(VSOutputPixelLighting pin) : SV_Target0
{
    float4 color = pin.Diffuse;

    float3 eyeVector = normalize(EyePosition - pin.PositionWS.xyz);
    float3 worldNormal = normalize(pin.NormalWS);
    
    ColorPair lightResult = ComputeLights(eyeVector, worldNormal, 3);

    color.rgb *= lightResult.Diffuse;
    
    AddSpecular(color, lightResult.Specular);
    ApplyFog(color, pin.PositionWS.w);
    
    return color;
}


// Pixel shader: pixel lighting + texture + fog.
float4 PSBasicPixelLightingTxFog(VSOutputPixelLightingTx pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
    
    float3 eyeVector = normalize(EyePosition - pin.PositionWS.xyz);
    float3 worldNormal = normalize(pin.NormalWS);
    
    ColorPair lightResult = ComputeLights(eyeVector, worldNormal, 3);
    
    color.rgb *= lightResult.Diffuse;

    AddSpecular(color, lightResult.Specular);
    ApplyFog(color, pin.PositionWS.w);
    
    return color;
}


// NOTE: The order of the techniques here are
// defined to match the indexing in LightingEffect.cs.

TECHNIQUE( BasicEffect,						                     VSBasic,		              PSBasic );
TECHNIQUE( BasicEffect_Fog,                                      VSBasicFog,                  PSBasicFog );
TECHNIQUE( BasicEffect_VertexColor,			                     VSBasicVc,                   PSBasic );
TECHNIQUE( BasicEffect_VertexColor_Fog,                          VSBasicVcFog,                PSBasicFog );
TECHNIQUE( BasicEffect_Texture,				                     VSBasicTx,		              PSBasicTx );
TECHNIQUE( BasicEffect_Texture_Fog,                              VSBasicTxFog,                PSBasicTxFog );
TECHNIQUE( BasicEffect_Texture_VertexColor,	                     VSBasicTxVc,	              PSBasicTx );
TECHNIQUE( BasicEffect_Texture_VertexColor_Fog,                  VSBasicTxVcFog,              PSBasicTxFog );

TECHNIQUE( BasicEffect_VertexLighting,						     VSBasicVertexLightingFog,    PSBasicVertexLighting );
TECHNIQUE( BasicEffect_VertexLighting_Fog,                       VSBasicVertexLightingFog,    PSBasicVertexLightingFog );
TECHNIQUE( BasicEffect_VertexLighting_VertexColor,			     VSBasicVertexLightingVcFog,  PSBasicVertexLighting );
TECHNIQUE( BasicEffect_VertexLighting_VertexColor_Fog,           VSBasicVertexLightingVcFog,  PSBasicVertexLightingFog );
TECHNIQUE( BasicEffect_VertexLighting_Texture,                   VSBasicVertexLightingTxFog,  PSBasicVertexLightingTx );
TECHNIQUE( BasicEffect_VertexLighting_Texture_Fog,               VSBasicVertexLightingTxFog,  PSBasicVertexLightingTxFog );
TECHNIQUE( BasicEffect_VertexLighting_Texture_VertexColor,       VSBasicVertexLightingTxVcFog,PSBasicVertexLightingTx );
TECHNIQUE( BasicEffect_VertexLighting_Texture_VertexColor_Fog,   VSBasicVertexLightingTxVcFog,PSBasicVertexLightingTxFog );

TECHNIQUE( BasicEffect_OneLight,                                 VSBasicOneLightFog,          PSBasicVertexLighting );
TECHNIQUE( BasicEffect_OneLight_Fog,                             VSBasicOneLightFog,          PSBasicVertexLightingFog );
TECHNIQUE( BasicEffect_OneLight_VertexColor,                     VSBasicOneLightVcFog,        PSBasicVertexLighting );
TECHNIQUE( BasicEffect_OneLight_VertexColor_Fog,                 VSBasicOneLightVcFog,        PSBasicVertexLightingFog );
TECHNIQUE( BasicEffect_OneLight_Texture,                         VSBasicOneLightTxFog,        PSBasicVertexLightingTx );
TECHNIQUE( BasicEffect_OneLight_Texture_Fog,                     VSBasicOneLightTxFog,        PSBasicVertexLightingTxFog );
TECHNIQUE( BasicEffect_OneLight_Texture_VertexColor,             VSBasicOneLightTxVcFog,      PSBasicVertexLightingTx );
TECHNIQUE( BasicEffect_OneLight_Texture_VertexColor_Fog,         VSBasicOneLightTxVcFog,      PSBasicVertexLightingTxFog );

TECHNIQUE( BasicEffect_PixelLighting,                            VSBasicPixelLightingFog,     PSBasicPixelLightingFog );
TECHNIQUE( BasicEffect_PixelLighting_Fog,                        VSBasicPixelLightingFog,     PSBasicPixelLightingFog );
TECHNIQUE( BasicEffect_PixelLighting_VertexColor,                VSBasicPixelLightingVcFog,   PSBasicPixelLightingFog );
TECHNIQUE( BasicEffect_PixelLighting_VertexColor_Fog,            VSBasicPixelLightingVcFog,   PSBasicPixelLightingFog );
TECHNIQUE( BasicEffect_PixelLighting_Texture,                    VSBasicPixelLightingTxFog,   PSBasicPixelLightingTxFog );
TECHNIQUE( BasicEffect_PixelLighting_Texture_Fog,                VSBasicPixelLightingTxFog,   PSBasicPixelLightingTxFog );
TECHNIQUE( BasicEffect_PixelLighting_Texture_VertexColor,        VSBasicPixelLightingTxVcFog, PSBasicPixelLightingTxFog );
TECHNIQUE( BasicEffect_PixelLighting_Texture_VertexColor_Fog,    VSBasicPixelLightingTxVcFog, PSBasicPixelLightingTxFog );
