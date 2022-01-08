using Emgu.CV;
using Emgu.CV.Structure;
using Subdivider.Struct;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Subdivider.Imaging
{
	public static class ImageProcessing
	{

		public static class Alteration { 

		public static Image<Bgr, Byte> ProcessCanny(Image<Bgr, Byte> image, double thresh, double threshLinking )
		{
			image = image.Convert<Gray, byte>()
				.Canny(thresh, threshLinking)
				.ThresholdBinaryInv(new Gray(150), new Gray(255))
				.Convert<Bgr, byte>();
			return image;
		}

		}

		public static class Opperation
		{

            /// <summary>
            /// Calculate and set page rois with overlap
            /// </summary>
            /// <param name="ppi">calculated PPI</param>
            /// <param name="overlapPercentage">Overlap Percentage</param>
            /// <returns>calculated page rois and overlap rois</returns>
            public static (ObservableCollection<System.Drawing.Rectangle>, ObservableCollection<System.Drawing.Rectangle>) 
                CreatePageAndOverlapROIs(
                Image<Bgr, byte> image, 
                PaperSize paperSize, 
                double ppi, 
                double overlapPercentage)
            {
                //Create Page ROIS
                int width = (int)(paperSize.Width * ppi);
                int height = (int)(paperSize.Height * ppi);

                int overLapModifier = (int)(width * overlapPercentage);
                int xposition = 0 - overLapModifier / 2;
                int yposition = 0 - overLapModifier / 2;


                ObservableCollection<System.Drawing.Rectangle> pageRois = new ObservableCollection<System.Drawing.Rectangle>();
                ObservableCollection<System.Drawing.Rectangle> overlapRois = new ObservableCollection<System.Drawing.Rectangle>();

                while (yposition <= image.Rows)
                {
                    while (xposition <= image.Cols)
                    {
                        System.Drawing.Rectangle overlapRoi = new System.Drawing.Rectangle(new System.Drawing.Point(xposition, yposition), new System.Drawing.Size(width, height));
                        System.Drawing.Rectangle pageRoi = new System.Drawing.Rectangle(new System.Drawing.Point(xposition + overLapModifier / 2, yposition + overLapModifier / 2), new System.Drawing.Size(width - overLapModifier, height - overLapModifier));
                        overlapRois.Add(overlapRoi);
                        pageRois.Add(pageRoi);
                        xposition = xposition += (int)(paperSize.Width * ppi) - overLapModifier;
                    }
                    yposition = yposition += (int)(paperSize.Height * ppi) - overLapModifier;
                    xposition = 0 - overLapModifier / 2;
                }


                return (pageRois, overlapRois);
            }

            /// <summary>
            /// Calculate and set page ROIS based on without overlap
            /// </summary>
            /// <param name="ppi">Calculated PPI</param>
            /// <returns>Calculated page rois</returns>
            public static ObservableCollection<System.Drawing.Rectangle> CreatePageROIs(
                Image<Bgr, byte> image, 
                PaperSize paperSize, 
                double ppi)
            {
                //Create Page ROIS
                int xposition = 0;
                int yposition = 0;

                ObservableCollection<System.Drawing.Rectangle> rois = new ObservableCollection<System.Drawing.Rectangle>();

                while (yposition <= image.Rows)
                {
                    while (xposition <= image.Cols)
                    {
                        System.Drawing.Rectangle roi = new System.Drawing.Rectangle(new System.Drawing.Point(xposition, yposition), new System.Drawing.Size((int)(paperSize.Width * ppi), (int)(paperSize.Height * ppi)));
                        rois.Add(roi);
                        xposition = xposition += (int)(paperSize.Width * ppi);
                    }
                    yposition = yposition += (int)(paperSize.Height * ppi);
                    xposition = 0;
                }
                return rois;
            }




        }

	}
}
