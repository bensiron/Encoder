using System;
using System.Windows;
using UGTS.Encoder.WPF;

namespace UGTS.Encoder
{
    public static class ProgramEntry
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                IconManager.IconPath = "encoder.ico";
                var w = new SecureStringWindow { WindowStartupLocation = WindowStartupLocation.CenterScreen };
                w.ShowDialog();
            }
            catch (Exception ex)
            {
                ex.Show("running Encoder");
            }
        }
    }
}
