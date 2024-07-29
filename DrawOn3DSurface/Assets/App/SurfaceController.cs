using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SurfaceController : MonoBehaviour
{
	private static Material paintMainMaterial = null;
	private static Material paintNormalMaterial = null;
	private static Material paintHeightMaterial = null;

	private void Awake ()
	{
		SetMaterial ();
	}

	private void SetMaterial ()
	{
		if (paintMainMaterial == null)
			paintMainMaterial = new Material (Resources.Load<Material> ("Es.InkPainter.PaintMain"));
		if (paintNormalMaterial == null)
			paintNormalMaterial = new Material (Resources.Load<Material> ("Es.InkPainter.PaintNormal"));
		if (paintHeightMaterial == null)
			paintHeightMaterial = new Material (Resources.Load<Material> ("Es.InkPainter.PaintHeight"));
		var m = GetComponent<Renderer> ().materials;
		for (int i = 0; i < m.Length; ++i)
		{
			if (paintSet[i].material == null)
				paintSet[i].material = m[i];
		}
	}

	[SerializeField]
	private List<PaintSet> paintSet = null;

	public bool Paint (Brush brush, RaycastHit hitInfo, Func<PaintSet, bool> materialSelector = null)
	{
		if (hitInfo.collider != null)
		{
			if (hitInfo.collider is MeshCollider)
				return PaintUVDirect (brush, hitInfo.textureCoord, materialSelector);
		}

		return false;
	}

	public bool PaintUVDirect (Brush brush, Vector2 uv, Func<PaintSet, bool> materialSelector = null)
	{
		var set = materialSelector == null ? paintSet : paintSet.Where (materialSelector);
		foreach (var p in set)
		{
			var mainPaintConditions = p.useMainPaint && brush.BrushTexture != null && p.paintMainTexture != null &&
			                          p.paintMainTexture.IsCreated ();
			var normalPaintConditions = p.useNormalPaint && brush.BrushNormalTexture != null &&
			                            p.paintNormalTexture != null && p.paintNormalTexture.IsCreated ();
			var heightPaintConditions = p.useHeightPaint && brush.BrushHeightTexture != null &&
			                            p.paintHeightTexture != null && p.paintHeightTexture.IsCreated ();


			if (mainPaintConditions)
			{
				var mainPaintTextureBuffer = RenderTexture.GetTemporary (p.paintMainTexture.width, p.paintMainTexture.height, 0,
					RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
				//SetPaintMainData (brush, uv);
				Graphics.Blit (p.paintMainTexture, mainPaintTextureBuffer, paintMainMaterial);
				Graphics.Blit (mainPaintTextureBuffer, p.paintMainTexture);
				RenderTexture.ReleaseTemporary (mainPaintTextureBuffer);
			}

			if (normalPaintConditions)
			{
				var normalPaintTextureBuffer = RenderTexture.GetTemporary (p.paintNormalTexture.width,
					p.paintNormalTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			//	SetPaintNormalData (brush, uv, false);
				Graphics.Blit (p.paintNormalTexture, normalPaintTextureBuffer, paintNormalMaterial);
				Graphics.Blit (normalPaintTextureBuffer, p.paintNormalTexture);
				RenderTexture.ReleaseTemporary (normalPaintTextureBuffer);
			}

			if (heightPaintConditions)
			{
				var heightPaintTextureBuffer = RenderTexture.GetTemporary (p.paintHeightTexture.width,
					p.paintHeightTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		//		SetPaintHeightData (brush, uv);
				Graphics.Blit (p.paintHeightTexture, heightPaintTextureBuffer, paintHeightMaterial);
				Graphics.Blit (heightPaintTextureBuffer, p.paintHeightTexture);
				RenderTexture.ReleaseTemporary (heightPaintTextureBuffer);
			}
		}

		return true;
	}
}

[Serializable]
	public class PaintSet
	{
		/// <summary>
		/// Applying paint materials.
		/// </summary>
		[HideInInspector]
		[NonSerialized]
		public Material material;

		[SerializeField, Tooltip ("The property name of the main texture.")]
		public string mainTextureName = "_MainTex";

		[SerializeField, Tooltip ("Normal map texture property name.")]
		public string normalTextureName = "_BumpMap";

		[SerializeField, Tooltip ("The property name of the heightmap texture.")]
		public string heightTextureName = "_ParallaxMap";

		[SerializeField, Tooltip ("Whether or not use main texture paint.")]
		public bool useMainPaint = true;

		[SerializeField, Tooltip ("Whether or not use normal map paint (you need material on normal maps).")]
		public bool useNormalPaint = false;

		[SerializeField, Tooltip ("Whether or not use heightmap painting (you need material on the heightmap).")]
		public bool useHeightPaint = false;

		/// <summary>
		/// In the first time set to the material's main texture.
		/// </summary>
		[HideInInspector]
		[NonSerialized]
		public Texture mainTexture;

		/// <summary>
		/// Copied the main texture to rendertexture that use to paint.
		/// </summary>
		[HideInInspector]
		[NonSerialized]
		public RenderTexture paintMainTexture;

		/// <summary>
		/// In the first time set to the material's normal map.
		/// </summary>
		[HideInInspector]
		[NonSerialized]
		public Texture normalTexture;

		/// <summary>
		/// Copied the normal map to rendertexture that use to paint.
		/// </summary>
		[HideInInspector]
		[NonSerialized]
		public RenderTexture paintNormalTexture;

		/// <summary>
		/// In the first time set to the material's height map.
		/// </summary>
		[HideInInspector]
		[NonSerialized]
		public Texture heightTexture;

		/// <summary>
		/// Copied the height map to rendertexture that use to paint.
		/// </summary>
		[HideInInspector]
		[NonSerialized]
		public RenderTexture paintHeightTexture;

		#region ShaderPropertyID

		[HideInInspector]
		[NonSerialized]
		public int mainTexturePropertyID;

		[HideInInspector]
		[NonSerialized]
		public int normalTexturePropertyID;

		[HideInInspector]
		[NonSerialized]
		public int heightTexturePropertyID;

		#endregion ShaderPropertyID

		#region Constractor

		/// <summary>
		/// Default constractor.
		/// </summary>
		public PaintSet ()
		{
		}

		/// <summary>
		/// Setup paint data.
		/// </summary>
		/// <param name="mainTextureName">Shader property name(main texture).</param>
		/// <param name="normalTextureName">Shader property name(normal map).</param>
		/// <param name="heightTextureName">Shader property name(height map)</param>
		/// <param name="useMainPaint">Whether to use main texture paint.</param>
		/// <param name="useNormalPaint">Whether to use normal map paint.</param>
		/// <param name="useHeightPaint">Whether to use height map paint.</param>
		public PaintSet (string mainTextureName, string normalTextureName, string heightTextureName, bool useMainPaint,
			bool useNormalPaint, bool useHeightPaint)
		{
			this.mainTextureName = mainTextureName;
			this.normalTextureName = normalTextureName;
			this.heightTextureName = heightTextureName;
			this.useMainPaint = useMainPaint;
			this.useNormalPaint = useNormalPaint;
			this.useHeightPaint = useHeightPaint;
		}

		/// <summary>
		/// Setup paint data.
		/// </summary>
		/// <param name="mainTextureName">Shader property name(main texture).</param>
		/// <param name="normalTextureName">Shader property name(normal map).</param>
		/// <param name="heightTextureName">Shader property name(height map)</param>
		/// <param name="useMainPaint">Whether to use main texture paint.</param>
		/// <param name="useNormalPaint">Whether to use normal map paint.</param>
		/// <param name="useHeightPaint">Whether to use height map paint.</param>
		/// <param name="material">Specify when painting a specific material.</param>
		public PaintSet (string mainTextureName, string normalTextureName, string heightTextureName, bool useMainPaint,
			bool useNormalPaint, bool useHeightPaint, Material material)
			: this (mainTextureName, normalTextureName, heightTextureName, useMainPaint, useNormalPaint, useHeightPaint)
		{
			this.material = material;
		}

		#endregion Constractor
	}