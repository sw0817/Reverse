//-------------------
// Copyright 2019
// Reachable Games, LLC
//-------------------

Shader "Hidden/ReachableGames/PostLinerFree"
{
	HLSLINCLUDE
	#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);  // rendered scene image with all objects in it
	TEXTURE2D_SAMPLER2D(_OutlineDepth, sampler_OutlineDepth);  // this is a shader global texture
	TEXTURE2D_SAMPLER2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture);  // this is a shader global texture too
	TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);  // this is a shader global texture too
	float2 _PixelOffset;  // this is a texture sampler property, not user editable

	// Separate color for outline and fill
	float4 _OutlineColor;

	// Final blend is knob that controls maximum amount of outline to present to the player.
	float _FinalBlend;

	inline void DecodeDepthNormal(float4 enc, out float depth, out float3 normal)
	{
		// DecodeFloatRG
		const float2 kDecodeDot = float2(1.0, 1 / 255.0);
		depth = dot(enc.zw, kDecodeDot);

		// DecodeViewNormalStereo
		const float kScale = 1.7777;
		float3 nn = enc.xyz*float3(2 * kScale, 2 * kScale, 0) + float3(-kScale, -kScale, 1);
		float g = 2.0 / dot(nn.xyz, nn.xyz);
		normal.xy = g * nn.xy;
		normal.z = g - 1;
	}

	// Depth+Normals in one texture means the depth values are stored in linear space, and are really low quality.
	// Normals are also stored with only two components and are also really low quality.  Not sure there's a better
	// option though.  Getting exterior edges is easy, but figuring out the rendered pixels in the frame buffer were
	// not from our object is hard, because the depth values are so bad.
	float4 Frag(VaryingsDefault i) : SV_Target
	{
		// This is the outline objects depth ONLY, which we can use to tell if this object's pixel was 
		// rendered closest to the camera or not based on depth versus the sceneDepth above.
		float objectDepth = SAMPLE_DEPTH_TEXTURE(_OutlineDepth, sampler_OutlineDepth, i.texcoord);  // 1 means very close to near plane, 0 means close to far plane

		// This is the source color in the frame buffer
		float4 origColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
		if (objectDepth > 0.0 && objectDepth < 1.0)  // skip far plane pixels, they are never outlined
		{
			// Figure out the fading factors, so we can hold the effect full on until a point is reached, then a controlled fade beyond that
			float linearObjDepth = Linear01Depth(objectDepth);
			float topologyDistanceBlend = 1 - smoothstep(0.02, 0.02+0.01, linearObjDepth);
			float hardEdgelDistanceBlend = 1 - smoothstep(0.02, 0.02+0.01, linearObjDepth);
			float fillDistanceBlend = 1 - smoothstep(0.083, 0.083+0.01, linearObjDepth);

			// sample the four adjacencies and decide if we should put an outline pixel in the center or not.
			float2 deltaY = float2(0, _PixelOffset.y * 0.78);
			float2 deltaX = float2(_PixelOffset.x * 0.78, 0);

			// Camera depth with ALL objects rendered into it.  Depth comes back as linear values 0..1, so we convert to real world distances
			float sceneDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord);
			if (abs(LinearEyeDepth(objectDepth) - LinearEyeDepth(sceneDepth)) < 0.03 )  // use visible settings
			{
				float centerDepth;
				float3 centerNormal;
				DecodeDepthNormal(SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, i.texcoord), centerDepth, centerNormal);

				// Use normals to figure out if the center is significantly different from the edges.
				float upDepth;
				float3 upNormal;
				DecodeDepthNormal(SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, i.texcoord - deltaY), upDepth, upNormal);

				float downDepth;
				float3 downNormal;
				DecodeDepthNormal(SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, i.texcoord + deltaY), downDepth, downNormal);

				float leftDepth;
				float3 leftNormal;
				DecodeDepthNormal(SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, i.texcoord - deltaX), leftDepth, leftNormal);

				float rightDepth;
				float3 rightNormal;
				DecodeDepthNormal(SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, i.texcoord + deltaX), rightDepth, rightNormal);

				// when real objectDepth is nearly equal to sceneDepth, the outline object is visible.  Otherwise something is obscuring it, so it's hidden.
				float depthBlend =  topologyDistanceBlend * 0.501 * smoothstep(0, 0.00027, upDepth + downDepth + leftDepth + rightDepth - 4 * centerDepth);
				float normalBlend = hardEdgelDistanceBlend * 0.327 * (2.0 - dot(upNormal, downNormal) - dot(leftNormal, rightNormal));

				float fillBlend = _FinalBlend * fillDistanceBlend * 0.122;
				float outlineBlend = _FinalBlend * saturate(depthBlend + normalBlend);

				// Draw the original color, lerped somewhat to the outline color based on whether the outline is present
				float4 fillColor = float4(lerp(origColor.rgb, float3(1,1,1), fillBlend.xxx), origColor.a);
				float4 finalColor = float4(lerp(fillColor.rgb, _OutlineColor.rgb, outlineBlend.xxx), origColor.a);
				return finalColor;
			}
			else  // use hidden settings
			{
				return origColor;
			}
		}
		return origColor;
	}
	ENDHLSL

	SubShader
	{
		Cull Off 
		ZWrite Off 
		ZTest Always
		ColorMask RGB
		Pass
		{
			HLSLPROGRAM

#pragma vertex VertDefault
#pragma fragment Frag

			ENDHLSL
		}
	}
}
