using System;
using System.Windows;

namespace UGTS.Encoder
{
    public static class ProgramEntry
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                WPF.MMain.IconPath = "encoder.ico";
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
