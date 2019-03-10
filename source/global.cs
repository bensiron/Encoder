using System;
using System.Windows;
using log4net;
using log4net.Config;
using UGTS.UI;

namespace UGTS.Encoder
{
    public static class MGlobal
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (MGlobal));

        [STAThread]
        public static void Main()
        {
            XmlConfigurator.Configure();
            Log.Info("run begin");

            WPF.MMain.IconPath = "encoder.ico";
            MUI.Run(Run, "Encoder");

            Log.Info("run end");
        }

        private static void Run()
        {
            var w = new SecureStringWindow {WindowStartupLocation = WindowStartupLocation.CenterScreen};
            w.ShowDialog();
        }
    }
}
