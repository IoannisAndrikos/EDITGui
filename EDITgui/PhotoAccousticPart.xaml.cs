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

        public delegate void zoomPhotoAccousticChangedHandler(List<Matrix> obj);
        public static event zoomPhotoAccousticChangedHandler zoomPhotoAccousticChanged = delegate { };

        UltrasoundPart ultrasound;

        //-----------for slider----------
        bool photoaccousticDicomWasLoaded = false;
        int slider_value = 0;
        int fileCount;
        //-------------------------------

        List<Point> points = new List<Point>();
        List<Polyline> polylines = new List<Polyline>();
        List<List<EDITCore.CVPoint>> thicknessCvPoints = new List<List<EDITCore.CVPoint>>();
        List<List<Point>> thickness = new List<List<Point>>();
        string imagesDir;

        Messages warningMessages = new Messages();
        coreFunctionality coreFunctionality;// = new coreFunctionality();

        public PhotoAccousticPart()
        {
            InitializeComponent();
            UltrasoundPart.sliderValueChanged += OnUltrasoundSliderValueChanged;
            UltrasoundPart.zoomUltrasoundChanged += OnUltrasoundZoomChanged;
            //chechBox_Logger.IsChecked = true;
            // coreFunctionality.setExaminationsDirectory("C:/Users/Legion Y540/Desktop/EDIT_STUDIES");
        }

        //my constructor ...I have to pass the ultrasound instance in order to get some data
        public PhotoAccousticPart(UltrasoundPart ultrasoundPart)
        {
            InitializeComponent();
            this.ultrasound = ultrasoundPart;
            UltrasoundPart.sliderValueChanged += OnUltrasoundSliderValueChanged;
            UltrasoundPart.zoomUltrasoundChanged += OnUltrasoundZoomChanged;
            //chechBox_Logger.IsChecked = true;
            // coreFunctionality.setExaminationsDirectory("C:/Users/Legion Y540/Desktop/EDIT_STUDIES");
        }

        public coreFunctionality InitializeCoreFunctionality
        {
            get { return coreFunctionality; }
            set { coreFunctionality = value; }
        }

        public void OnUltrasoundSliderValueChanged(int obj)
        {
            slider_value = (int)obj;
            if (photoaccousticDicomWasLoaded)
            {
                image.Source = new BitmapImage(new Uri(imagesDir + "/" + obj + ".bmp"));
                frame_num_label.Content = "Frame:" + " " + slider_value;
                clear_canvas();
                display();
            }
        }

        public void OnUltrasoundZoomChanged(List<Matrix> obj)
        {
            var element = canvasUltrasound;
            element.RenderTransform = new MatrixTransform(obj.LastOrDefault());
            zoom_out = obj;
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
                    imagesDir = coreFunctionality.exportPhotoAcousticImages(dcmfile, true); //enablelogging = true

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


        private async void Extract_thikness_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                thicknessCvPoints = coreFunctionality.extractThickness(ultrasound.getBladderCVPoints());
                thickness = editCVPointToWPFPoint(thicknessCvPoints);
            });
            clear_canvas();
            display();
        }

        private void Repeat_process_Click(object sender, RoutedEventArgs e)
        {


        }

        private async void Extract_STL_Click(object sender, RoutedEventArgs e)
        {

        }


        private async void Export_Points_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                coreFunctionality.writeThicknessPoints();

            });
        }

        private async void Export_Skin_Click(object sender, RoutedEventArgs e)
        {

        }


        //----------------------------------------------------------CANVAS OPERATIONS--------------------------------------------------------
        bool areTherePoints()
        {
            return (thickness.Any() && thickness[slider_value].Any());
        }

        void display()
        {
            if (areTherePoints())
            {
                draw_polyline(thickness[slider_value]);
                draw_points(thickness[slider_value]);
            }

        }


        private void draw_points(List<Point> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Ellipse ellipse = new Ellipse();
                //ellipse.Fill = Brushes.Blue;
                ellipse.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FFFF"));
                ellipse.Width = 2;
                ellipse.Height = 2;
                canvasUltrasound.Children.Add(ellipse);
                Canvas.SetLeft(ellipse, points.ElementAt(i).X);
                Canvas.SetTop(ellipse, points.ElementAt(i).Y);
            }

        }

        private void draw_polyline(List<Point> points)
        {
            List<Point> closeCurvePoints = points.ToList();//new List<Point>();
            closeCurvePoints.Add(points[0]);
            for (int i = 0; i < closeCurvePoints.Count - 1; i++)
            {
                    Polyline pl = new Polyline();
                    pl.FillRule = FillRule.EvenOdd;
                    pl.StrokeThickness = 0.5;
                    pl.Points.Add(closeCurvePoints.ElementAt(i));
                    pl.Points.Add(closeCurvePoints.ElementAt(i + 1));
                    //pl.Stroke = System.Windows.Media.Brushes.Green;
                    pl.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3FF00"));
                    pl.StrokeStartLineCap = PenLineCap.Round;
                    pl.StrokeEndLineCap = PenLineCap.Round;
                    canvasUltrasound.Children.Add(pl);
                    polylines.Add(pl);
            }
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
            canvasUltrasound.Children.Clear();
            canvasUltrasound.Children.Add(image);
        }


        protected List<Matrix> zoom_out = new List<Matrix>();
        private void canvasUltrasound_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var element = sender as UIElement;
            var position = e.GetPosition(element);
            var transform = element.RenderTransform as MatrixTransform;
            var matrix = transform.Matrix;
            var scale = e.Delta > 0 ? 1.1 : (1.0 / 1.1); // choose appropriate scaling factor

            if (e.Delta < 0) zoom_out.Remove(zoom_out.LastOrDefault());

            if ((matrix.M11 >= 1 && e.Delta > 0))//((matrix.M11 >= 1 && cout_wh == 0) || (matrix.M11 >= 1.1 && cout_wh > 0))
            {
                matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);
                element.RenderTransform = new MatrixTransform(matrix);
                zoom_out.Add(matrix);
            }
            else if ((matrix.M11 >= 1.1 && e.Delta < 0))
            {
               // matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);
                element.RenderTransform = new MatrixTransform(zoom_out.LastOrDefault());
            }
            zoomPhotoAccousticChanged(zoom_out);
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



        //--------------------------------------M A N A G E - P O I N T S---------------------------------

        //Convert Point to EDITCore.CVPoint
        List<List<Point>> editCVPointToWPFPoint(List<List<EDITCore.CVPoint>> cvp)
        {
            thickness = new List<List<Point>>();
            //bladderArea = new List<double>();
            //bladderPerimeter = new List<double>();

            List<List<Point>> points = new List<List<Point>>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                points.Add(new List<Point>(cvp[0].Count));
            }
            int count = ultrasound.getStartingFrame();
            for (int i = 0; i < cvp.Count; i++)
            {
                List<Point> contour = new List<Point>();
                for (int j = 0; j < cvp[i].Count(); j++)
                {
                    contour.Add(new Point(cvp[i][j].X, cvp[i][j].Y)); // * (1 / calibration_x)
                }
                points[count++] = contour;
            }

            //after extracting the 2D bladder segmentation we calculate and some metrics
            //for (int i = 0; i < points.Count; i++)
            //{
            //    bladderArea.Add(metrics.calulateArea(points[i]));
            //    bladderPerimeter.Add(metrics.calulatePerimeter(points[i]));
            //}
            return points;
        }


        //Convert EDITCore.CVPoint to Point
        List<List<EDITCore.CVPoint>> WPFPointToCVPoint(List<List<Point>> points)
        {
            thicknessCvPoints = new List<List<EDITCore.CVPoint>>();
            List<List<EDITCore.CVPoint>> cvp = new List<List<EDITCore.CVPoint>>();
            for (int i = ultrasound.getStartingFrame(); i <= ultrasound.getEndingFrame(); i++)
            {
                List<EDITCore.CVPoint> contour = new List<EDITCore.CVPoint>();
                for (int j = 0; j < points[i].Count(); j++)
                {
                    contour.Add(new EDITCore.CVPoint(points[i][j].X, points[i][j].Y));
                }
                cvp.Add(contour);
            }
            return cvp;
        }

    }

        
}
