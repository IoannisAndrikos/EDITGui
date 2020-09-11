using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Threading;
using Microsoft.Win32;


using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using Microsoft.Win32;
using OpenCvSharp.Extensions;
using System.Windows.Media.Animation;
using OpenCvSharp;
using Emgu.CV.Shape;

namespace EDITgui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            //this.DataContext = new SharedVariables();
        }

    }
}
