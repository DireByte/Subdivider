using Subdivider.CustomControls.UIElements;
using Subdivider.Imaginng;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Subdivider.CustomControls
{
    public partial class ImageViewControl : UserControl
    {

        #region External Binding Properties

        //dependency property for TemplateImage
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "DisplayImage",
            typeof(BitmapSource),
            typeof(ImageViewControl),
            new FrameworkPropertyMetadata(null,
            FrameworkPropertyMetadataOptions.AffectsRender,
            new PropertyChangedCallback(DisplayImageChanged)
            )
        );

        public BitmapSource DisplayImage //TemplateImage that is bound to ImageProperty
        {
            get { return (BitmapSource)GetValue(ImageSourceProperty); }
            set { 
                SetValue(ImageSourceProperty, value); 
                imageDisplay.Source = value; 
                UpdatePageCanvas();
                ResetCanvas(value.Width, value.Height);
                ResetImageZoomAndLocation();
            }
        }
        private static void DisplayImageChanged(DependencyObject sender, DependencyPropertyChangedEventArgs arg)  //Updates TemplateImage on property changed
        {
            if (sender != null)
            {
                ((ImageViewControl)sender).DisplayImage = (BitmapSource)arg.NewValue;
            }
        }



        //dependency property for PageRois
        public static readonly DependencyProperty PageRoisProperty = DependencyProperty.Register(
            "PageRois",
            typeof(ObservableCollection<System.Drawing.Rectangle>),
            typeof(ImageViewControl),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(displayMarkersChanged))
            );

        public ObservableCollection<System.Drawing.Rectangle> PageRois //PageRois drawing that is bound to PageRoisProperty
		{
			get {  return (ObservableCollection<System.Drawing.Rectangle>)GetValue(PageRoisProperty); }
			set
			{
                SetValue(PageRoisProperty, value);
                ReDrawAll();
			}
		}

        //dependency property for OverlapRois
        public static readonly DependencyProperty OverlapRoisProperty = DependencyProperty.Register(
            "OverlapRois",
            typeof(ObservableCollection<System.Drawing.Rectangle>),
            typeof(ImageViewControl),
             new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(displayMarkersChanged))
            );

        public ObservableCollection<System.Drawing.Rectangle> OverlapRois //OverlapRois drawing that is bound to OverlapRoisProperty
        {
            get { return (ObservableCollection<System.Drawing.Rectangle>)GetValue(OverlapRoisProperty); }
            set
            {
                SetValue(OverlapRoisProperty, value);
                ReDrawAll();
            }
        }

        //dependency property for Point1
        public static readonly DependencyProperty Point1Property = DependencyProperty.Register(
           "Point1",
           typeof(PointB),
           typeof(ImageViewControl)
       );
        public PointB Point1  //Point1 point that is bound to Point1Property
        {
            get { return (PointB)GetValue(Point1Property); }
            set
            {
                SetValue(Point1Property, value);
            }
        }
       
        //dependency property for Point2
        public static readonly DependencyProperty Point2Property = DependencyProperty.Register(
           "Point2",
           typeof(PointB),
           typeof(ImageViewControl)
           );
        public PointB Point2 //Point2 point that is bound to Point2Property
        {
            get { return (PointB)GetValue(Point2Property); }
            set
            {
                SetValue(Point2Property, value);
            }
        }

        //dependency property for SelectorColor
        public static readonly DependencyProperty SelectorColorProperty = DependencyProperty.Register(
           "SelectorColor",
           typeof(LineColor),
           typeof(ImageViewControl),
           new FrameworkPropertyMetadata(Lines.Color["Yellow"],
           FrameworkPropertyMetadataOptions.AffectsRender,
           new PropertyChangedCallback(displayMarkersChanged)
           ));

        public LineColor SelectorColor //SelectorColor LineColor that is bound to SelectorColorProperty
        {
            get { return (LineColor)GetValue(SelectorColorProperty); }
            set
            {
                SetValue(SelectorColorProperty, value);
            }
        }

        //dependency property for PageSelect
        public static readonly DependencyProperty PageSelectProperty = DependencyProperty.Register(
           "PageSelect",
           typeof(bool),
           typeof(ImageViewControl),
           new FrameworkPropertyMetadata(true,
           FrameworkPropertyMetadataOptions.AffectsRender,
           new PropertyChangedCallback(PageSelectChanged)
           ));

        public bool PageSelect //PageSelect bool that is bound to PageSelectProperty
        {
            get { return (bool)GetValue(PageSelectProperty); }
            set
            {
                if(value == true)
                {
                    pageSelectCanvas.Visibility = Visibility.Visible;
                }
                else
                {
                    pageSelectCanvas.Visibility = Visibility.Hidden;
                }

                SetValue(PageSelectProperty, value);
            }
        }

        private static void PageSelectChanged(DependencyObject sender, DependencyPropertyChangedEventArgs arg) //Updates PageSelect if property changed
        {
            if (sender != null)
            {
                ((ImageViewControl)sender).PageSelect = (bool)arg.NewValue;
            }
        }

        //dependency property for PageROIColor
        public static readonly DependencyProperty PageROIColorProperty = DependencyProperty.Register(
          "PageROIColor",
          typeof(LineColor),
          typeof(ImageViewControl),
          new FrameworkPropertyMetadata(Lines.Color["Red"],
          FrameworkPropertyMetadataOptions.AffectsRender,
          new PropertyChangedCallback(displayMarkersChanged)
          ));

        public LineColor PageROIColor //PageROIColor LineColor that is bound to PageROIColorProperty
        {
            get { return (LineColor)GetValue(PageROIColorProperty); }
            set
            {
                SetValue(PageROIColorProperty, value);
            }
        }

        //dependency property for OverlapROIColor
        public static readonly DependencyProperty OverlapROIColorProperty = DependencyProperty.Register(
          "OverlapROIColor",
          typeof(LineColor),
          typeof(ImageViewControl),
          new FrameworkPropertyMetadata(Lines.Color["Green"],
          FrameworkPropertyMetadataOptions.AffectsRender,
          new PropertyChangedCallback(displayMarkersChanged)
          ));

        public LineColor OverlapROIColor //OverlapROIColor that is bound to OverlapROIColorProperty
        {
            get { return (LineColor)GetValue(OverlapROIColorProperty); }
            set
            {
                SetValue(OverlapROIColorProperty, value);
            }
        }
        private static void displayMarkersChanged(DependencyObject sender, DependencyPropertyChangedEventArgs arg) //Updates drawing on any visual properties changing
        {
            if (sender != null)
            {
                ((ImageViewControl)sender).ReDrawAll();
            }
        }

        #endregion

        #region Internal Properties
        
        private bool PointToggler = true;
        private List<UIElement> ClickMarkers = new List<UIElement>();
        private List<UIElement> MainCanvasPageRoiControls = new List<UIElement>();
        private List<UIElement> CornerCanvasPageRoiControls = new List<UIElement>();

        #endregion


        public ImageViewControl()
        {
            InitializeComponent();
        }


        #region Event Handlers

        //private void canvasDisplay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) //places a marker and updates visuals
        //{
        //    if (Keyboard.IsKeyDown(Key.LeftCtrl))
        //    { 
        //         Point1 = new PointB((int)e.GetPosition(canvasDisplay).X, (int)e.GetPosition(canvasDisplay).Y);
        //        ClearMarkersAndLines();
        //        UpdateMarkersAndLines();
        //    }
        //}

        private void canvasDisplay_MouseClickerEvent(object sender, MouseButtonEventArgs e) //places a marker and updates visuals
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if(e.ChangedButton == MouseButton.Left)
				{
                    PointToggler = !PointToggler;
                    Point1 = new PointB((int)e.GetPosition(canvasDisplay).X, (int)e.GetPosition(canvasDisplay).Y);
                    ClearMarkersAndLines();
                    UpdateMarkersAndLines();
                }
				else if(e.ChangedButton == MouseButton.Right)
				{
                    PointToggler = !PointToggler;
                    Point2 = new PointB((int)e.GetPosition(canvasDisplay).X, (int)e.GetPosition(canvasDisplay).Y);
                    ClearMarkersAndLines();
                    UpdateMarkersAndLines();
                }
            }
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e) //resets zoom panel
        {
            ResetImageZoomAndLocation();  
        }

        #endregion

        #region Supporting Methods

        public void ResetImageZoomAndLocation()
		{
            if(DisplayImage != null) { 
            zoomBoi.Reset();
            var zoomRatioHeight = zoomBoi.ActualHeight / DisplayImage.Height;
            var zoomRatioWidth = zoomBoi.ActualWidth / DisplayImage.Width;
            var zoomRatio = zoomRatioHeight > zoomRatioWidth ? zoomRatioWidth : zoomRatioHeight;
            zoomBoi.ZoomTo(zoomRatio, 0, 0);
            }
        }

        public void ResetCanvas(double imageWidth, double imageHeight) //clears canvas
        {

            if (this.canvasDisplay.Width != imageWidth || this.canvasDisplay.Height != imageHeight)
            {
                this.canvasDisplay.Width = imageWidth;
                this.canvasDisplay.Height = imageHeight;
            }
            
            if ((canvasDisplay.Width < Point1.X || canvasDisplay.Width < Point2.X) || 
                (canvasDisplay.Height < Point1.Y || canvasDisplay.Height < Point2.Y))
            {
                Point1 = new PointB(0,0);
                Point2 = new PointB(0,0);
                this.canvasDisplay.Children.Clear();
            }
        }
        

        public void ReDrawAll() //redraws all visuals
		{
            canvasDisplay.Children.Clear();
            UpdateMarkersAndLines();
            DrawPageRois();
            UpdatePageCanvas();
        }

        private void ClearMarkersAndLines() //clears all markers
		{
            foreach(UIElement e in ClickMarkers)
			{
                canvasDisplay.Children.Remove(e);
			}
            ClickMarkers = new List<UIElement>();
		}

        private void ClearMainCanvasRois() //clears main canvas
		{
            foreach(UIElement e in MainCanvasPageRoiControls)
            {
                canvasDisplay.Children.Remove(e);
			}
		}

        private void ClearCornerCanvasRois() //clears selectablepage canvas
		{
            foreach(UIElement e in CornerCanvasPageRoiControls)
            {
                canvasDisplay.Children.Remove(e);
			}
		}
            
        public void UpdateMarkersAndLines() //redraws all lines and markers
        {

            var lineWidth = (int)(canvasDisplay.Width + canvasDisplay.Height) / 2000;
            if (lineWidth < 1)
                lineWidth = 1;

            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb((byte)SelectorColor.Red, (byte)SelectorColor.Green, (byte)SelectorColor.Blue));
            var color = Color.FromRgb((byte)SelectorColor.Red, (byte)SelectorColor.Green, (byte)SelectorColor.Blue);

            if (Point1.X != 0 || Point1.Y != 0) //if point1 is not placed
			{
                var crosshairControl = Crosshair.DrawCrossHair(Point1.X, Point1.Y, color, lineWidth);
                canvasDisplay.Children.Add(crosshairControl);
                ClickMarkers.Add(crosshairControl);

			}
            if (Point2.X != 0 || Point2.Y != 0) //if point2 is not placed
			{
                var crosshairControl = Crosshair.DrawCrossHair(Point2.X, Point2.Y, color, lineWidth);
                canvasDisplay.Children.Add(crosshairControl);
                ClickMarkers.Add(crosshairControl);
			}

            //If points are set
            if ((Point1.X != 0 || Point1.Y != 0) && (Point2.X != 0 || Point2.Y != 0))
            {

                var point1 = new PointB();
                var point2 = new PointB();

                var hypot = (int)Math.Round(Math.Sqrt((Math.Pow(Math.Abs(Point1.X - Point2.X), 2) + Math.Pow(Math.Abs(Point1.Y - Point2.Y), 2))));

                double percent = .9;

                int test = hypot / 100;

                for (int i = 0; i < test; i++)
                {
                    percent += .1;
                }

                if (percent > .99)
                {
                    percent = .99;
                }

                point1.X = (int)(Point2.X + ((1 - percent) * (Point1.X - Point2.X)));
                point1.Y = (int)(Point2.Y + ((1 - percent) * (Point1.Y - Point2.Y)));

                point2.X = (int)(Point2.X + (percent * (Point1.X - Point2.X)));
                point2.Y = (int)(Point2.Y + (percent * (Point1.Y - Point2.Y)));

                Line line = new Line() //draw line
                {
                    X1 = point1.X,
                    X2 = point2.X,
                    Y1 = point1.Y,
                    Y2 = point2.Y,
                    StrokeThickness = lineWidth,
                    Stroke = brush 
                };

                ClickMarkers.Add(line);
                canvasDisplay.Children.Add(line);
            }
        }

        public void DrawPageRois() //redraws all display rois on the image
		{
            if(PageRois != null) 
            { 
                foreach(System.Drawing.Rectangle roi in PageRois)
			    {
                
                    var rectShape = new Rectangle()
				    {
                        Width = roi.Width,
                        Height= roi.Height,
				    };

                    SolidColorBrush pageROIBrush = new SolidColorBrush(Color.FromRgb((byte)PageROIColor.Red, (byte)PageROIColor.Green, (byte)PageROIColor.Blue));
                    rectShape.Stroke  = pageROIBrush;
                    canvasDisplay.Children.Add(rectShape);
                    Canvas.SetLeft(rectShape,Convert.ToDouble(roi.X));
                    Canvas.SetTop(rectShape, Convert.ToDouble(roi.Y));
                }
			}

            if(OverlapRois != null)
			{
                foreach (System.Drawing.Rectangle roi in OverlapRois)
                {

                    var rectShape = new Rectangle()
                    {
                        Width = roi.Width,
                        Height = roi.Height,
                    };

                    SolidColorBrush overlapROIBrush = new SolidColorBrush(Color.FromRgb((byte)OverlapROIColor.Red, (byte)OverlapROIColor.Green, (byte)OverlapROIColor.Blue));
                    rectShape.Stroke = overlapROIBrush;
                    canvasDisplay.Children.Add(rectShape);
                    Canvas.SetLeft(rectShape, Convert.ToDouble(roi.X));
                    Canvas.SetTop(rectShape, Convert.ToDouble(roi.Y));
                }

            }
		}

	    public void UpdatePageCanvas() //updates pageselect canvas to match main canvas
        {
            pageSelectCanvas.Children.Clear();
            ButtonRectangle.chosenPages.Clear();

            if(PageRois != null) 
            { 
                foreach (System.Drawing.Rectangle roi in PageRois)
                {
                    ButtonRectangle buttonRectangle = new ButtonRectangle() { X = roi.X, Y = roi.Y, Width = roi.Width, Height = roi.Height, Fill = Brushes.White, Stroke = Brushes.Black, StrokeThickness = 3, Opacity = .5 };
                    ButtonRectangle.chosenPages.Add(buttonRectangle);
                    pageSelectCanvas.Children.Add(buttonRectangle);
                    Canvas.SetLeft(buttonRectangle, roi.X);
                    Canvas.SetTop(buttonRectangle, roi.Y);
                }
            }
        }

		#endregion

	
	}
}
