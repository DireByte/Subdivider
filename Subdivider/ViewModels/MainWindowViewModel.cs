using Emgu.CV;
using Emgu.CV.Structure;
using Subdivider.CustomControls.UIElements;
using Subdivider.Imaginng;
using Subdivider.Struct;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Subdivider.Imaging;

namespace Subdivider.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

        #region private Properties

        #region Theme

        private string _title = "Subdivider";
        private string _theme = "Light";

        #endregion Theme

        #region Settings

        //Scale Settings
        private bool isInches = true;
        private double selectionLength = 1.0;
        private string ppi = "300";
        private ObservableCollection<string> paperSizes;
        private string selectedPaperSize = "USLetter";
        private bool enableOverlap = false;
        private int overlapValue = 0;
        bool enablePageSelection = false;
        private PointB point1;
        private PointB point2;

        //Color Settings
        private ObservableCollection<string> colors;
        private string pageROISelectedColor = "Red";
        private string overlapSelectedROIColor = "Green";
        private string selectorSelectedColor = "Yellow";
        private LineColor selectorColor = Lines.Color["Yellow"];
        private LineColor pageROIColor = Lines.Color["Red"];
        private LineColor overlapROIColor = Lines.Color["Green"];

        //Image Processing
        private bool enableCanny = false;
        private double cannyThresh = 150;
        private double cannyThreshLinking = 150;

        // App Functionality
        private bool enablePDFAutoOpen = true;
        
		#endregion Settings

		private TemplateImage templateImage;
        private BitmapSource displayImage;
        public static double imageWidth;
        public static double imageHeight;



		#endregion

		#region Public Properties

		#region
        public string LinearDimentionToolTip => "Distance between the two selected points on the image.";
        public string EnableOverlapToolTip => "Enables overlapping of page ROIs (Region of Interest) based on selected percentage. " +
            "Cut lines will be added to the resulting pdf for simple alignment.";
        public string EnablePageSelectionToolTip => "Enables the selection of specific pages and omission of the unselected.";
        public string CannyEdgesToolTip => "Binarizes the image and detects edges based on the Binarization Threshold and Edge Linking Threshold.  " +
            "This is very useful when attempting to get a rough outline of an object and would like to save ink.";

		#endregion

		#region Theme
		public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }


        public string ThemeSwitch
        {
            get { return _theme; }
            set { SetProperty(ref _theme, value); }
        }

        #endregion Theme

        #region Settings

        #region Scale Settings
        /// <summary>
        /// Radio Button property for IsInches
        /// </summary>
        public bool IsInches
        {
            get { return isInches; }
            set
            {
                SetProperty(ref isInches, value);
            }
        }
        /// <summary>
        /// Selection Length Creates a new PPI based on selection length and the unit selected.
        /// </summary>
        public double SelectionLength
        {
            get { return this.selectionLength; }
            set
            {
                this.PPI = Math.Round(300 * (value * DetermineUnit())).ToString();
                SetProperty(ref selectionLength, value);
            }
        }

        public string PPI
        {
            get { return ppi; }
            set { SetProperty(ref ppi, value); }
        }

        /// <summary>
        /// Paper Sizes for selection box
        /// </summary>
        public ObservableCollection<string> PaperSizes
        {
            get { return paperSizes; }
            set
            {
                SetProperty(ref paperSizes, value);
            }
        }

        /// <summary>
        /// Selected paper size
        /// </summary>
        public string SelectedPaperSize
        {
            get { return selectedPaperSize; }
            set
            {
                SetProperty(ref selectedPaperSize, value);
                ChangePaperSize();
            }
        }

        /// <summary>
        /// Overlap enable toggle
        /// </summary>
        public bool EnableOverlap
        {
            get { return this.enableOverlap; }
            set
            {
                SetProperty(ref enableOverlap, value);
                if (TemplateImage != null)
                    this.TemplateImage.Overlap = value;

            }
        }

        /// <summary>
        /// Overlap Value percentage 0-20
        /// </summary>
        public int OverlapValue
        {
            get { return overlapValue; }
            set
            {
                SetProperty(ref overlapValue, value);
                if (this.TemplateImage != null)
                    this.TemplateImage.OverlapPercentage = value * .01;
            }
        }

        /// <summary>
        /// Enables Page selection overlay
        /// </summary>
        public bool EnablePageSelection
        {
            get { return this.enablePageSelection; }
            set
            {
                SetProperty(ref this.enablePageSelection, value);
            }
        }

        /// <summary>
        /// Selection point 1 (Left CLick on image view tool)
        /// </summary>
        public PointB Point1
        {
            get { return point1; }
            set
            {
                SetProperty(ref point1, value);
            }

        }

        /// <summary>
        /// Selection point 2 (Right CLick on image view tool)
        /// </summary>
        public PointB Point2
        {
            get { return point2; }
            set
            {
                SetProperty(ref point2, value);
            }

        }
        #endregion Scale Settings

        #region Color Settings

        /// <summary>
        /// Coloers possible to select for UI elements
        /// </summary>
        public ObservableCollection<string> Colors
        {
            get { return colors; }
            set
            {
                SetProperty(ref colors, value);
            }
        }

        /// <summary>
        /// Page ROI Color
        /// </summary>
        public string PageROISelectedColor
        {
            get { return pageROISelectedColor; }
            set
            {
                SetProperty(ref pageROISelectedColor, value);
                PageROIColor = Lines.Color[value];
                ChangeColor();
            }
        }

        /// <summary>
        /// Overlap ROI Selected Color
        /// </summary>
        public string OverlapROISelectedColor
        {
            get { return overlapSelectedROIColor; }
            set
            {
                SetProperty(ref overlapSelectedROIColor, value);
                OverlapROIColor = Lines.Color[value];
                ChangeColor();
            }
        }

        /// <summary>
        /// Selected selection tool color
        /// </summary>
        public string SelectorSelectedColor
        {
            get { return selectorSelectedColor; }
            set
            {
                SetProperty(ref selectorSelectedColor, value);
                SelectorColor = Lines.Color[value];
            }
        }
        /// <summary>
        /// selection tool color object
        /// </summary>
        public LineColor SelectorColor
        {
            get { return selectorColor; }
            set
            {
                SetProperty(ref selectorColor, value);
            }
        }
        /// <summary>
        /// page roi selected colot object
        /// </summary>
        public LineColor PageROIColor
        {
            get { return pageROIColor; }
            set
            {
                SetProperty(ref pageROIColor, value);
            }
        }
        /// <summary>
        /// Color for the oberlap ROIs
        /// </summary>
        public LineColor OverlapROIColor
        {
            get { return overlapROIColor; }
            set
            {
                SetProperty(ref overlapROIColor, value);
            }
        }


		#endregion Color Settings

		#region Image Processing Settings
		public bool EnableCanny
        {
            get { return this.enableCanny; }
            set
            {
                SetProperty(ref enableCanny, value);
                ReprocessImageProcessing();
            }
        }

        public double CannyThresh
		{
			get { return this.cannyThresh;}
			set
			{
                SetProperty(ref this.cannyThresh, value);
                ReprocessImageProcessing();
			}
		}
        public double CannyThreshLinking
        {
            get { return this.cannyThreshLinking; }
            set
            {
                SetProperty(ref this.cannyThreshLinking, value);
                ReprocessImageProcessing();
            }
        }

        #endregion Image Processing Settings

        #endregion Settings 


        public TemplateImage TemplateImage
        {
            get { return templateImage;}
            set {  SetProperty(ref templateImage, value);}
        }

        public BitmapSource DisplayImage
		{
			get { return this.displayImage;}
			set
			{
                SetProperty(ref this.displayImage, value);
			}
		}

        #endregion

        #region Commands

        public DelegateCommand OpenImageCommand { get; private set;}
        public DelegateCommand SliceImageCommand { get; private set;}
        public DelegateCommand ExportImageCommand { get; private set; }
        public DelegateCommand Recalculate { get; private set;}
        public DelegateCommand ThemeSwitchCommand { get; private set; }
        public DelegateCommand LicenseCommand { get; private set; }

        public DelegateCommand PDFCommand { get; private set; }
        public DelegateCommand HelpCommand { get; private set; }

        public DelegateCommand CoffeeCommand { get; private set; }
        public DelegateCommand WebsiteCommand { get; private set; }
        public DelegateCommand UnitCommand { get; private set; }

        #endregion

        #region Constuctor
        public MainWindowViewModel()
        {
            PaperSizes = new ObservableCollection<string>(Papers.Sizes.Keys);
            Colors= new ObservableCollection<string>(Lines.Color.Keys);
            OpenImageCommand = new DelegateCommand(LoadImage);
            ExportImageCommand = new DelegateCommand(Export);
            Recalculate = new DelegateCommand(Recalc);
            ThemeSwitchCommand = new DelegateCommand(ThemeSwap);
            LicenseCommand = new DelegateCommand(LicenseDisplay);
            HelpCommand = new DelegateCommand(HelpDisplay);
            CoffeeCommand = new DelegateCommand(CoffeeDisplay);
            WebsiteCommand = new DelegateCommand(WebsiteDisplay);
            //SliceImageCommand = new DelegateCommand(Slice);
            //PDFCommand = new DelegateCommand(PDFDisplay);
            //UnitCommand = new DelegateCommand(UnitSet);
            //MouseClickedCommand = new DelegateCommand(GetPosition);
            //LeftClickCommand = new DelegateCommand(LeftPosition(sender, e));
            //RightClickCommand = new DelegateCommand(RightPosition(sender, e));
        }

        #endregion

        #region Command Methods
        public void LoadImage()
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png",
                DereferenceLinks = false
            };

            if (ofd.ShowDialog() == true)
            {
                TemplateImage = new TemplateImage(ofd.FileName, Papers.Sizes[SelectedPaperSize], Lines.Color[PageROISelectedColor], Lines.Color[OverlapROISelectedColor], Lines.Color[SelectorSelectedColor], this.EnableOverlap, this.OverlapValue);
                imageWidth = this.templateImage.OriginalImage.Width;
                imageHeight = this.templateImage.OriginalImage.Height;
                DisplayImage = TemplateImage.WorkingImage.ToBitmapSource();
                ReprocessImageProcessing();
            }

        }
        /// <summary>
        /// Used 
        /// </summary>
        public void Recalc()
        {

            if (this.point1.X != 0 && this.point1.Y != 0 && this.point2.X != 0 && this.point2.Y != 0)
            {

                double distanceBetween = pythag(new Point(this.point1.X, this.point1.Y),
                    new Point(this.point2.X, this.point2.Y));

                this.templateImage.PPI = distanceBetween / (this.SelectionLength * DetermineUnit());
                this.PPI = (distanceBetween / (this.SelectionLength * DetermineUnit())).ToString();

                this.TemplateImage = this.TemplateImage;
                Slice();

            }
        }
        /// <summary>
        /// Essentially recalculates the image
        /// </summary>
        public void Slice()
        {

            if (templateImage == null)
                return;

            templateImage.PPI = Double.Parse(this.PPI);
            templateImage.RecalculateTemplate();
        }

        /// <summary>
        /// Export to pdf method
        /// </summary>
        private void Export()
        {
			if (this.TemplateImage == null) { 
                return;
            }
            if (TemplateImage.PageRois == null)
            {
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF Files | *.pdf";

            if (sfd.ShowDialog() == true)
            {
                var doc = templateImage.BuildPdfDoc();
                doc.Save(sfd.FileName);

                ShowPdf(sfd.FileName);

            }
        }

        private void ThemeSwap()
        {
            if (_theme == "Dark")
            {
                ThemeSwitch = "Light";
            }
            if (_theme == "Light")
            {
                ThemeSwitch = "Dark";
            }
        }


        private void LicenseDisplay()
        {
            var window = new License();
            window.DataContext = this;
            window.Show();
        }


        private void HelpDisplay()
        {
            ShowPdf(AppDomain.CurrentDomain.BaseDirectory + "yes.pdf");
        }

        private void WebsiteDisplay()
        {

            var psi = new ProcessStartInfo
            {
                FileName = "https://www.direbyte.com/",
                UseShellExecute = true
            };
            Process.Start(psi);

        }


        private void CoffeeDisplay()
        {

            var psi = new ProcessStartInfo
            {
                FileName = "https://www.buymeacoffee.com/direbyte",
                UseShellExecute = true
            };
            Process.Start(psi);

        }




		#endregion Command Methods

		#region Private Methods

		private void ReprocessImageProcessing()
		{
            if(TemplateImage != null) 
            { 
                //reset image
                TemplateImage.WorkingImage = TemplateImage.OriginalImage.Clone();

				if (EnableCanny)
				{
                        var newImage = ImageProcessing.ProcessCanny(
                        TemplateImage.WorkingImage,
                        CannyThresh,
                        CannyThreshLinking);
                    TemplateImage.WorkingImage = newImage;
                   
				}

                DisplayImage = TemplateImage.WorkingImage.ToBitmapSource();
            }
        }

		private double pythag(Point p1, Point p2)
        {
            double ydiff = Math.Abs(p1.Y - p2.Y);
            double xdiff = Math.Abs(p1.X - p2.X);

            return Math.Sqrt(Math.Pow(ydiff,2) + Math.Pow(xdiff,2));
        }


        private void ChangePaperSize()
        {
            if(this.templateImage != null)
            {
                this.templateImage.PaperSize = Papers.Sizes[SelectedPaperSize];
                Slice();
            }
        }

        private void ChangeColor()
        {

            if (this.templateImage != null)
            {
                this.templateImage.PageROIColor = Lines.Color[PageROISelectedColor];
                this.templateImage.OverlapROIColor = Lines.Color[OverlapROISelectedColor];
                this.templateImage.SelectorColor = Lines.Color[SelectorSelectedColor];
            }
        }

        private double DetermineUnit()
        {
            if (this.isInches == true)
            {
                return 1;
            }
            else
            {
                return .393701;
            }
        }

        private void ShowPdf(string pdfpath) {
            if (enablePDFAutoOpen == true) {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo(pdfpath)
                {
                    UseShellExecute = true
                };
                p.Start();
            };
        }


     #endregion


    }
}
