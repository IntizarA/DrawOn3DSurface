using DrawOn3DSurface.Enums;
using DynamicBox.EventManagement;

namespace DrawOn3DSurface.Events
{
	public class OnPaintToolChangeEvent:GameEvent
	{
		public readonly PaintToolType ToolType;

		public OnPaintToolChangeEvent (PaintToolType toolType)
		{
			ToolType = toolType;
		}
	}
}