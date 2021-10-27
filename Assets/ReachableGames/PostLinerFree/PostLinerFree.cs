//-------------------
// Copyright 2019
// Reachable Games, LLC
//-------------------

using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace ReachableGames
{
	namespace PostLinerFree
	{
		[Serializable]
		[PostProcess(typeof(PostLinerEffect), PostProcessEvent.BeforeTransparent, "ReachableGames/Post Liner Free", true)]
		public sealed class PostLinerFree : PostProcessEffectSettings
		{
			[Tooltip("RGB controls the color of the outline.")]
			public ColorParameter outlineColor = new ColorParameter { value = Color.yellow };
			[Range(0.0f, 1.0f), Tooltip("Master knob for the maximum blend amount.")]
			public FloatParameter finalBlend = new FloatParameter { value = 1.0f };
		}

		public sealed class PostLinerEffect : PostProcessEffectRenderer<PostLinerFree>
		{
			private static int _globalTextureId = Shader.PropertyToID("_OutlineDepth");
			private static int _pixelOffsetId = Shader.PropertyToID("_PixelOffset");
			private static int _outlineColorId = Shader.PropertyToID("_OutlineColor");
			private static int _finalBlendId = Shader.PropertyToID("_FinalBlend");

			public override DepthTextureMode GetCameraFlags()
			{
				return base.GetCameraFlags() | DepthTextureMode.DepthNormals;  // make sure we get depth and normals
			}

			public override void Render(PostProcessRenderContext context)
			{
				Texture outlineTexture = Shader.GetGlobalTexture(_globalTextureId);
				if (outlineTexture!=null)
				{
					// Configure the outline post effect
					var sheet = context.propertySheets.Get(Shader.Find("Hidden/ReachableGames/PostLinerFree"));
					sheet.properties.SetVector(_pixelOffsetId, new Vector4(1.0f/outlineTexture.width, 1.0f/outlineTexture.height, 0,0));
					sheet.properties.SetColor(_outlineColorId, settings.outlineColor);
					sheet.properties.SetFloat(_finalBlendId, settings.finalBlend);

					context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
				}
				else
				{
					context.command.CopyTexture(context.source, context.destination);
				}
			}
		}
	}
}