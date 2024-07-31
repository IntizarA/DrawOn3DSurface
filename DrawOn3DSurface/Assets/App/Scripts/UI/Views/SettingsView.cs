using UnityEngine;

namespace DrawOn3DSurface.UI.Views
{
	public class SettingsView : MonoBehaviour
	{
		[SerializeField] private SliderController slider;

		public SliderController Slider
		{
			get
			{
				return slider;
			}
			set
			{
				slider = value;
			}
		} 
		
		
	}
}