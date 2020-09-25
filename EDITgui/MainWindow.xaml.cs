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
            coreFunctionality core = new coreFunctionality();
            core.setExaminationsDirectory("C:/Users/Legion Y540/Desktop/EDIT_STUDIES");


            UltrasoundPart ultrasound = new UltrasoundPart(this);
            PhotoAcousticPart photoAcoustic = new PhotoAcousticPart(this, ultrasound); //pass ultasound instance in to photoaccoustic in order to exchange data
            ultrasound.InitializeCoreFunctionality = core;
            photoAcoustic.InitializeCoreFunctionality = core;

            ultrasound.Width = 811;
            ultrasound.Margin = new Thickness(0, 0, 0, 0);
            ultrasound.HorizontalAlignment = HorizontalAlignment.Left;

            photoAcoustic.HorizontalAlignment = HorizontalAlignment.Right;
            ultrasound.Margin = new Thickness(0, 0, 0, 0);

            //ultrasound.Margin = new Thickness(10, 0, 0, 0);
            //photoAccoustic.Margin = new Thickness(786, 0, 0, 0);
            this.Rat.Children.Add(ultrasound);
            this.Rat.Children.Add(photoAcoustic);
        }

    }
}
