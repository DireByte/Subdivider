using Emgu.CV;
using Emgu.CV.Structure;
using I2T.CustomControls.UIElements;
using I2T.Imaginng;
using I2T.Struct;
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

namespace I2T.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

        #region private Properties

        private string _title = "I2T";
        private string _theme = "Light";
        private double selectionLength = 1.0;
        private string ppi = "300";
        private bool isInches = true;
        private TemplateImage templateImage;
        private ObservableCollection<string> paperSizes;
        private ObservableCollection<string> colors;
        public Image<Bgr, Byte> temp { get; set; }
        private string selectedPaperSize = "USLetter";
        private string pageROISelectedColor = "Red";
        private string overlapSelectedROIColor = "Green";
        private string selectorSelectedColor = "Yellow";
        
        private PointB point1;
        private PointB point2;

        public static double imageWidth;
        public static double imageHeight;

        private bool enablePDFAutoOpen = true;
        private bool enableOverlap = false;
        private int overlapValue = 0;

        private bool pageSelect = false;


        private LineColor selectorColor = Lines.Color["Yellow"];
        private LineColor pageROIColor = Lines.Color["Red"];
        private LineColor overlapROIColor = Lines.Color["Green"];

        #endregion


        #region Public Properties

        /// <summary>
        /// Paper Sizes for selection box
        /// </summary>
        public ObservableCollection<string> PaperSizes 
        {
            get { return paperSizes;}
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
            get { return selectedPaperSize;}
            set
            {
                SetProperty(ref selectedPaperSize, value);
                ChangePaperSize();
            }
        }


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
        public LineColor SelectorColor { 
            get{return selectorColor; }
            set{
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

        public LineColor OverlapROIColor
        {
            get { return overlapROIColor; }
            set
            {
                SetProperty(ref overlapROIColor, value);
            }
        }

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

        public TemplateImage TemplateImage
        {
            get { return templateImage;}
            set {  SetProperty(ref templateImage, value);}
        }

        public double SelectionLength
        {
            get { return this.selectionLength; }
            set
            {

                this.CurrentPPI = Math.Round(300 * (value*DetermineUnit())).ToString();

                Debug.WriteLine("1:" + this.CurrentPPI);
                SetProperty(ref selectionLength, value);
            }
        }

        public string CurrentPPI
        {
            get { return ppi;}
            set { SetProperty(ref ppi, value);}
        }

        public bool SetUnit
        {
            get { return isInches; }
            set
            {
                SetProperty(ref isInches, value);
            }
        }

        /// <summary>
        /// Selection point 1 (Left CLick on image view tool)
        /// </summary>
        public PointB Point1 
        {
            get {return point1;}
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
            get {return point2; }
            set
            {
                SetProperty(ref point2, value);
            }

        }

        /// <summary>
        /// Overlap enable toggle
        /// </summary>
        public bool EnableOverlap
        {
            get { return enableOverlap; }
            set
            {
                SetProperty(ref enableOverlap, value);
                if(TemplateImage != null)
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
                if(this.TemplateImage != null)
                    this.TemplateImage.OverlapPercentage = value * .01;
            }
        }

        public bool PageSelect
        {
            get { return pageSelect; }
            set { SetProperty(ref pageSelect, value); }
        }


        #endregion

        #region Commands

        public DelegateCommand OpenImageCommand { get; private set;}
        public DelegateCommand SliceImageCommand { get; private set;}
        public DelegateCommand ExportImageCommand { get; private set; }
        public DelegateCommand CanvasClickCommand { get; private set;}
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
            SliceImageCommand = new DelegateCommand(Slice);
            ExportImageCommand = new DelegateCommand(Export);
            CanvasClickCommand = new DelegateCommand(CanvasClickRecalc);
            ThemeSwitchCommand = new DelegateCommand(ThemeSwap);
            LicenseCommand = new DelegateCommand(LicenseDisplay);
            HelpCommand = new DelegateCommand(HelpDisplay);
            CoffeeCommand = new DelegateCommand(CoffeeDisplay);
            WebsiteCommand = new DelegateCommand(WebsiteDisplay);
            //PDFCommand = new DelegateCommand(PDFDisplay);
            //UnitCommand = new DelegateCommand(UnitSet);
            //MouseClickedCommand = new DelegateCommand(GetPosition);
            //LeftClickCommand = new DelegateCommand(LeftPosition(sender, e));
            //RightClickCommand = new DelegateCommand(RightPosition(sender, e));
        }

   





        #endregion


        #region Private Methods

        public void LoadImage()
        {
            OpenFileDialog ofd = new OpenFileDialog() { 
               Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png",
               DereferenceLinks = false,
            };

            if (ofd.ShowDialog() == true)
            {
                TemplateImage = new TemplateImage(ofd.FileName, Papers.Sizes[SelectedPaperSize], Lines.Color[PageROISelectedColor], Lines.Color[OverlapROISelectedColor], Lines.Color[SelectorSelectedColor], this.EnableOverlap, this.OverlapValue);
                imageWidth = this.templateImage.OriginalImage.Width;
                imageHeight = this.templateImage.OriginalImage.Height;
                //Markers.canvasBoy.Width = width;
                //Markers.canvasBoy.Height = height;
                //this.DisplayBitmap = this.templateImage.GetBitmapImageSource(BitmapSelection.Original);

            }
                     
        }
        public void CanvasClickRecalc()
        {
             
            if(this.point1.X != 0 && this.point1.Y != 0 && this.point2.X != 0 && this.point2.Y !=0)
            {

                double distanceBetween = pythag(new Point(this.point1.X,this.point1.Y) ,
                    new Point(this.point2.X, this.point2.Y));

                this.templateImage.PPI = distanceBetween/(this.SelectionLength * DetermineUnit());
                this.CurrentPPI =(distanceBetween/(this.SelectionLength * DetermineUnit())).ToString();

                this.TemplateImage = this.TemplateImage;
                Slice();

            }
        }

        public double pythag(Point p1, Point p2)
        {
            double ydiff = Math.Abs(p1.Y - p2.Y);
            double xdiff = Math.Abs(p1.X - p2.X);

            return Math.Sqrt(Math.Pow(ydiff,2) + Math.Pow(xdiff,2));
        }


        /// <summary>
        /// Essentially recalculates the image
        /// </summary>
        public void Slice()
        {

            if( templateImage == null)
                return;

            templateImage.PPI = Double.Parse(this.CurrentPPI);
            templateImage.RecalculateTemplate();
            templateImage.RedrawTemplate();
            //this.DisplayBitmap = this.templateImage.GetBitmapImageSource(BitmapSelection.Working);
        }

        /// <summary>
        /// Export to pdf method
        /// </summary>
        private void Export()
        {

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF Files | *.pdf";

            if (sfd.ShowDialog() == true)
            {
                var doc = templateImage.BuildPdfDoc();
                doc.Save(sfd.FileName);
                //this.DisplayBitmap = this.templateImage.GetBitmapImageSource(BitmapSelection.Working);

                ShowPdf(sfd.FileName);

            }
        }


        
        public void ChangePaperSize()
        {
            if(this.templateImage != null)
            {
                this.templateImage.PaperSize = Papers.Sizes[SelectedPaperSize];
                Slice();
            }
        }

        public void ChangeColor()
        {

            if (this.templateImage != null)
            {
                this.templateImage.PageROIColor = Lines.Color[PageROISelectedColor];
                this.templateImage.OverlapROIColor = Lines.Color[OverlapROISelectedColor];
                this.templateImage.SelectorColor = Lines.Color[SelectorSelectedColor];
            }
        }

        public double DetermineUnit()
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
