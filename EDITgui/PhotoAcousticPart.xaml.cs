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
    public partial class PhotoAcousticPart : UserControl
    {

        public delegate void zoomPhotoAccousticChangedHandler(List<Matrix> obj);
        public static event zoomPhotoAccousticChangedHandler zoomPhotoAccousticChanged = delegate { };

        UltrasoundPart ultrasound;

        enum ContourSegmentation
        {
            CORRECTION,
            MANUAL,
            FILL_POINTS
        }


        //-----------for slider----------
        bool photoaccousticDicomWasLoaded = false;
        int slider_value = 0;
        int fileCount;
        //-------------------------------


        ContourSegmentation contourSeg = ContourSegmentation.CORRECTION;

        List<double> pixelSpacing = new List<double>(); //x=pixelSpacing[0] y=pixelSpacing[1]
        double calibration_x;
        double calibration_y;


        List<Point> points = new List<Point>();
        List<Polyline> polylines = new List<Polyline>();
        List<List<EDITCore.CVPoint>> thicknessCvPoints = new List<List<EDITCore.CVPoint>>();
        List<List<Point>> thickness = new List<List<Point>>();
        List<EDITCore.CVPoint> contourForFix = new List<EDITCore.CVPoint>();
        List<double> meanThickness = new List<double>();
        List<List<Point>> bladderUltrasound = new List<List<Point>>();
        string imagesDir;

        Messages warningMessages = new Messages();
        coreFunctionality coreFunctionality;// = new coreFunctionality();

        public PhotoAcousticPart()
        {
            InitializeComponent();
            UltrasoundPart.sliderValueChanged += OnUltrasoundSliderValueChanged;
            UltrasoundPart.zoomUltrasoundChanged += OnUltrasoundZoomChanged;
            //chechBox_Logger.IsChecked = true;
            // coreFunctionality.setExaminationsDirectory("C:/Users/Legion Y540/Desktop/EDIT_STUDIES");
        }

        //my constructor ...I have to pass the ultrasound instance in order to get some data
        public PhotoAcousticPart(UltrasoundPart ultrasoundPart)
        {
            InitializeComponent();
            this.ultrasound = ultrasoundPart;
            UltrasoundPart.sliderValueChanged += OnUltrasoundSliderValueChanged;
            UltrasoundPart.zoomUltrasoundChanged += OnUltrasoundZoomChanged;
            UltrasoundPart.bladderPointChanged += OnBladderPointChanged;
            UltrasoundPart.repeatProcess += OnrepeatProcess;
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
                doCorrection();
                clear_canvas();
                display();
            }
        }

        public void OnUltrasoundZoomChanged(List<Matrix> obj)
        {
            var element = canvasPhotoAcoustic;
            element.RenderTransform = new MatrixTransform(obj.LastOrDefault());
            zoom_out = obj;
        }

        public void OnBladderPointChanged(List<List<Point>> newBladder)
        {
            if (!areTherePoints()) return;
            
            bladderUltrasound = newBladder;
            clear_canvas();
            display();
        }

        public void OnrepeatProcess()
        {
            thickness.Clear();
            thicknessCvPoints.Clear();
            bladderUltrasound.Clear();
            meanThickness.Clear();
            metrics_label.Visibility = Visibility.Hidden;
            contourSeg = ContourSegmentation.CORRECTION;
            clear_canvas();
            display();
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
                    photoAcoustic_studyname_label.Content = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
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
                fillmeanThicknessList(coreFunctionality.meanThickness);
            });
            bladderUltrasound = ultrasound.getBladderPoints();
            diplayMetrics();
            clear_canvas();
            display();
        }

        private async void Recalculate_Click(object sender, RoutedEventArgs e)
        {
            bladderUltrasound = ultrasound.getBladderPoints();
            await Task.Run(() =>
            {
                contourForFix = coreFunctionality.recalculateThicknessOfContour(slider_value, WPFPointToCVPoint(bladderUltrasound[slider_value]));
                thickness[slider_value].AddRange(editCVPointToWPFPoint(contourForFix));
                //contourForFix.Clear();
                meanThickness.AddRange(coreFunctionality.meanThickness);
            });
            Console.WriteLine("contour size: " + contourForFix.Count);
            clear_canvas();
            display();
        }

        private async void Extract_STL_Click(object sender, RoutedEventArgs e)
        {
            if (!areTherePoints())
            {
                MessageBox.Show(warningMessages.noBladderSegmentation);
                return;
            }

            startSpinner();
            thicknessCvPoints = WPFPointToCVPoint(thickness);
            await Task.Run(() => { coreFunctionality.extractThicknessSTL(thicknessCvPoints); });
            stopSpinner();
        }


        private async void Export_Points_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                coreFunctionality.writeThicknessPoints();

            });
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
            if (ultrasound.areTherePoints() && bladderUltrasound.Any())
            {
                draw_polyline_ultrasound(bladderUltrasound[slider_value]);
                //draw_points_ultrasound(bladderUltrasound[slider_value]);
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
                canvasPhotoAcoustic.Children.Add(ellipse);
                Canvas.SetLeft(ellipse, points.ElementAt(i).X);
                Canvas.SetTop(ellipse, points.ElementAt(i).Y);
            }

        }

        private void draw_points_ultrasound(List<Point> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Fill = Brushes.Pink;
                //ellipse.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FFFF"));
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

            switch (contourSeg)
            {
                case ContourSegmentation.CORRECTION:
                    closeCurvePoints.Add(points[0]);
                    break;
                case ContourSegmentation.FILL_POINTS:
                    if (indexA != 0)
                    {
                        closeCurvePoints.Add(points[0]);
                    }
                    break;
            }

            for (int i = 0; i < closeCurvePoints.Count - 1; i++)
            {
                if (contourSeg != ContourSegmentation.FILL_POINTS || i != indexA - 1)
                {
                    Polyline pl = new Polyline();
                    pl.FillRule = FillRule.EvenOdd;
                    pl.StrokeThickness = 0.5;
                    pl.Points.Add(closeCurvePoints.ElementAt(i));
                    pl.Points.Add(closeCurvePoints.ElementAt(i + 1));
                    //pl.Stroke = System.Windows.Media.Brushes.Yellow;
                    pl.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3FF00"));
                    pl.StrokeStartLineCap = PenLineCap.Round;
                    pl.StrokeEndLineCap = PenLineCap.Round;
                    canvasPhotoAcoustic.Children.Add(pl);
                    polylines.Add(pl);
                }
            }
        }

        private void draw_polyline_ultrasound(List<Point> points)
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
                pl.Stroke = System.Windows.Media.Brushes.Silver;
                //pl.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3FF00"));
                pl.StrokeStartLineCap = PenLineCap.Round;
                pl.StrokeEndLineCap = PenLineCap.Round;
                canvasPhotoAcoustic.Children.Add(pl);
                polylines.Add(pl);
            }
        }

        private void diplayMetrics(bool recalculate = false)
        {
            //if (recalculate)
            //{
            //    bladderArea[slider_value] = metrics.calulateArea(bladder[slider_value]);
            //    bladderPerimeter[slider_value] = metrics.calulatePerimeter(bladder[slider_value]);
            //}
            try
            {
                if (slider_value >= ultrasound.getStartingFrame() && slider_value <= ultrasound.getEndingFrame() && contourSeg == ContourSegmentation.CORRECTION)
                {
                    metrics_label.Visibility = Visibility.Visible;
                    metrics_label.Content = "mean thickness = " + Math.Round(meanThickness[slider_value], 2) + " mm";                    
                }
                else
                {
                    metrics_label.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception e)
            {
                //TO DO
            }
        }




        private void canvasPhotoAcoustic_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!contourSeg.Equals(ContourSegmentation.CORRECTION))
            {
                Point point = e.GetPosition(image); //position relative to the image

                //Ellipse ellipse = new Ellipse();
                //ellipse.Width = 5;
                //ellipse.Height = 5;

                if (e.ChangedButton == MouseButton.Left)
                {
                    if (contourSeg == ContourSegmentation.FILL_POINTS)
                    {
                        clear_canvas();
                        // bladder[slider_value].Insert(indexA++, point);

                        if (indexA > thickness[slider_value].Count - 1) indexA = 0;

                        double d1, d2;
                        if (indexA != 0)
                        {
                            d1 = Math.Sqrt(Math.Pow(point.X - thickness[slider_value][indexA - 1].X, 2) + Math.Pow(point.Y - thickness[slider_value][indexA - 1].Y, 2));
                        }
                        else
                        {
                            int num = thickness[slider_value].Count - 1;
                            d1 = Math.Sqrt(Math.Pow(point.X - thickness[slider_value][num].X, 2) + Math.Pow(point.Y - thickness[slider_value][num].Y, 2));
                        }
                        d2 = Math.Sqrt(Math.Pow(point.X - thickness[slider_value][indexA].X, 2) + Math.Pow(point.Y - thickness[slider_value][indexA].Y, 2));

                        if (d2 > d1)
                        {
                            thickness[slider_value].Insert(indexA++, point);
                        }
                        else
                        {

                            thickness[slider_value].Insert(indexA, point);
                        }
                        display();
                         diplayMetrics(true);
                    }else if (contourSeg == ContourSegmentation.MANUAL)
                    {
                        clear_canvas();
                        thickness[slider_value].Add(point);
                        display();
                        diplayMetrics(true);
                    }
                }
                else if (e.ChangedButton == MouseButton.Right && contourSeg == ContourSegmentation.MANUAL)
                {
                    if (thickness[slider_value].Any())
                    {
                        thickness[slider_value].RemoveAt(thickness[slider_value].Count - 1);
                        diplayMetrics(true);
                        if (contourSeg == ContourSegmentation.MANUAL)
                        {
                            clear_canvas();
                        }
                        else if (contourSeg == ContourSegmentation.CORRECTION)
                        {
                            Ellipse ellipse = new Ellipse();
                            ellipse.Width = 5;
                            ellipse.Height = 5;
                            canvasPhotoAcoustic.Children.Remove(ellipse);
                        }
                        display();
                    }
                }
            }
        }


        bool isPressed1 = false;
        Point initialPoisition1;
        Point startPosition1;
        int a = 0, b = 1;

        private void canvasPhotoAcoustic_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (contourSeg.Equals(ContourSegmentation.CORRECTION))
            {
                if (e.OriginalSource is Ellipse)
                {
                    Ellipse ellipse = (Ellipse)e.OriginalSource;
                    initialPoisition1.Y = (double)ellipse.GetValue(Canvas.TopProperty);//ellipse.GetValue(Canvas.TopProperty);
                    initialPoisition1.X = (double)ellipse.GetValue(Canvas.LeftProperty);

                    //find the polylines which contain the pressed point
                    for (int i = 0; i < polylines.Count; i++)
                    {
                        if (polylines[i].Points[1].Equals(initialPoisition1)) a = i;

                        if (polylines[i].Points[0].Equals(initialPoisition1)) b = i;
                    }
                    ellipse.Opacity = 0.5;
                    if (e.ClickCount == 2)
                    {
                        canvasPhotoAcoustic.Children.Remove(ellipse);
                    }
                    else
                    {
                        isPressed1 = true;
                        startPosition1 = e.GetPosition(image);
                        ellipse.CaptureMouse();
                    }
                }
            }

        }

        private void canvasPhotoAcoustic_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (contourSeg.Equals(ContourSegmentation.CORRECTION))
            {
                if (e.OriginalSource is Ellipse)
                {
                    Ellipse ClickedRectangle = (Ellipse)e.OriginalSource;
                    ClickedRectangle.Opacity = 1;
                    ClickedRectangle.ReleaseMouseCapture();
                    isPressed1 = false;

                    Point final_position = e.GetPosition(image);

                    //draw point only into canvas 
                    if (final_position.Y > image.Height) final_position.Y = image.Height;
                    if (final_position.Y < 0) final_position.Y = 0;
                    if (final_position.X > image.Width) final_position.X = image.Width;
                    if (final_position.X < 0) final_position.X = 0;

                    //the borders have to be updated in the same try catch block, in order to manage any potential points overlapping

                    try
                    {
                        int j = thickness[slider_value].IndexOf(initialPoisition1);
                        thickness[slider_value][j] = final_position;
                        diplayMetrics(true);
                        //update_centerline();
                    }
                    catch { }

                    clear_canvas();
                    display();
                }
            }
        }

        Point _start;
        Rectangle rectRemovePoints;
        private void canvasPhotoAcoustic_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!areTherePoints() || contourSeg == ContourSegmentation.MANUAL) return;

            _start = Mouse.GetPosition(image);
            rectRemovePoints = new Rectangle();
            rectRemovePoints.Stroke = System.Windows.Media.Brushes.Red;
            rectRemovePoints.StrokeThickness = 0.5;
            Canvas.SetLeft(rectRemovePoints, _start.X);
            Canvas.SetTop(rectRemovePoints, _start.Y);
            canvasPhotoAcoustic.Children.Add(rectRemovePoints);

        }


        int indexA;
        private void canvasPhotoAcoustic_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!areTherePoints() || contourSeg == ContourSegmentation.MANUAL) return;

            List<Point> pointsToRemove = new List<Point>();
            int count = 0;
            for (int i = 0; i < thickness[slider_value].Count; i++)
            {
                if (thickness[slider_value][i].X >= Canvas.GetLeft(rectRemovePoints) && thickness[slider_value][i].Y >= Canvas.GetTop(rectRemovePoints) &&
                    thickness[slider_value][i].X <= Canvas.GetLeft(rectRemovePoints) + rectRemovePoints.Width && thickness[slider_value][i].Y <= Canvas.GetTop(rectRemovePoints) + rectRemovePoints.Height)
                {
                    pointsToRemove.Add(thickness[slider_value][i]);
                    if (count == 0) indexA = i;
                    count++;
                }

            }

            canvasPhotoAcoustic.Children.Remove(rectRemovePoints);
            if (count == thickness[slider_value].Count)
            {
                doManual();
               diplayMetrics(true);
                return;
            }
            thickness[slider_value].RemoveAll(item => pointsToRemove.Contains(item));
            if (!contourSeg.Equals(ContourSegmentation.MANUAL) && count > 0) doFillPoints();

            diplayMetrics();
        }


        private void canvasPhotoAcoustic_MouseMove(object sender, MouseEventArgs e)
        {
            if (!areTherePoints() || contourSeg.Equals(ContourSegmentation.MANUAL)) return;

            if (e.RightButton == MouseButtonState.Pressed)
            {
                var mouse = Mouse.GetPosition(image);
                Canvas.SetLeft(rectRemovePoints, _start.X > mouse.X ? mouse.X : _start.X);
                Canvas.SetTop(rectRemovePoints, _start.Y > mouse.Y ? mouse.Y : _start.Y);

                rectRemovePoints.Width = Math.Abs(mouse.X - _start.X);
                rectRemovePoints.Height = Math.Abs(mouse.Y - _start.Y);
            }


            if (e.LeftButton == MouseButtonState.Pressed && contourSeg.Equals(ContourSegmentation.CORRECTION)) ///--------------> (enableCorrection)
            {
                if (e.OriginalSource is Ellipse)
                {
                    Ellipse ClickedRectangle = (Ellipse)e.OriginalSource;
                    if (isPressed1)
                    {
                        Point position = e.GetPosition(image);

                        //some code to grag points only into canvas-image area
                        if (position.Y > canvasPhotoAcoustic.Height) position.Y = canvasPhotoAcoustic.Height;
                        if (position.Y < 0) position.Y = 0;
                        if (position.X > canvasPhotoAcoustic.Width) position.X = canvasPhotoAcoustic.Width;
                        if (position.X < 0) position.X = 0;

                        double daltaY = position.Y - startPosition1.Y;
                        double daltaX = position.X - startPosition1.X;
                        ClickedRectangle.SetValue(Canvas.TopProperty, (double)ClickedRectangle.GetValue(Canvas.TopProperty) + daltaY);
                        ClickedRectangle.SetValue(Canvas.LeftProperty, (double)ClickedRectangle.GetValue(Canvas.LeftProperty) + daltaX);

                        //manage move of polylines
                        polylines[a].Points[1] = e.GetPosition(image);
                        polylines[b].Points[0] = e.GetPosition(image);

                        startPosition1 = position;
                    }
                }
            }
        }

        private void ApplicationGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (rectRemovePoints != null && rectRemovePoints.GetType() == typeof(Rectangle))
            {
                canvasPhotoAcoustic.Children.Remove(rectRemovePoints);
            }
        }


        void clear_canvas()
        {
            canvasUltrasound.Children.Clear();
            canvasPhotoAcoustic.Children.Clear();
            canvasPhotoAcoustic.Children.Add(image);
            canvasPhotoAcoustic.Children.Add(canvasUltrasound);
        }


        protected List<Matrix> zoom_out = new List<Matrix>();
        private void canvasPhotoAcoustic_MouseWheel(object sender, MouseWheelEventArgs e)
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
            if (this.switch_auto_manual.Content.Equals("Manual"))
            {
                doCorrection();
                diplayMetrics(true);
            }
            else if (this.switch_auto_manual.Content.Equals("Correction"))
            {
                doManual();
            }
            else if (this.switch_auto_manual.Content.Equals("Fill Points"))
            {
                doCorrection();
               diplayMetrics(true);
            }
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
            this.switch_auto_manual.Content = "Correction";
            contourSeg = ContourSegmentation.CORRECTION;
            diplayMetrics();
            clear_canvas();
            display();
        }



        private void doManual()
        {
            this.switch_auto_manual.Content = "Manual";
            contourSeg = ContourSegmentation.MANUAL;
            if (areTherePoints()) thickness[slider_value].Clear();
            diplayMetrics();
            clear_canvas();
            display();
        }

        private void doFillPoints()
        {
            this.switch_auto_manual.Content = "Fill Points";
            contourSeg = ContourSegmentation.FILL_POINTS;
            diplayMetrics();
            clear_canvas();
            display();
        }



        //--------------------------------------M A N A G E - P O I N T S---------------------------------

        //Convert Point to EDITCore.CVPoint //for many contours
        List<List<Point>> editCVPointToWPFPoint(List<List<EDITCore.CVPoint>> cvp)
        {
            thickness = new List<List<Point>>();
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

        //----------------ONLY ONE CONTOUR----------------
        List<Point> editCVPointToWPFPoint(List<EDITCore.CVPoint> cvp)
        {
            thickness[slider_value].Clear();
            for (int i = 0; i < cvp.Count; i++)
            {
                thickness[slider_value].Add(new Point(cvp[i].X, cvp[i].Y)); // * (1 / calibration_x)
            }
            return points;
        }

        //Convert EDITCore.CVPoint to Point
        List<EDITCore.CVPoint> WPFPointToCVPoint(List<Point> points)
        {
            List<EDITCore.CVPoint> cvp = new List<EDITCore.CVPoint>();
            for (int i = 0; i < points.Count; i++)
            {
                cvp.Add(new EDITCore.CVPoint(points[i].X, points[i].Y));
           
            }
            return cvp;
        }

        //-----------------------------------------------


        void fillmeanThicknessList(List<double> values)
        {
            meanThickness.Clear();
            meanThickness = new List<double>();

            for(int i=0; i<fileCount; i++)
            {
                meanThickness.Add(0.0);
            }
            int start = ultrasound.getStartingFrame();
            int end = ultrasound.getEndingFrame();
            for (int i=start; i<=end; i++)
            {
                Console.WriteLine(i);
                Console.WriteLine(start);
                Console.WriteLine(values.Count);
                meanThickness[i] = values[i - start];
            }

        }

    }

        
}
