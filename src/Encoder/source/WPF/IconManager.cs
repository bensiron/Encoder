﻿using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;

namespace UGTS.Encoder.WPF
{
    public static class IconManager
    {
        public static Icon MainIcon;
        public static ImageSource MainIconImage;
        private static string _iconPath;

        public static string IconPath
        {
            get { return _iconPath; }
            set
            {
                _iconPath = value;

                using (var s = GetResourceStream(MainAssembly, value))
                {
                    MainIcon = ReadIntoIcon(s);
                    s.Seek(0, SeekOrigin.Begin);
                    MainIconImage = ReadIntoImage(s);
                }
            }
        }

        private static Assembly MainAssembly => Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        private static ImageSource ReadIntoImage(Stream stream)
        {
            try
            {
                return stream != null ? System.Windows.Media.Imaging.BitmapFrame.Create(stream) : null;
            }
            catch
            {
                return null;
            }
            finally
            {
                stream?.Close();
            }
        }
        private static Stream GetResourceStream(Assembly asm, string name)
        {
            var foundName = FindResource(asm, name);
            if (foundName == null)
            {
                return null;
            }

            try
            {
                return asm.GetManifestResourceStream(foundName);
            }
            catch
            {
                return null;
            }
        }

        private static string FindResource(Assembly asm, string name)
        {
            if ((asm == null) || name == null)
            {
                return null;
            }

            return asm.GetManifestResourceNames().FirstOrDefault(found => found.EndsWith(name));
        }

        private static Icon ReadIntoIcon(Stream stream)
        {
            try
            {
                return stream != null ? new Icon(stream) : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
