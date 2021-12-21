using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Subdivider.Imaging
{
	public static class ImageProcessing
	{

		public static Image<Bgr, Byte> ProcessCanny(Image<Bgr, Byte> image, double thresh, double threshLinking )
		{
			image = image.Convert<Gray, byte>()
				.Canny(thresh, threshLinking)
				.ThresholdBinaryInv(new Gray(150), new Gray(255))
				.Convert<Bgr, byte>();
			return image;
		}

	}
}
