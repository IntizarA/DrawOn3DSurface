using System;
using DrawOn3DSurface.Enums;
using UnityEngine;

namespace DrawOn3DSurface.Controllers
{
	[Serializable]
	public class BrushController : ICloneable
	{
		[SerializeField]
		private Texture brushTexture;

		[SerializeField]
		private Texture brushNormalTexture;

		[SerializeField, Range (0, 1)]
		private float brushScale = 0.1f;

		[SerializeField]
		private Color brushColor;

		[SerializeField]
		private ColorBlendType colorBlendType;

		public Texture BrushTexture
		{
			get { return brushTexture; }
			set { brushTexture = value; }
		}

		public Texture BrushNormalTexture
		{
			get { return brushNormalTexture; }
			set { brushNormalTexture = value; }
		}


		public float Size
		{
			get { return Mathf.Clamp01 (brushScale); }
			set { brushScale = Mathf.Clamp01 (value); }
		}

		public Color Color
		{
			get { return brushColor; }
			set { brushColor = value; }
		}

		public ColorBlendType ColorBlending
		{
			get { return colorBlendType; }
			set { colorBlendType = value; }
		}

		public BrushController (Texture brushTex, float size, Color color)
		{
			BrushTexture = brushTex;
			Size = size;
			Color = color;
		}
		public object Clone ()
		{
			return MemberwiseClone ();
		}
	}
}