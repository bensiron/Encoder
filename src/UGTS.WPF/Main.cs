using UGTS.UI;

namespace UGTS.WPF
{
    public static class MMain
    {
        public static System.Drawing.Icon MainIcon;
        public static System.Windows.Media.ImageSource MainIconImage;
        private static string _iconPath;

        public static string IconPath
        {
            get { return _iconPath; }
            set
            {
                _iconPath = value;
                if (value.XIsBlank())
                    return;

                using (var s = MAssembly.MainAssembly.XGetResourceStream(value))
                {
                    MainIcon = s.XReadIntoIcon();
                    s.Seek(0, System.IO.SeekOrigin.Begin);
                    MainIconImage = s.XReadIntoImage();
                }
            }
        }
    }
}
