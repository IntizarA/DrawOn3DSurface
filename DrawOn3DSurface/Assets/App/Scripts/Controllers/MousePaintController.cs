using UnityEngine;

namespace DrawOn3DSurface.Controllers
{
	public class MousePaintController : MonoBehaviour
	{
		public BrushController brush;
		public bool isErasing;

		void Update ()
		{
			if (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl))
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
	}
}