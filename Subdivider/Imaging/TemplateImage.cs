using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Prism.Mvvm;
using System.IO;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using System.Collections.ObjectModel;
using System.Linq;
using Subdivider.Struct;
using Subdivider.CustomControls.UIElements;

namespace Subdivider.Imaginng
{
    public enum DisplayImage
    {
        Origninal,
        Working
    }

    public class TemplateImage: BindableBase
    {
		#region BindableProperties

		#region private

        private ObservableCollection<Rectangle> pageRois;
        private ObservableCollection<Rectangle> overlapRois;
        private double overlapPercentage;
        private bool overlap = false;
        private bool canny = false;

		#endregion

		#region Public

		/// <summary>
		/// Page ROIs used to designate Scaled portion of image 
		/// </summary>
		public ObservableCollection<Rectangle> PageRois
		{
			get {  return pageRois; }
			set
			{
                SetProperty(ref this.pageRois, value);
			}
		}

        /// <summary>
        /// ROIs used for the overlap percentage of the image
        /// </summary>
        public ObservableCollection<Rectangle> OverlapRois
		{
			get {  return overlapRois; }
			set
			{
                SetProperty(ref this.overlapRois, value);
			}
        }

        /// <summary>
        /// Bool for overlaping or not
        /// </summary>
        public bool Overlap
		{
            get { return this.overlap;}
			set
			{
                SetProperty(ref this.overlap, value);
			}
		}

        /// <summary>
        /// Overlap percentage.  (Converted later for calculation to decimal) 
        /// </summary>
        public double OverlapPercentage
		{
			get { return this.overlapPercentage;}
			set
			{
                SetProperty(ref this.overlapPercentage, value);
			}
		}

        public bool Canny
		{
			get { return this.canny;}
			set
			{
                SetProperty(ref this.canny, value);
            }
		}

		#endregion

		#endregion BindableProperties

		#region Properties

        /// <summary>
        /// Original loaded image
        /// </summary>
		public Image<Bgr, Byte> OriginalImage { get; set; }
        /// <summary>
        /// Image Used for processing
        /// </summary>
        public Image<Bgr, Byte> WorkingImage { get; set;}

        public static TemplateImage copyTemplate;
        /// <summary>
        /// Pixels per inch setting
        /// </summary>
        public double PPI { get; set;}
        /// <summary>
        /// Selected Paper sized
        /// </summary>
        public PaperSize PaperSize { get; set;} = Papers.Sizes["USLetter"];
        /// <summary>
        /// Page ROI Color
        /// </summary>
        public LineColor PageROIColor  { get; set; } = Lines.Color["Green"];
        /// <summary>
        /// Overlaping ROI Color
        /// </summary>
        public LineColor OverlapROIColor { get; set; } = Lines.Color["Red"];
        /// <summary>
        /// Selector Color
        /// </summary>
        public LineColor SelectorColor { get; set; } = Lines.Color["Red"];

        #endregion Properties

		#region Constructor
        /// <summary>
        /// Template Image constructor
        /// </summary>
        /// <param name="path">Path to image file.</param>
        /// <param name="paperSize">Paper size start</param>
        /// <param name="pageROIColor">page roi color</param>
        /// <param name="OverlapROIColor"> Overlap ROI Color</param>
        /// <param name="selectorColor">Selection tool color</param>
		public TemplateImage(string path, PaperSize paperSize, LineColor pageROIColor,
            LineColor OverlapROIColor, LineColor selectorColor, bool overlap, double overlapPercentage)
        {
            this.PaperSize = paperSize;
            this.PageROIColor = pageROIColor;
            this.SelectorColor = selectorColor;
            this.OriginalImage = new Image<Bgr, byte>(path);
            this.WorkingImage = this.OriginalImage.Clone();
            this.Overlap = overlap;
            this.OverlapPercentage = overlapPercentage * .01;
        }

		#endregion Constructor

		#region Private Supporting Methods

        #endregion Private Supporting Methods


        #region Public Methods

        /// <summary>
        /// Recalculates the template rois and overlaps
        /// </summary>
        public void RecalculateTemplate()
        {

			if (Overlap)
			{
                (PageRois, OverlapRois) = CreatePageAndOverlapROIs(PPI, OverlapPercentage);
			}
			else
			{
                PageRois = CreatePageROIs(this.PPI);
                OverlapRois = null;
			}
        }

        /// <summary>
        /// Calculate and set page ROIS based on without overlap
        /// </summary>
        /// <param name="ppi">Calculated PPI</param>
        /// <returns>Calculated page rois</returns>
        public ObservableCollection<System.Drawing.Rectangle> CreatePageROIs(double ppi)
        {
            //Create Page ROIS
            int xposition = 0;
            int yposition = 0;
            Image<Bgr, byte> ImageToCalcRoisWith = this.WorkingImage;

           ObservableCollection<System.Drawing.Rectangle> rois = new ObservableCollection<System.Drawing.Rectangle>();

            while(yposition <= ImageToCalcRoisWith.Rows)
            {
                while(xposition <= ImageToCalcRoisWith.Cols)
                {
                    System.Drawing.Rectangle roi = new System.Drawing.Rectangle(new System.Drawing.Point(xposition,yposition), new System.Drawing.Size((int)(this.PaperSize.Width * this.PPI), (int)(this.PaperSize.Height * this.PPI)));
                    rois.Add(roi);
                    xposition = xposition += (int)(this.PaperSize.Width * this.PPI);
                }
                yposition = yposition += (int)(this.PaperSize.Height * this.PPI);
                xposition = 0; 
            }
           return rois;
        }

        /// <summary>
        /// Calculate and set page rois with overlap
        /// </summary>
        /// <param name="ppi">calculated PPI</param>
        /// <param name="overlapPercentage">Overlap Percentage</param>
        /// <returns>calculated page rois and overlap rois</returns>
        public (ObservableCollection<System.Drawing.Rectangle>, ObservableCollection<System.Drawing.Rectangle>) CreatePageAndOverlapROIs(double ppi, double overlapPercentage)
        {
            //Create Page ROIS
            int width = (int)(this.PaperSize.Width * this.PPI);
            int height = (int)(this.PaperSize.Height * this.PPI);

            int overLapModifier = (int)(width * overlapPercentage);
            int xposition = 0 - overLapModifier/2;
            int yposition = 0 - overLapModifier/2;

            Image<Bgr, byte> ImageToCalcRoisWith = this.WorkingImage;

            ObservableCollection<System.Drawing.Rectangle> pageRois = new ObservableCollection<System.Drawing.Rectangle>();
            ObservableCollection<System.Drawing.Rectangle> overlapRois = new ObservableCollection<System.Drawing.Rectangle>();

            while (yposition <= ImageToCalcRoisWith.Rows)
            {
                while (xposition <= ImageToCalcRoisWith.Cols)
                {
                    System.Drawing.Rectangle overlapRoi = new System.Drawing.Rectangle(new System.Drawing.Point(xposition, yposition), new System.Drawing.Size(width, height));
                    System.Drawing.Rectangle pageRoi = new System.Drawing.Rectangle(new System.Drawing.Point(xposition + overLapModifier/2, yposition + overLapModifier/2), new System.Drawing.Size(width-overLapModifier, height-overLapModifier));
                    overlapRois.Add(overlapRoi);
                    pageRois.Add(pageRoi);
                    xposition = xposition += (int)(this.PaperSize.Width * this.PPI) - overLapModifier;
                }
                yposition = yposition += (int)(this.PaperSize.Height * this.PPI) - overLapModifier;
                xposition = 0 - overLapModifier/2;
            }


            return (pageRois, overlapRois);
        }


        /// <summary>
        /// Draw page rois on image for PDF
        /// </summary>
        /// <param name="m">Image Mat</param>
        /// <param name="rois">Page ROIs</param>
        public void DrawPageRois(Mat m, ObservableCollection<System.Drawing.Rectangle> rois)
        {
            Rectangle roiExample = rois[0];
            var color = new MCvScalar(this.PageROIColor.Blue, this.PageROIColor.Green, this.PageROIColor.Red);
            foreach(System.Drawing.Rectangle r in rois)
            {
                CvInvoke.Rectangle(m, r, color, 1, LineType.EightConnected, 0);
            }
        }

        public void CropAndSaveBitmaps(Image<Bgr, Byte> image, List<System.Drawing.Rectangle> rois)
        {
            int cropNumber = 0;
            foreach (System.Drawing.Rectangle roi in rois)
            {
                image.ROI = roi;
                Image<Bgr, Byte> tmp = image.Copy();
                string filename = "Crop-"+ cropNumber +".bmp";
                tmp.Mat.ToBitmap().Save(filename);
                cropNumber++;
            }
        }

        /// <summary>
        /// Build pdf doc
        /// </summary>
        /// <param name="image"></param>
        /// <param name="rois"></param>
        public PdfDocument BuildPdfDoc()
        {

            var image = this.WorkingImage.Clone();
            //if(enableRegistrations)
            //    DrawRegistrationMarks(image.Mat, pageROIS);

            PdfDocument pdfDoc = new PdfDocument();
    
            int cropNumber = 0;

            ObservableCollection<Rectangle> roisToUse;
            int overLapModifierX = 0;
            int overLapModifierY= 0;
            if (overlap)
			{
                roisToUse = OverlapRois;
                image = this.WorkingImage.Clone();
                DrawPageRois(image.Mat, PageRois);
            }
            else
                roisToUse = PageRois;

            var useAllPages = false;
            if(ButtonRectangle.chosenPages.Where(x => x.Clicked == true).Count() == 0)
                useAllPages = true;


            foreach (System.Drawing.Rectangle roi in roisToUse)
            {
                if (ButtonRectangle.chosenPages[cropNumber].Clicked == true || useAllPages) {
                    image.ROI = roi;
                    Image<Bgr, Byte> tmp = image.Copy();
                    //tmp = tmp.Resize((int)(this.PPI * this.PaperSize.Width), (int)(this.PPI*this.PaperSize.Height), Inter.Linear);
                    //Console.WriteLine(pdfDoc.PageCount);
                    PdfPage page = pdfDoc.AddPage();
                    page.Width = (int)(this.PPI * (this.PaperSize.Width));
                    page.Height = (int)(this.PPI * (this.PaperSize.Height));
                    XGraphics xgr = XGraphics.FromPdfPage(page);
                    string filename = "Crop-" + cropNumber + ".bmp";
                    //tmp.Mat.ToBitmap().Save(memoryStream);
                    byte[] bytes = ImageToBytes(tmp.Mat.ToBitmap());
                    XImage croppedImage = XImage.FromStream(() => new MemoryStream(bytes));

                    if(roi.Location.X <0)
                        overLapModifierX = (int)((this.PaperSize.Width * this.PPI)* overlapPercentage);
                    else
                        overLapModifierX = 0;

                    if(roi.Location.Y < 0)
                        overLapModifierY = (int)((this.PaperSize.Width * this.PPI) * overlapPercentage);
                    else
                        overLapModifierY = 0;

                    if (tmp.Width != page.Width || tmp.Height != page.Height)
                    {
                        if (tmp.Width == page.Width && tmp.Height != page.Height)
                        {
                            xgr.DrawImage(croppedImage, 0 + overLapModifierX/2, 0 + overLapModifierY / 2, page.Width, tmp.Mat.Height);
                        }
                        if (tmp.Width != page.Width && tmp.Height == page.Height)
                        {
                            xgr.DrawImage(croppedImage, 0 + overLapModifierX / 2, 0 + overLapModifierY/ 2, tmp.Mat.Width, page.Height);
                        }
                        if (tmp.Width != page.Width && tmp.Height != page.Height)
                        {
                            xgr.DrawImage(croppedImage, 0 + overLapModifierX / 2, 0 + overLapModifierY / 2, tmp.Mat.Width, tmp.Mat.Height);
                        }
                    }
                    else
                    {
                        xgr.DrawImage(croppedImage, 0 + overLapModifierX / 2, 0 + overLapModifierY / 2, page.Width, page.Height);
                    }
                }
                cropNumber++;
            }

           return pdfDoc;
        }

        //public static Stream GetStream(Image<Bgr,Byte> image)
        //{
        //    return new MemoryStream(image.Bytes);
        //}

        public static byte[] ImageToBytes(System.Drawing.Image img)
        {
            using (var stream = new MemoryStream())
            { 
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
        #endregion Supporting Methods

	}
}
