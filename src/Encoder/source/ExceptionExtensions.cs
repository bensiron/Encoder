using System;
using System.Windows.Forms;

namespace UGTS.Encoder
{
    public static class ExceptionExtensions
    {
        public static void Show(this Exception ex, string action)
        {
            MessageBox.Show($"An exception occurred while {action} + {ex.Message}", "Encoder Exception");
        }
    }
}
