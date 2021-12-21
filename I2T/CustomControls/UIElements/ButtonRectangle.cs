using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Subdivider.CustomControls.UIElements
{
    class ButtonRectangle : Shape
    {
        private bool _clicked = false;
        public static List<ButtonRectangle> chosenPages = new List<ButtonRectangle>();

        public double X { get; set; }
        public double Y { get; set; }
        
        public bool Clicked  //changes bool value
        {
            get {return _clicked; }
            set { _clicked = value; }
        }


        protected override Geometry DefiningGeometry //creating a rectangle
        {
            get
            {
                Rect rect = new Rect(0, 0, Width, Height);

                RectangleGeometry rectangleGeometry = new RectangleGeometry(rect);

                return rectangleGeometry;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Clicked = !Clicked;
            changeColor();
        }

        protected override void OnGotMouseCapture(MouseEventArgs e)
        {
            base.OnGotMouseCapture(e);
            Clicked = !Clicked;
            changeColor();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            changeEnterColor();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            changeLeaveColor();
        }

        public void changeColor() //change fill color depending on bool
        {
            if(Clicked == true)
            {
                Fill = Brushes.Transparent;
            }
            else
            {
                Fill = Brushes.White;
            }
        }

        public void changeEnterColor() //changes enter color depending on bool
        {
            if (Clicked == true)
            {
                Fill = Brushes.SlateGray;
            }
            else
            {
                Fill = Brushes.DarkGray;
            }
        }


        public void changeLeaveColor() //changes leave color depending on bool 
        {
            if (Clicked == true)
            {
                Fill = Brushes.Transparent;
            }
            else
            {
                Fill = Brushes.White;
            }
        }

        public static bool checkSelected() //checks all rectangles that are clicked
        {
            bool selected = false;

            foreach (var page in chosenPages)
            {
                if (page.Clicked == true)
                {
                    selected = true;
                }
            }

            return selected;
        }
    }
}
