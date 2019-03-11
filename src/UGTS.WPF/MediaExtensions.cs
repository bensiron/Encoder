using System;
using System.IO;
using System.Windows.Media;

namespace UGTS.WPF
{
    static class MediaExtensions
	{
		/// <summary>
		/// reads the given stream into a bitmap, closes the stream, and returns the bitmap
		/// </summary>
		public static ImageSource XReadIntoImage(this Stream stream)
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

		/// <summary>
		/// returns a new color which is the fraction of the way from the first to the second color
		/// </summary>
		public static Color XBlend(this Color c1, Color c2, double fraction)
		{
			var c = default(Color);
			c.A = Convert.ToByte(Blend(c1.A, c2.A, fraction).XLimit(0, 255));
			c.R = Convert.ToByte(Blend(c1.R, c2.R, fraction).XLimit(0, 255));
			c.G = Convert.ToByte(Blend(c1.G, c2.G, fraction).XLimit(0, 255));
			c.B = Convert.ToByte(Blend(c1.B, c2.B, fraction).XLimit(0, 255));
			return c;
		}

		private static double Blend(double a, double b, double fraction)
		{
			return a + (b - a) * fraction;
		}
	}
}
