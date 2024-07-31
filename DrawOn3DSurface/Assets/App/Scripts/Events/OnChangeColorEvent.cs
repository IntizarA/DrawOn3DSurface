using DynamicBox.EventManagement;
using UnityEngine;

namespace DrawOn3DSurface.Events
{
	public class OnChangeColorEvent : GameEvent
	{
		public readonly Color Color;

		public OnChangeColorEvent (Color color)
		{
			Color = color;
		}
	}
}