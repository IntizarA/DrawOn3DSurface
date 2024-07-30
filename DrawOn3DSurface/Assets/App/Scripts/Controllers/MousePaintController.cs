using UnityEngine;

namespace DrawOn3DSurface.Controllers
{
	public class MousePaintController : MonoBehaviour
	{
		public BrushController brush;

		void Update ()
		{
			if (Input.GetMouseButton (0))
			{
				var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hitInfo;
				if (Physics.Raycast (ray, out hitInfo))
				{
					var paintObject = hitInfo.transform.GetComponent<SurfaceController> ();
					if (paintObject == null)
						return;

					paintObject.Paint (brush, hitInfo);
				}
			}
		}
	}
}