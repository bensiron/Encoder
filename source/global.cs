using System;
using System.Windows;
using log4net;
using log4net.Config;
using UGTS.UI;

namespace UGTS.Encoder
{
    public static class MGlobal
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (MGlobal));

        [STAThread]
        public static void Main()
        {
            XmlConfigurator.Configure();
            log.Info("run begin");

            MUI.Run(Run, "Encoder");
            WPF.MMain.IconPath = "encoder.ico";

            log.Info("run end");
        }

        private static void Run()
        {
            var w = new SecureStringWindow {WindowStartupLocation = WindowStartupLocation.CenterScreen};
            w.ShowDialog();
        }
    }
}
