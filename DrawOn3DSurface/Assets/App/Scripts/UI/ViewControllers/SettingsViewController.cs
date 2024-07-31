using DrawOn3DSurface.Enums;
using DrawOn3DSurface.Events;
using DrawOn3DSurface.UI.Views;
using DynamicBox.EventManagement;
using UnityEngine;

namespace DrawOn3DSurface.UI.ViewControllers
{
	public class SettingsViewController : MonoBehaviour
	{
		[SerializeField] private SettingsView view;

		#region Unity Methods

		void OnEnable ()
		{
			EventManager.Instance.AddListener<OnValueUpdateEvent> (OnValueUpdateEventHandler);
		}

		void OnDisable ()
		{
			EventManager.Instance.RemoveListener<OnValueUpdateEvent> (OnValueUpdateEventHandler);
		}

		#endregion
		
		public void OnSettingsButtonClicked ()
		{
			view.gameObject.SetActive (true);
		}

		public void OnClearAllClicked ()
		{
			EventManager.Instance.Raise (new OnClearAllEvent ());
		}

		public void OnSaveToFileClicked ()
		{
			EventManager.Instance.Raise (new OnFileOperationEvent (FileOperationType.Save));
		}

		public void OnLoadFromFileClicked ()
		{
			EventManager.Instance.Raise (new OnFileOperationEvent (FileOperationType.Load));
		}

		public void OnCloseButtonClicked ()
		{
			view.gameObject.SetActive (false);
		}

		public void OnPaintToolChange (bool isEraser)
		{
			EventManager.Instance.Raise (new OnPaintToolChangeEvent (isEraser ? PaintToolType.Eraser : PaintToolType.Brush));
		}


		#region Event Handlers
		
		private void OnValueUpdateEventHandler (OnValueUpdateEvent eventDetails)
		{
			view.Slider.SliderValue = eventDetails.BrushSize;
		}

		#endregion
	}
}