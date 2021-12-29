using System;
using System.Collections.Generic;
using System.Text;

namespace Subdivider.Struct
{
    public struct PaperSize
    {
        public double Width { get;}
        public double Height { get;}
        public PaperSize(double width, double height) 
        {
            Width = width;
            Height = height;
        }
    }

    public static class Papers
    {
        public static readonly Dictionary<string, PaperSize> Sizes =  new Dictionary<string, PaperSize>() //sets paper size dictionary
        {
            {"USLetter", new PaperSize(8.5,11) },
            {"USLegal", new PaperSize(11,17) },
            {"A5", new PaperSize(5.875,8.25) },
            {"A4", new PaperSize(8.25,11.75) }
        };
    }
}
