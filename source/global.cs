using System;
using UGTS.UI;
using System.Windows;

namespace UGTS.Encoder
{
	public static class MGlobal
	{
        [STAThread]
		public static void Main()
		{
            MUI.Run(Run, "Encoder", "encoder.ico");
		}

	    private static void Run()
        {
	        var w = new SecureStringWindow {WindowStartupLocation = WindowStartupLocation.CenterScreen};
	        w.ShowDialog();
        }
	}
}
