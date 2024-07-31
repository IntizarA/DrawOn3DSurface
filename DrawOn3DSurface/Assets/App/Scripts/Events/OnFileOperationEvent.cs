using DrawOn3DSurface.Enums;
using DynamicBox.EventManagement;

namespace DrawOn3DSurface.Events
{
	public class OnFileOperationEvent:GameEvent
	{
		public readonly FileOperationType OperationType;

		public OnFileOperationEvent (FileOperationType operationType)
		{
			OperationType = operationType;
		}
	}
}