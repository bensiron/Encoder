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
		    var darkColor = new Color {R = 230, G = 230, B = 240, A = 255};
            var lightColor = new Color {R = 247, G = 247, B = 240, A = 255};
            Background = new LinearGradientBrush(lightColor, darkColor, 13.0);
		}

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
	}
}
