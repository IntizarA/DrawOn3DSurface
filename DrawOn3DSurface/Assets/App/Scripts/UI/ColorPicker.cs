using DynamicBox.EventManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DrawOn3DSurface {

	public class ColorPicker : MonoBehaviour
	{
		[SerializeField] Texture2D colorChart;
		[SerializeField] RectTransform chart;

		[SerializeField] RectTransform cursor;
		[SerializeField] Image button;
		[SerializeField] Image cursorColor;

		void Start ()
		{
			SimulatePickColor (cursor.position);
		}
		
		private void SimulatePickColor(Vector2 position)
		{
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
			{
				position = position
			};

			PickColor(pointerEventData);
		}

		public void PickColor(BaseEventData data)
		{
			PointerEventData pointer = data as PointerEventData;
			cursor.position = pointer.position;

			Color pickedColor = colorChart.GetPixel((int)(cursor.localPosition.x * (colorChart.width / transform.GetChild(0).GetComponent<RectTransform>().rect.width)), (int)(cursor.localPosition.y * (colorChart.height / transform.GetChild(0).GetComponent<RectTransform>().rect.height)));
			button.color = pickedColor;
			cursorColor.color = pickedColor;
			EventManager.Instance.Raise (new OnChangeColorEvent (pickedColor));
		}
	}
}