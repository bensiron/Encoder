using System;
using System.Windows;
using log4net;
using log4net.Config;

namespace UGTS.Encoder
{
    public static class ProgramEntry
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (MGlobal));

        [STAThread]
        public static void Main()
        {
            try
            {
                XmlConfigurator.Configure();
                Log.Info("run begin");

                WPF.MMain.IconPath = "encoder.ico";

                var w = new SecureStringWindow { WindowStartupLocation = WindowStartupLocation.CenterScreen };
                w.ShowDialog();

                Log.Info("run end");
            }
            catch (Exception ex)
            {
                ex.Show("running Encoder");
            }
        }
    }
}
