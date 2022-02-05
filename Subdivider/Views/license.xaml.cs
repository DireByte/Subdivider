using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
            licenseText.Text = "Error loading license resource";

            var currentAssembly = Assembly.GetExecutingAssembly();

            string[] names = currentAssembly.GetManifestResourceNames();
            using (var stream = currentAssembly.GetManifestResourceStream("Subdivider.Resources.license.txt"))
            using (var reader = new StreamReader(stream))
            {
                // TODO: read the stream here
                licenseText.Text = reader.ReadToEnd();

            }
          
        }

    }
}
