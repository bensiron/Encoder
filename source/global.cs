using System;
using System.Windows;
using UGTS.UI;

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
