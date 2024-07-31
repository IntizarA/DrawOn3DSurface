using System.Collections;
using System.Collections.Generic;
using UnityEngine;

	public static class GrabArea
	{
		public enum GrabTextureWrapMode
		{
			Clamp,
			Repeat,
			Clip,
		}

		#region PrivateField

		private const string GRAB_AREA_MATERIAL = "Es.InkPainter.Effective.GrabArea";
		private const string CLIP = "_ClipTex";
		private const string TARGET = "_TargetTex";
		private const string CLIP_SCALE = "_ClipScale";
		private const string CLIP_UV = "_ClipUV";

		private const string WM_CLAMP = "WRAP_MODE_CLAMP";
		private const string WM_REPEAT = "WRAP_MODE_REPEAT";
		private const string WM_CLIP = "WRAP_MODE_CLIP";

		private const string ALPHA_REPLACE = "ALPHA_REPLACE";
		private const string ALPHA_NOT_REPLACE = "ALPHA_NOT_REPLACE";

		private static Material grabAreaMaterial = null;

		#endregion PrivateField

		#region PublicMethod

		public static void Clip(Texture clipTexture, float clipScale, Texture grabTargetTexture, Vector2 targetUV, GrabTextureWrapMode wrapMode, RenderTexture dst, bool replaceAlpha = true)
		{
			if(grabAreaMaterial == null)
				InitGrabAreaMaterial();
			SetGrabAreaProperty(clipTexture, clipScale, grabTargetTexture, targetUV, wrapMode, replaceAlpha);
			var tmp = RenderTexture.GetTemporary(clipTexture.width, clipTexture.height, 0);
			Graphics.Blit(clipTexture, tmp, grabAreaMaterial);
			Graphics.Blit(tmp, dst);
			RenderTexture.ReleaseTemporary(tmp);
		}

		#endregion PublicMethod

		#region PrivateMethod

		private static void InitGrabAreaMaterial()
		{
			grabAreaMaterial = new Material(Resources.Load<Material>(GRAB_AREA_MATERIAL));
		}

		private static void SetGrabAreaProperty(Texture clip, float clipScale, Texture grabTarget, Vector2 targetUV, GrabTextureWrapMode wrapMpde, bool replaceAlpha)
		{
			grabAreaMaterial.SetTexture(CLIP, clip);
			grabAreaMaterial.SetTexture(TARGET, grabTarget);
			grabAreaMaterial.SetFloat(CLIP_SCALE, clipScale);
			grabAreaMaterial.SetVector(CLIP_UV, targetUV);

			foreach(var key in grabAreaMaterial.shaderKeywords)
				grabAreaMaterial.DisableKeyword(key);
			switch(wrapMpde)
			{
				case GrabTextureWrapMode.Clamp:
					grabAreaMaterial.EnableKeyword(WM_CLAMP);
					break;

				case GrabTextureWrapMode.Repeat:
					grabAreaMaterial.EnableKeyword(WM_REPEAT);
					break;

				case GrabTextureWrapMode.Clip:
					grabAreaMaterial.EnableKeyword(WM_CLIP);
					break;

				default:
					break;
			}

			switch(replaceAlpha)
			{
				case true:
					grabAreaMaterial.EnableKeyword(ALPHA_REPLACE);
					break;
				case false:
					grabAreaMaterial.EnableKeyword(ALPHA_NOT_REPLACE);
					break;
			}
		}

		#endregion PrivateMethod
	}
