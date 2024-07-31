using DrawOn3DSurface.Events;
using DynamicBox.EventManagement;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DrawOn3DSurface.UI
{
	public class SliderController : MonoBehaviour,IPointerUpHandler
	{
		[SerializeField] private Slider slider;
		[SerializeField] private TMP_Text sliderValueText;

		public float SliderValue
		{
			get
			{
				return slider.value;
			}
			set
			{
				slider.value = value;
			}
		}
		public void OnValueChange (float value)
		{
			int formatValue=FormatValue (value);
			sliderValueText.text = $"{formatValue}";
		}
		
		public void OnPointerUp (PointerEventData eventData)
		{
			float value=slider.value;
			
			EventManager.Instance.Raise (new OnValueUpdateEvent (value));
		}

		private int FormatValue (float value)
		{
			int result =(int)(value * 100);
			return result;
		}
		
	}
}