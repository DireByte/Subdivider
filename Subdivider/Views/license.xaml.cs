using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Subdivider
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class License : Window
    {
        public License()
        {
            InitializeComponent();
            licenseText.Text = "";

            string path = Directory.GetCurrentDirectory();

            path = path + "\\Resources\\license.txt";



            // This text is added only once to the file.
            if (!File.Exists(path))
            {
                // Create a file to write to.
                licenseText.Text = "Could not find license file!";
            }

            // Open the file to read from.
            string readText = File.ReadAllText(path);

            licenseText.Text = readText;
          
        }

    }
}
