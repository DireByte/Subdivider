using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Subdivider.CustomControls.UIElements
{
    public struct LineColor: INotifyPropertyChanged
    {

        private int red;
        private int blue;
        private int green;

        public int Red //red property
        { 
            get { return this.red; }
            set
            {
                this.red = value;
                OnPropertyChanged();
            }
        } 

        public int Green //green property
        { 
            get{return this.green;}
            set
            {
                this.blue = value;
                OnPropertyChanged();
            }
        }
        public int Blue //blue property
        {
            get { return this.blue; }
            set
            {
                this.blue = value;
                OnPropertyChanged();
            }
        }

        public LineColor(int red, int green, int blue) : this() //sets lines color
        {
            this.red = red;
            this.blue = blue;
            this.green = green;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null) //calls update if line color changes
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void Update() //updates color
		{
            Red = this.red;
            Green = this.green;
            Blue = this.blue;
		}

    }

        public static class Lines //sets line color dictionary
        {
            public static readonly Dictionary<string, LineColor> Color = new Dictionary<string, LineColor>()
            {
                {"Red", new LineColor(255, 0, 0) },
                {"Orange", new LineColor(255, 165, 0) },
                {"Yellow", new LineColor(255, 255, 0) },
                {"Green", new LineColor(0, 128, 0) },
                {"Blue", new LineColor(0, 0, 255) },
                {"Violet", new LineColor(75, 0, 130) },
                {"Pink", new LineColor(238, 130, 238) }
            };

    }

    
}
