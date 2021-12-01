using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace I2T.Imaginng
{
    public static class MatExtensions
    {

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        /// <summary>
        /// Convert Mat to Bitmap Source
        /// </summary>
        /// <param name="image">Likely Image Mat</param>
        /// <returns>BitmapSource used to display image in WPF</returns>
        public static BitmapSource ToBitmapSource(this IInputArray image)
        {
            using(InputArray ia = image.GetInputArray())
            using(Mat m = ia.GetMat())
            using( System.Drawing.Bitmap source = m.ToBitmap())
            {
                IntPtr ptr = source.GetHbitmap();

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr);
                return bs;
            }
        }
    }
}
