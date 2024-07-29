using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePainter : MonoBehaviour
{
    public Brush brush;
    void Update()
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

                paintObject.Paint (brush,hitInfo);
            }
        }
    }
}
