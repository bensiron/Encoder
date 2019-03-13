using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace UGTS.WPF
{
	public class UgtsWindow : Window, INotifyPropertyChanged
	{
        public event PropertyChangedEventHandler PropertyChanged;
        
		protected UgtsWindow()
		{
			var c1 = SystemColors.ControlLightColor;
			var c2 = SystemColors.ControlColor;
			var cs = c1.XBlend(c2, 0.8);
			var ce = c1.XBlend(c2, 2.9);
			Background = new LinearGradientBrush(cs, ce, 13.0);
		}

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
	}
}
