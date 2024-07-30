using System;
using System.Collections.Generic;
using System.Linq;
using DrawOn3DSurface.Enums;
using DrawOn3DSurface.Controllers;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[DisallowMultipleComponent]
public class SurfaceController : MonoBehaviour
{
	[Serializable]
		public class PaintSet
		{
			[HideInInspector]
			[NonSerialized]
			public Material material;

			[SerializeField, Tooltip("The property name of the main texture.")]
			public string mainTextureName = "_MainTex";

			[SerializeField, Tooltip("Normal map texture property name.")]
			public string normalTextureName = "_BumpMap";

			[SerializeField, Tooltip("Whether or not use main texture paint.")]
			public bool useMainPaint = true;

			[SerializeField, Tooltip("Whether or not use normal map paint (you need material on normal maps).")]
			public bool useNormalPaint = false;

			[HideInInspector]
			[NonSerialized]
			public Texture mainTexture;

			[HideInInspector]
			[NonSerialized]
			public RenderTexture paintMainTexture;

			[HideInInspector]
			[NonSerialized]
			public Texture normalTexture;

			[HideInInspector]
			[NonSerialized]
			public RenderTexture paintNormalTexture;

			[HideInInspector]
			[NonSerialized]
			public int mainTexturePropertyID;

			[HideInInspector]
			[NonSerialized]
			public int normalTexturePropertyID;

			public PaintSet() { }
			public PaintSet(string mainTextureName, string normalTextureName, bool useMainPaint, bool useNormalPaint)
			{
				this.mainTextureName = mainTextureName;
				this.normalTextureName = normalTextureName;
				this.useMainPaint = useMainPaint;
				this.useNormalPaint = useNormalPaint;
			}
			
			public PaintSet(string mainTextureName, string normalTextureName , bool useMainPaint, bool useNormalPaint, 
				Material material) :this(mainTextureName, normalTextureName, useMainPaint, useNormalPaint)
			{
				this.material = material;
			}
		}

		private static Material paintMainMaterial = null;
		private static Material paintNormalMaterial = null;

		[SerializeField] private List<PaintSet> paintSet = null;

		private int paintUVPropertyID;
		private int brushTexturePropertyID;
		private int brushScalePropertyID;
		private int brushColorPropertyID;
		private int brushNormalTexturePropertyID;

		private const string COLOR_BLEND_USE_BRUSH = "INK_PAINTER_COLOR_BLEND_USE_BRUSH";

		#region Unity Methods

		void Awake()
		{
			InitPropertyID();
			SetMaterial();
			SetTexture();
		}

		void Start()
		{
			SetRenderTexture();
		}

		void OnDestroy()
		{
			ReleaseRenderTexture();
		}

		#endregion 
		private void InitPropertyID()
		{
			foreach(var p in paintSet)
			{
				p.mainTexturePropertyID = Shader.PropertyToID(p.mainTextureName);
				p.normalTexturePropertyID = Shader.PropertyToID(p.normalTextureName);
			}
			paintUVPropertyID = Shader.PropertyToID("_PaintUV");
			brushTexturePropertyID = Shader.PropertyToID("_Brush");
			brushScalePropertyID = Shader.PropertyToID("_BrushScale");
			brushColorPropertyID = Shader.PropertyToID("_ControlColor");
			brushNormalTexturePropertyID = Shader.PropertyToID("_BrushNormal");
		}

		private void SetMaterial()
		{
			if(paintMainMaterial == null)
				paintMainMaterial = new Material(Resources.Load<Material>("Es.InkPainter.PaintMain"));
			if(paintNormalMaterial == null)
				paintNormalMaterial = new Material(Resources.Load<Material>("Es.InkPainter.PaintNormal"));
			var m = GetComponent<Renderer>().materials;
			for(int i = 0; i < m.Length; ++i)
			{
                if (paintSet[i].material == null)
                    paintSet[i].material = m[i];
			}
		}

		private void SetTexture()
		{
			foreach(var p in paintSet)
			{
				if(p.material.HasProperty(p.mainTexturePropertyID))
					p.mainTexture = p.material.GetTexture(p.mainTexturePropertyID);
				if(p.material.HasProperty(p.normalTexturePropertyID))
					p.normalTexture = p.material.GetTexture(p.normalTexturePropertyID);
			}
		}

		private RenderTexture SetupRenderTexture(Texture baseTex, int propertyID, Material material)
		{
			var rt = new RenderTexture(baseTex.width, baseTex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			rt.filterMode = baseTex.filterMode;
			Graphics.Blit(baseTex, rt);
			material.SetTexture(propertyID, rt);
			return rt;
		}

		private void SetRenderTexture()
		{
			foreach(var paint in paintSet)
			{
				if(paint.useMainPaint)
				{
					if(paint.mainTexture != null)
						paint.paintMainTexture = SetupRenderTexture(paint.mainTexture, paint.mainTexturePropertyID, paint.material);
					else
						Debug.LogWarning("To take advantage of the main texture paint must set main texture to materials.");
				}
				if(paint.useNormalPaint)
				{
					if(paint.normalTexture != null)
						paint.paintNormalTexture = SetupRenderTexture(paint.normalTexture, paint.normalTexturePropertyID, paint.material);
					else
						Debug.LogWarning("To take advantage of the normal map paint must set normal map to materials.");
				}
			}
		}

		private void ReleaseRenderTexture()
		{
			foreach(var paint in paintSet)
			{
				if(RenderTexture.active != paint.paintMainTexture && paint.paintMainTexture != null && paint.paintMainTexture.IsCreated())
					paint.paintMainTexture.Release();
				if(RenderTexture.active != paint.paintNormalTexture && paint.paintNormalTexture != null && paint.paintNormalTexture.IsCreated())
					paint.paintNormalTexture.Release();
			}
		}

		private void SetPaintMainData(BrushController brush, Vector2 uv)
		{
			paintMainMaterial.SetVector(paintUVPropertyID, uv);
			paintMainMaterial.SetTexture(brushTexturePropertyID, brush.BrushTexture);
			paintMainMaterial.SetFloat(brushScalePropertyID, brush.Scale);
			paintMainMaterial.SetVector(brushColorPropertyID, brush.Color);

			foreach(var key in paintMainMaterial.shaderKeywords)
				paintMainMaterial.DisableKeyword(key);
			
			if(brush.ColorBlending==ColorBlendType.UseBrush)
				paintMainMaterial.EnableKeyword(COLOR_BLEND_USE_BRUSH);
		}

		private bool PaintUVDirect(BrushController brush, Vector2 uv, Func<PaintSet, bool> materialSelector = null)
		{
			brush = brush.Clone() as BrushController;
			var set = materialSelector == null ? paintSet : paintSet.Where(materialSelector);
			foreach(var paint in set)
			{
				bool mainPaintConditions = paint.useMainPaint && brush.BrushTexture != null && paint.paintMainTexture != null && paint.paintMainTexture.IsCreated();
				bool normalPaintConditions = paint.useNormalPaint && brush.BrushNormalTexture != null && paint.paintNormalTexture != null && paint.paintNormalTexture.IsCreated();

				if(mainPaintConditions)
				{
					var mainPaintTextureBuffer = RenderTexture.GetTemporary(paint.paintMainTexture.width, paint.paintMainTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
					SetPaintMainData(brush, uv);
					Graphics.Blit(paint.paintMainTexture, mainPaintTextureBuffer, paintMainMaterial);
					Graphics.Blit(mainPaintTextureBuffer, paint.paintMainTexture);
					RenderTexture.ReleaseTemporary(mainPaintTextureBuffer);
				}

				if(normalPaintConditions)
				{
					var normalPaintTextureBuffer = RenderTexture.GetTemporary(paint.paintNormalTexture.width, paint.paintNormalTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
					Graphics.Blit(paint.paintNormalTexture, normalPaintTextureBuffer, paintNormalMaterial);
					Graphics.Blit(normalPaintTextureBuffer, paint.paintNormalTexture);
					RenderTexture.ReleaseTemporary(normalPaintTextureBuffer);
				}
			}
			return true;
		}
		
		public bool Paint(BrushController brush, RaycastHit hitInfo, Func<PaintSet, bool> materialSelector = null)
		{
			if(hitInfo.collider != null)
			{
				if(hitInfo.collider is MeshCollider)
					return PaintUVDirect(brush, hitInfo.textureCoord, materialSelector);
			}
			return false;
		}

}
