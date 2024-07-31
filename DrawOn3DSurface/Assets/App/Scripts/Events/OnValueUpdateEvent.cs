using DynamicBox.EventManagement;

namespace DrawOn3DSurface.Events
{
	public class OnValueUpdateEvent:GameEvent
	{
		public readonly float BrushSize;

		public OnValueUpdateEvent (float brushSize)
		{
			BrushSize = brushSize;
		}
	}
}