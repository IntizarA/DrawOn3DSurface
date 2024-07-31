using DrawOn3DSurface.Enums;
using DrawOn3DSurface.Events;
using DynamicBox.EventManagement;
using UnityEngine;

namespace DrawOn3DSurface.Controllers
{
	public class PaintController : MonoBehaviour
	{
		public BrushController brush;
		private bool isErasing;

		#region Unity Methods

		void OnEnable ()
		{
			EventManager.Instance.AddListener <OnChangeColorEvent>(OnChangeColorEventHandler);
			EventManager.Instance.AddListener <OnPaintToolChangeEvent>(OnPaintToolChangeEventHandler);
		}

		void OnDisable ()
		{
			EventManager.Instance.RemoveListener <OnChangeColorEvent>(OnChangeColorEventHandler);
			EventManager.Instance.RemoveListener <OnPaintToolChangeEvent>(OnPaintToolChangeEventHandler);
		}

		void Update ()
		{
			if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))
				return;
			if (Input.GetMouseButton (0))
			{
				var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hitInfo;
				if (Physics.Raycast (ray, out hitInfo))
				{
					var paintObject = hitInfo.transform.GetComponent<SurfaceController> ();
					if (paintObject == null)
						return;
					if (isErasing)
					{
						paintObject.Erase (brush, hitInfo);
					}
					else
					{
						paintObject.Paint (brush, hitInfo);
					}
				}
			}
		}
		#endregion

		#region Event Handlers

		private void OnChangeColorEventHandler (OnChangeColorEvent eventDetails)
		{
			brush.Color = eventDetails.Color;
		}

		private void OnPaintToolChangeEventHandler (OnPaintToolChangeEvent eventDetails)
		{
			isErasing = eventDetails.ToolType == PaintToolType.Eraser;
		}

		#endregion
	}
}