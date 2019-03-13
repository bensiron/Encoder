using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;

namespace UGTS.Encoder.WPF
{
    public static class IconReader
    {
        public static ImageSource ReadIcon(string path)
        {
            using (var s = GetResourceStream(Assembly.GetEntryAssembly(), path))
            {
                return System.Windows.Media.Imaging.BitmapFrame.Create(s);
            }
        }

        private static Stream GetResourceStream(Assembly asm, string name)
        {
            var foundName = asm.GetManifestResourceNames().FirstOrDefault(found => found.EndsWith(name));
            return asm.GetManifestResourceStream(foundName);
        }
    }
}
