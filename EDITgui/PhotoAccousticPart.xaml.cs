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
using System.ComponentModel;

namespace EDITgui
{
    /// <summary>
    /// Interaction logic for UltrasoundPart.xaml
    /// </summary>
    public partial class PhotoAccousticPart : UserControl
    {

        //-----------for slider----------
        bool photoaccousticDicomWasLoaded = false;
        int slider_value = 0;
        int fileCount;
        //-------------------------------


        string imagesDir;

        Messages warningMessages = new Messages();
        coreFunctionality coreFunctionality = new coreFunctionality();

        public PhotoAccousticPart()
        {
            InitializeComponent();
            UltrasoundPart.sliderValueChanged += OnUltrasoundSliderValueChanged;
            //chechBox_Logger.IsChecked = true;
            coreFunctionality.setExaminationsDirectory("C:/Users/Legion Y540/Desktop/EDIT_STUDIES");
        }


        public void OnUltrasoundSliderValueChanged(int obj)
        {
            slider_value = (int)obj;
            if (photoaccousticDicomWasLoaded)
            {
                image.Source = new BitmapImage(new Uri(imagesDir + "/" + obj + ".bmp"));
                frame_num_label.Content = "Frame:" + " " + slider_value;
            }
        }


        private async void LoadDicom_Click(object sender, RoutedEventArgs e)
        {
            photoaccousticDicomWasLoaded = false;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                startSpinner();

                clear_canvas();
                coreFunctionality.repeatSegmentation();
                string dcmfile = openFileDialog.FileName;

                await Task.Run(() =>
                {
                    imagesDir = coreFunctionality.exportImages(dcmfile, true); //enablelogging = true

                });
                if (imagesDir != null)
                {
                    photoaccousticDicomWasLoaded = true;
                    image.Source = new BitmapImage(new Uri(imagesDir + "/" + slider_value + ".bmp")); //imagesDir + "/0.bmp"
                    ultrasound_studyname_label.Content = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    frame_num_label.Content = "Frame:" + " " + slider_value;
                    fileCount = Directory.GetFiles(imagesDir, "*.bmp", SearchOption.AllDirectories).Length;
                    stopSpinner();
                }
                else
                {
                    stopSpinner();
                    MessageBox.Show(warningMessages.cannotLoadDicom);
                }


            }
        }


        private void Extract_thikness_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Repeat_process_Click(object sender, RoutedEventArgs e)
        {


        }

        private async void Extract_STL_Click(object sender, RoutedEventArgs e)
        {

        }


        private async void Export_Points_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void Export_Skin_Click(object sender, RoutedEventArgs e)
        {

        }


        //----------------------------------------------------------CANVAS OPERATIONS--------------------------------------------------------

        private void draw_points(List<Point> points)
        {


        }

        private void draw_polyline(List<Point> points)
        {

        }

        private void canvasUltrasound_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }



        private void canvasUltrasound_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {


        }

        private void canvasUltrasound_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }


        private void canvasUltrasound_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {


        }



        private void canvasUltrasound_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

        }


        private void canvasUltrasound_MouseMove(object sender, MouseEventArgs e)
        {


        }

        private void ApplicationGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

        }


        void clear_canvas()
        {

        }

        void display()
        {


        }

        protected List<Matrix> zoom_out = new List<Matrix>();
        private void canvasUltrasound_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var element = sender as UIElement;
            var position = e.GetPosition(element);
            var transform = element.RenderTransform as MatrixTransform;
            var matrix = transform.Matrix;
            var scale = e.Delta > 0 ? 1.1 : (1.0 / 1.1); // choose appropriate scaling factor

            //Console.WriteLine(transform);
            //Console.WriteLine(e.Delta);

            if (e.Delta > 0)
            {
                zoom_out.Add(matrix);
            }

            if ((matrix.M11 >= 1 && e.Delta > 0))//((matrix.M11 >= 1 && cout_wh == 0) || (matrix.M11 >= 1.1 && cout_wh > 0))
            {
                matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);
                element.RenderTransform = new MatrixTransform(matrix);
            }
            else if ((matrix.M11 >= 1.1 && e.Delta < 0))
            {
                matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);
                element.RenderTransform = new MatrixTransform(zoom_out.LastOrDefault());
                zoom_out.RemoveAt(zoom_out.Count - 1);
            }
        }

        private void Switch_auto_manual_Click(object sender, RoutedEventArgs e)
        {

        }

        private void startSpinner()
        {
            ((Storyboard)FindResource("WaitStoryboard")).Begin();
            applicationGrid.IsEnabled = false;
            Wait.Visibility = Visibility.Visible;
        }

        private void stopSpinner()
        {
            ((Storyboard)FindResource("WaitStoryboard")).Stop();
            applicationGrid.IsEnabled = true;
            Wait.Visibility = Visibility.Hidden;
        }

        private void doCorrection()
        {

        }



        private void doManual()
        {

        }

        private void doFillPoints()
        {

        }      

    }

        
}
