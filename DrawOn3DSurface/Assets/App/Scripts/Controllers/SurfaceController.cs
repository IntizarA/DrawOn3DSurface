using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DrawOn3DSurface.Enums;
using DrawOn3DSurface.Events;
using DynamicBox.EventManagement;
using UnityEngine;

namespace DrawOn3DSurface.Controllers
{
	[RequireComponent (typeof (Renderer))]
	[DisallowMultipleComponent]
	public class SurfaceController : MonoBehaviour
	{
		[Serializable]
		public class PaintSet
		{
			[HideInInspector]
			[NonSerialized]
			public Material material;

			[SerializeField, Tooltip ("The property name of the main texture.")]
			public string mainTextureName = "_MainTex";

			[SerializeField, Tooltip ("Whether or not use main texture paint.")]
			public bool useMainPaint = true;

			[HideInInspector]
			[NonSerialized]
			public Texture mainTexture;

			[HideInInspector]
			[NonSerialized]
			public RenderTexture paintMainTexture;

			[HideInInspector]
			[NonSerialized]
			public int mainTexturePropertyID;

			public PaintSet ()
			{
			}

			public PaintSet (string mainTextureName, bool useMainPaint)
			{
				this.mainTextureName = mainTextureName;
				this.useMainPaint = useMainPaint;
			}

			public PaintSet (string mainTextureName, bool useMainPaint,
				Material material) : this (mainTextureName, useMainPaint)
			{
				this.material = material;
			}
		}

		private static Material paintMainMaterial = null;

		[SerializeField] private Texture cleanTexture;

		[SerializeField] private List<PaintSet> paintSet = null;

		private int paintUVPropertyID;
		private int brushTexturePropertyID;
		private int brushScalePropertyID;
		private int brushColorPropertyID;
		private bool eraseFlag = false;

		private const string COLOR_BLEND_USE_BRUSH = "INK_PAINTER_COLOR_BLEND_USE_BRUSH";

		//erase
		private const string DXT5NM_COMPRESS_USE = "DXT5NM_COMPRESS_USE";
		private const string DXT5NM_COMPRESS_UNUSE = "DXT5NM_COMPRESS_UNUSE";

		#region Unity Methods

		void OnEnable ()
		{
			EventManager.Instance.AddListener<OnFileOperationEvent> (OnFileOperationEventHandler);
			EventManager.Instance.AddListener<OnClearAllEvent> (OnClearAllEventHandler);
		}

		void OnDisable ()
		{
			EventManager.Instance.RemoveListener<OnFileOperationEvent> (OnFileOperationEventHandler);
			EventManager.Instance.RemoveListener<OnClearAllEvent> (OnClearAllEventHandler);
		}

		void Awake ()
		{
			InitPropertyID ();
			SetMaterial ();
			SetTexture ();
		}

		void Start ()
		{
			SetRenderTexture ();
		}

		void OnDestroy ()
		{
			ReleaseRenderTexture ();
		}

		#endregion

		private void ClearAll ()
		{
			GetComponent<Renderer> ().material.mainTexture = cleanTexture;
			SetMaterial ();
			SetTexture ();
			SetRenderTexture ();
		}


		private void InitPropertyID ()
		{
			foreach (var p in paintSet)
			{
				p.mainTexturePropertyID = Shader.PropertyToID (p.mainTextureName);
			}

			paintUVPropertyID = Shader.PropertyToID ("_PaintUV");
			brushTexturePropertyID = Shader.PropertyToID ("_Brush");
			brushScalePropertyID = Shader.PropertyToID ("_BrushScale");
			brushColorPropertyID = Shader.PropertyToID ("_ControlColor");
		}

		private void SetMaterial ()
		{
			if (paintMainMaterial == null)
				paintMainMaterial = new Material (Resources.Load<Material> ("Es.InkPainter.PaintMain"));
			var m = GetComponent<Renderer> ().materials;
			for (int i = 0; i < m.Length; ++i)
			{
				if (paintSet[i].material == null)
					paintSet[i].material = m[i];
			}
		}

		private void SetTexture ()
		{
			foreach (var p in paintSet)
			{
				if (p.material.HasProperty (p.mainTexturePropertyID))
					p.mainTexture = p.material.GetTexture (p.mainTexturePropertyID);
			}
		}

		private RenderTexture SetupRenderTexture (Texture baseTex, int propertyID, Material material)
		{
			var rt = new RenderTexture (baseTex.width, baseTex.height, 0, RenderTextureFormat.ARGB32,
				RenderTextureReadWrite.Linear);
			rt.filterMode = baseTex.filterMode;
			Graphics.Blit (baseTex, rt);
			material.SetTexture (propertyID, rt);
			return rt;
		}

		private void SetRenderTexture ()
		{
			foreach (var paint in paintSet)
			{
				if (paint.useMainPaint)
				{
					if (paint.mainTexture != null)
						paint.paintMainTexture =
							SetupRenderTexture (paint.mainTexture, paint.mainTexturePropertyID, paint.material);
					else
						Debug.LogWarning ("To take advantage of the main texture paint must set main texture to materials.");
				}
			}
		}

		private void ReleaseRenderTexture ()
		{
			foreach (var paint in paintSet)
			{
				if (RenderTexture.active != paint.paintMainTexture && paint.paintMainTexture != null &&
				    paint.paintMainTexture.IsCreated ())
					paint.paintMainTexture.Release ();
			}
		}

		private void SetPaintMainData (BrushController brush, Vector2 uv)
		{
			paintMainMaterial.SetVector (paintUVPropertyID, uv);
			paintMainMaterial.SetTexture (brushTexturePropertyID, brush.BrushTexture);
			paintMainMaterial.SetFloat (brushScalePropertyID, brush.Size);
			paintMainMaterial.SetVector (brushColorPropertyID, brush.Color);

			foreach (var key in paintMainMaterial.shaderKeywords)
				paintMainMaterial.DisableKeyword (key);

			if (brush.ColorBlending == ColorBlendType.UseBrush)
				paintMainMaterial.EnableKeyword (COLOR_BLEND_USE_BRUSH);
		}

		private bool PaintUVDirect (BrushController brush, Vector2 uv, Func<PaintSet, bool> materialSelector = null)
		{
			brush = brush.Clone () as BrushController;
			var set = materialSelector == null ? paintSet : paintSet.Where (materialSelector);
			foreach (var paint in set)
			{
				bool mainPaintConditions = paint.useMainPaint && brush.BrushTexture != null && paint.paintMainTexture != null &&
				                           paint.paintMainTexture.IsCreated ();
				if (eraseFlag)
					brush = GetEraser (brush, paint, uv, mainPaintConditions);


				if (mainPaintConditions)
				{
					var mainPaintTextureBuffer = RenderTexture.GetTemporary (paint.paintMainTexture.width,
						paint.paintMainTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
					SetPaintMainData (brush, uv);
					Graphics.Blit (paint.paintMainTexture, mainPaintTextureBuffer, paintMainMaterial);
					Graphics.Blit (mainPaintTextureBuffer, paint.paintMainTexture);
					RenderTexture.ReleaseTemporary (mainPaintTextureBuffer);
				}
			}

			eraseFlag = false;
			return true;
		}

		public bool Paint (BrushController brush, RaycastHit hitInfo, Func<PaintSet, bool> materialSelector = null)
		{
			if (hitInfo.collider != null)
			{
				if (hitInfo.collider is MeshCollider)
					return PaintUVDirect (brush, hitInfo.textureCoord, materialSelector);
			}

			return false;
		}

		public bool Erase (BrushController brush, RaycastHit hitInfo, Func<PaintSet, bool> materialSelector = null)
		{
			eraseFlag = true;
			return Paint (brush, hitInfo, materialSelector);
		}

		private BrushController GetEraser (BrushController brush, PaintSet paintSet, Vector2 uv, bool useMainPaint)
		{
			BrushController brushClone = brush.Clone () as BrushController;
			brushClone.Color = Color.white;
			brushClone.ColorBlending = ColorBlendType.UseBrush;

			if (useMainPaint)
			{
				var rt = RenderTexture.GetTemporary (brush.BrushTexture.width, brush.BrushTexture.height);
				GrabArea.Clip (brush.BrushTexture, brush.Size, paintSet.mainTexture, uv, GrabArea.GrabTextureWrapMode.Clamp,
					rt);
				brushClone.BrushTexture = rt;
			}

			return brushClone;
		}

		#region File Operations

		private void Save ()
		{
			Texture texture = GetComponent<MeshRenderer> ().material.mainTexture;

			RenderTexture renderTexture = texture as RenderTexture;
			Texture2D texture2D;

			if (texture == null)
				return;

			if (renderTexture == null)
				return;
			RenderTexture.active = renderTexture;
			texture2D = new Texture2D (renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
			texture2D.ReadPixels (new Rect (0, 0, renderTexture.width, renderTexture.height), 0, 0);
			texture2D.Apply ();
			RenderTexture.active = null;
			SaveTextureOnDisk (texture2D);
		}

		private void SaveTextureOnDisk (Texture2D texture2D)
		{
			byte[] bytes = texture2D.EncodeToPNG ();
			string fullPath = Path.Combine (Application.persistentDataPath, $"{gameObject.GetInstanceID ()}.png");
			File.WriteAllBytes (fullPath, bytes);

			Debug.Log ($"RenderTexture saved to {fullPath}");
		}

		private void Load ()
		{
			RenderTexture renderTexture = LoadFromFile ();

			if (renderTexture != null)
			{
				GetComponent<Renderer> ().material.mainTexture = renderTexture;
				SetMaterial ();
				SetTexture ();
				SetRenderTexture ();
			}
			else
			{
				
			}
		}

		private RenderTexture LoadFromFile ()
		{
			string filePath = Path.Combine (Application.persistentDataPath, $"{gameObject.GetInstanceID ()}.png");
			if (!File.Exists (filePath))
			{
				Debug.Log ($"File not found at {filePath}");
				return null;
			}

			byte[] fileData = File.ReadAllBytes (filePath);

			Texture2D texture = new Texture2D (2, 2, TextureFormat.RGBA32, false, true);
			if (texture.LoadImage (fileData))
			{
				Debug.Log ("Texture loaded successfully.");

				RenderTexture renderTexture = new RenderTexture (texture.width, texture.height, 0);

				Graphics.Blit (texture, renderTexture);

				return renderTexture;
			}
			else
			{
				Debug.LogError ("Failed to load texture from file.");
				return null;
			}
		}

		#endregion


		#region Event Handlers

		private void OnFileOperationEventHandler (OnFileOperationEvent eventDetails)
		{
			switch (eventDetails.OperationType)
			{
				case FileOperationType.Save:
					Save ();
					break;
				case FileOperationType.Load:
					Load ();
					break;
			}
		}

		private void OnClearAllEventHandler (OnClearAllEvent eventDetails)
		{
			ClearAll ();
		}

		#endregion
	}
}