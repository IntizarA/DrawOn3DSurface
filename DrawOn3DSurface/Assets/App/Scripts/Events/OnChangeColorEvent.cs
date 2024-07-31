using DynamicBox.EventManagement;
using UnityEngine;

public class OnChangeColorEvent : GameEvent
{
	public readonly Color Color;

	public OnChangeColorEvent (Color color)
	{
		Color = color;
	}
}
