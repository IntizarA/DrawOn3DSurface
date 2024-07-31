using DynamicBox.EventManagement;
using UnityEngine;

namespace DrawOn3DSurface.Controllers
{
	public class PaintController : MonoBehaviour
	{
		public BrushController brush;
		public bool isErasing;

		#region Unity Methods

		void OnEnable ()
		{
			EventManager.Instance.AddListener <OnChangeColorEvent>(OnChangeColorEventHandler);
		}

		void OnDisable ()
		{
			EventManager.Instance.RemoveListener <OnChangeColorEvent>(OnChangeColorEventHandler);
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
		
		#endregion
	}
}