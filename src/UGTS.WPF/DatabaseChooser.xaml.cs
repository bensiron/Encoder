using System.Windows;

namespace UGTS.WPF
{

    public partial class DatabaseChooserWindow : UgtsWindow
	{

		public DatabaseChooserWindow() : base()
		{
			Loaded += HLoaded;
			InitializeComponent();
		}

		public string ServerName {
			get { return serverText.Text; }
			set { serverText.Text = value; }
		}

		public string DatabaseName {
			get { return databaseText.Text; }
			set { databaseText.Text = value; }
		}

		private void HButton(System.Object sender, RoutedEventArgs e)
		{
			DialogResult = okButton == sender;
			Hide();
		}

		private void HLoaded(object sender, System.EventArgs e)
		{
			serverText.Focus();
		}
	}
}
