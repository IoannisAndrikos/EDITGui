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

namespace EDITgui
{
    /// <summary>
    /// Interaction logic for UltrasoundPart.xaml
    /// </summary>
    public partial class UltrasoundPart : UserControl
    {
        enum ContourSegmentation
        {
            INSERT_USER_POINTS,
            CORRECTION,
            MANUAL,
            FILL_POINTS
        }

        //-----------for slider----------
        Slider slider;
        public int slider_value = 0;
        int fileCount;
        //-------------------------------

        //------------user set intial points on images------
        List<Point> userPoints = new List<Point>(2);//
        int startingFrame = 0;
        int endingFrame = 0;
        //--------------------------------------------------

        ContourSegmentation contourSeg = ContourSegmentation.CORRECTION;

        List<double> pixelSpacing = new List<double>(); //x=pixelSpacing[0] y=pixelSpacing[1]
        double calibration_x;
        double calibration_y;

        List<Point> points = new List<Point>();
        List<Polyline> polylines = new List<Polyline>();
        List<List<EDITCore.CVPoint>> bladderCvPoints = new List<List<EDITCore.CVPoint>>();
        List<List<Point>> bladder = new List<List<Point>>();
        List<double> bladderArea = new List<double>();
        List<double> bladderPerimeter = new List<double>();
        string imagesDir;

        //create objects of the other classes
        Messages warningMessages = new Messages();
        coreFunctionality coreFunctionality = new coreFunctionality();
        metricsCalculations metrics = new metricsCalculations();

        public UltrasoundPart()
        {
            InitializeComponent();
            chechBox_Logger.IsChecked = true;
            coreFunctionality.setExaminationsDirectory("C:/Users/Legion Y540/Desktop/EDIT_STUDIES");
            contourSeg = ContourSegmentation.INSERT_USER_POINTS;
        }


        private async void LoadDicom_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                startSpinner();
                if (bladder != null) bladder.Clear();
                clear_canvas();
                coreFunctionality.repeatSegmentation();
                string dcmfile = openFileDialog.FileName;
                bool enablelogging = chechBox_Logger.IsChecked.Value;
                await Task.Run(() => {
                    imagesDir = coreFunctionality.exportImages(dcmfile, enablelogging);
                    pixelSpacing = coreFunctionality.getPixelSpacing();
                });
                if (imagesDir != null)
                {
                    metrics_label.Visibility = Visibility.Hidden;
                    metrics.setPixelSpacing(pixelSpacing);
                    image.Source = new BitmapImage(new Uri(imagesDir + "/0.bmp"));
                    ultrasound_studyname_label.Content = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    frame_num_label.Content = "Frame:" + " " + "0";
                    fileCount = Directory.GetFiles(imagesDir, "*.bmp", SearchOption.AllDirectories).Length;
                    slider.Value = 0;
                    slider.TickFrequency = 1 / (double)fileCount;
                    slider.Minimum = 0;
                    slider.Maximum = fileCount - 1;
                    slider.TickFrequency = 1;
                    slider.Visibility = Visibility.Visible;
                    calibration_x = image.Source.Width / canvasUltrasound.Width;
                    calibration_y = image.Source.Height / canvasUltrasound.Height;

                    //mange starting and ending frame
                    userPoints.Clear();
                    startingFrame = -1;
                    endingFrame = -1;
                    contourSeg = ContourSegmentation.INSERT_USER_POINTS;
                    stopSpinner();
                }
                else
                {
                    stopSpinner();
                    MessageBox.Show(warningMessages.cannotLoadDicom);
                }


            }
        }


        private async void Extract_bladder_Click(object sender, RoutedEventArgs e)
        {
            if (userPoints.Count < 2)
            {
                MessageBox.Show(warningMessages.notEnoughUserPoints);
                return;
            }

            startSpinner();
            int repeats = int.Parse(Repeats.Text);
            int smoothing = int.Parse(Smoothing.Text);
            double lamda1 = double.Parse(Lamda1.Text);
            double lamda2 = double.Parse(Lamda2.Text);
            int levelsetSize = int.Parse(LevelsetSize.Text);
            bool applyEqualizeHist = chechBox_FIltering.IsChecked.Value;

            //this to avoid an issue when user marks the starting frame after the ending frame
            if (endingFrame < startingFrame)
            {
                int temp = startingFrame;
                startingFrame = endingFrame;
                endingFrame = temp;
            }

                

            await Task.Run(() => {
                bladderCvPoints = coreFunctionality.Bladder2DExtraction(repeats, smoothing, lamda1, lamda2, levelsetSize, applyEqualizeHist, startingFrame, endingFrame, userPoints);
                bladder = editCVPointToWPFPoint(bladderCvPoints);
            });

            clear_canvas();
            contourSeg = ContourSegmentation.CORRECTION;
            switch_auto_manual.Content = "Correction";
            diplayMetrics();

            display();
            stopSpinner();
        }

        private void Repeat_process_Click(object sender, RoutedEventArgs e)
        {
            userPoints.Clear();
            bladder.Clear();
            bladderCvPoints.Clear();
            bladderArea.Clear();
            metrics_label.Visibility = Visibility.Hidden;
            coreFunctionality.repeatSegmentation();
            startingFrame = -1;
            endingFrame = -1;
            clear_canvas();
            contourSeg = ContourSegmentation.INSERT_USER_POINTS;

        }

        private async void Extract_STL_Click(object sender, RoutedEventArgs e)
        {
            if (!areTherePoints())
            {
                MessageBox.Show(warningMessages.noBladderSegmentation);
                return;
            }

            startSpinner();
            bladderCvPoints = WPFPointToCVPoint(bladder);
            await Task.Run(() => { coreFunctionality.extractBladderSTL(bladderCvPoints); });
            stopSpinner();
        }


        private async void Export_Points_Click(object sender, RoutedEventArgs e)
        {
            startSpinner();
            await Task.Run(() => { coreFunctionality.writePointsAndImages(); });
            stopSpinner();
        }

        private async void Export_Skin_Click(object sender, RoutedEventArgs e)
        {
            if (!areTherePoints())
            {
                MessageBox.Show(warningMessages.noBladderSegmentation);
                return;
            }
            startSpinner();
            await Task.Run(() => { coreFunctionality.extractSkin(); });
            stopSpinner();
        }


        private void Ultrasound_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                slider_value = (int)slider.Value;
                string I = imagesDir + "/" + slider_value.ToString() + ".bmp"; //path
                image.Source = new BitmapImage(new Uri(I)); 
                frame_num_label.Content = "Frame:" + " " + slider_value.ToString();
                diplayMetrics();
                clear_canvas();
                if (userPoints.Count == 2)
                {
                    doCorrection();
                }
                else
                {
                    display();
                }
            }
            catch { }
        }

        private void Ultrasound_slider_Initialized(object sender, EventArgs e)
        {
            slider = sender as Slider;
        }

        //----------------------------------------------------------CANVAS OPERATIONS--------------------------------------------------------

        private void draw_points(List<Point> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                //Color color = (Color)ColorConverter.ConvertFromString("#FF2BEDED");
                Ellipse ellipse = new Ellipse();
               // ellipse.Fill = Brushes.Cyan;
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
                    canvasUltrasound.Children.Add(pl);
                    polylines.Add(pl);
                }
            }
        }

        private void diplayMetrics(bool recalculate = false)
        {
            if (recalculate)
            {
                bladderArea[slider_value] = metrics.calulateArea(bladder[slider_value]);
                bladderPerimeter[slider_value] = metrics.calulatePerimeter(bladder[slider_value]);
            }
            try {
                if (slider_value>=startingFrame && slider_value<=endingFrame && contourSeg == ContourSegmentation.CORRECTION)
                {
                    metrics_label.Visibility = Visibility.Visible;
                    metrics_label.Content = "perimenter = " + Math.Round(bladderPerimeter[slider_value], 2) + " mm" + Environment.NewLine +
                                            "area = " + Math.Round(bladderArea[slider_value], 2) + "mm\xB2";
                  
                }
                else
                {
                    metrics_label.Visibility = Visibility.Hidden;
                }
            }
            catch(Exception e)
            {
                //TO DO
            }
        }


        private void canvasUltrasound_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!contourSeg.Equals(ContourSegmentation.CORRECTION))
            {
                Point point = e.GetPosition(image); //position relative to the image

                //Ellipse ellipse = new Ellipse();
                //ellipse.Width = 5;
                //ellipse.Height = 5;

                if (e.ChangedButton == MouseButton.Left)
                {
                    if (contourSeg == ContourSegmentation.INSERT_USER_POINTS && userPoints.Count < 2)
                    {
                        clear_canvas();
                        points.Clear();

                        if (userPoints.Count == 0)
                        {
                            canvasUltrasound.Children.Add(startingFrameMarker);
                            Canvas.SetLeft(startingFrameMarker, point.X - startingFrameMarker.Width / 2);
                            Canvas.SetTop(startingFrameMarker, point.Y - startingFrameMarker.Height / 2);
                            startingFrameMarker.Visibility = Visibility.Visible;


                            //ellipse.Fill = Brushes.Yellow;
                            userPoints.Add(point);
                            startingFrame = slider_value;
                        }
                        else if (userPoints.Count > 0 && userPoints.Count < 2)
                        {
                            // ellipse.Fill = Brushes.Red;
                            canvasUltrasound.Children.Add(endingFrameMarker);
                            Canvas.SetLeft(endingFrameMarker, point.X - endingFrameMarker.Width / 2);
                            Canvas.SetTop(endingFrameMarker, point.Y - endingFrameMarker.Height / 2);
                            endingFrameMarker.Visibility = Visibility.Visible;
                            userPoints.Add(point);
                            endingFrame = slider_value;
                        }
                        clear_canvas();
                        display();
                        //canvasUltrasound.Children.Add(ellipse);
                        //Canvas.SetLeft(ellipse, point.X);
                        //Canvas.SetTop(ellipse, point.Y);
                    }
                    else if (contourSeg == ContourSegmentation.FILL_POINTS)
                    {
                        clear_canvas();
                        // bladder[slider_value].Insert(indexA++, point);

                        if (indexA > bladder[slider_value].Count - 1) indexA = 0;

                        double d1, d2;
                        if (indexA != 0)
                        {
                            d1 = Math.Sqrt(Math.Pow(point.X - bladder[slider_value][indexA - 1].X, 2) + Math.Pow(point.Y - bladder[slider_value][indexA - 1].Y, 2));
                        }
                        else
                        {
                            int num = bladder[slider_value].Count - 1;
                            d1 = Math.Sqrt(Math.Pow(point.X - bladder[slider_value][num].X, 2) + Math.Pow(point.Y - bladder[slider_value][num].Y, 2));
                        }
                        d2 = Math.Sqrt(Math.Pow(point.X - bladder[slider_value][indexA].X, 2) + Math.Pow(point.Y - bladder[slider_value][indexA].Y, 2));

                        if (d2 > d1)
                        {
                            bladder[slider_value].Insert(indexA++, point);
                        }
                        else
                        {

                            bladder[slider_value].Insert(indexA, point);
                        }
                        display();
                       // diplayMetrics(true);
                    }
                    else if (contourSeg == ContourSegmentation.MANUAL)
                    {
                        clear_canvas();
                        bladder[slider_value].Add(point);
                        display();
                       // diplayMetrics(true);
                    }
                }
                else if (e.ChangedButton == MouseButton.Right && contourSeg == ContourSegmentation.MANUAL)
                {
                    if (bladder[slider_value].Any())
                    {
                        bladder[slider_value].RemoveAt(bladder[slider_value].Count - 1);
                      //  diplayMetrics(true);
                        if (contourSeg == ContourSegmentation.MANUAL)
                        {
                            clear_canvas();
                        }
                        else if (contourSeg == ContourSegmentation.CORRECTION)
                        {
                            Ellipse ellipse = new Ellipse();
                            ellipse.Width = 5;
                            ellipse.Height = 5;
                            canvasUltrasound.Children.Remove(ellipse);
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

        private void canvasUltrasound_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
                        canvasUltrasound.Children.Remove(ellipse);
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

        private void canvasUltrasound_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
                        int j = bladder[slider_value].IndexOf(initialPoisition1);
                        bladder[slider_value][j] = final_position;
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
        private void canvasUltrasound_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!areTherePoints() || contourSeg == ContourSegmentation.MANUAL) return;

            _start = Mouse.GetPosition(image);
            rectRemovePoints = new Rectangle();
            rectRemovePoints.Stroke = System.Windows.Media.Brushes.Red;
            rectRemovePoints.StrokeThickness = 0.5;
            Canvas.SetLeft(rectRemovePoints, _start.X);
            Canvas.SetTop(rectRemovePoints, _start.Y);
            canvasUltrasound.Children.Add(rectRemovePoints);

        }


        int indexA;
        private void canvasUltrasound_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!areTherePoints() || contourSeg == ContourSegmentation.MANUAL) return;

            List<Point> pointsToRemove = new List<Point>();
            int count = 0;
            for (int i = 0; i < bladder[slider_value].Count; i++)
            {
                if (bladder[slider_value][i].X >= Canvas.GetLeft(rectRemovePoints) && bladder[slider_value][i].Y >= Canvas.GetTop(rectRemovePoints) &&
                    bladder[slider_value][i].X <= Canvas.GetLeft(rectRemovePoints) + rectRemovePoints.Width && bladder[slider_value][i].Y <= Canvas.GetTop(rectRemovePoints) + rectRemovePoints.Height)
                {
                    pointsToRemove.Add(bladder[slider_value][i]);
                    if (count == 0) indexA = i;
                    count++;
                }

            }

            canvasUltrasound.Children.Remove(rectRemovePoints);
            if (count == bladder[slider_value].Count)
            {
                doManual();
                return;
            }
            bladder[slider_value].RemoveAll(item => pointsToRemove.Contains(item));
            if (!contourSeg.Equals(ContourSegmentation.MANUAL) && count > 0) doFillPoints();

            diplayMetrics();
        }


        private void canvasUltrasound_MouseMove(object sender, MouseEventArgs e)
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
                        if (position.Y > canvasUltrasound.Height) position.Y = canvasUltrasound.Height;
                        if (position.Y < 0) position.Y = 0;
                        if (position.X > canvasUltrasound.Width) position.X = canvasUltrasound.Width;
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
                canvasUltrasound.Children.Remove(rectRemovePoints);
            }
        }


        void clear_canvas()
        {
            canvasUltrasound.Children.Clear();
            canvasUltrasound.Children.Add(image);
        }

        void display()
        {
            if (areTherePoints())
            {
                draw_polyline(bladder[slider_value]);
                draw_points(bladder[slider_value]);
            }
            else //if there are no points mean that the user has to define starting and ending frame
            {
                if (slider_value == startingFrame) canvasUltrasound.Children.Add(startingFrameMarker);
                if (slider_value == endingFrame) canvasUltrasound.Children.Add(endingFrameMarker);
            }

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
            clear_canvas();
            display();
        }



        private void doManual()
        {
            this.switch_auto_manual.Content = "Manual";
            contourSeg = ContourSegmentation.MANUAL;
            if (areTherePoints()) bladder[slider_value].Clear();
            diplayMetrics();
            clear_canvas();
            display();
        }

        private void doFillPoints()
        {
            this.switch_auto_manual.Content = "Fill Points";
            contourSeg = ContourSegmentation.FILL_POINTS;
            clear_canvas();
            display();
        }

        bool areTherePoints()
        {
            return (bladder.Any() && bladder[slider_value].Any());
        }

        //--------------------------------------M A N A G E - P O I N T S---------------------------------

        //Convert Point to EDITCore.CVPoint
        List<List<Point>> editCVPointToWPFPoint(List<List<EDITCore.CVPoint>> cvp)
        {
            bladder = new List<List<Point>>();
            bladderArea = new List<double>();
            bladderPerimeter = new List<double>();



            List<List<Point>> points = new List<List<Point>>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                points.Add(new List<Point>(cvp[0].Count));
            }
            int count = startingFrame;
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
            for (int i = 0; i < points.Count; i++)
            {
                bladderArea.Add(metrics.calulateArea(points[i]));
                bladderPerimeter.Add(metrics.calulatePerimeter(points[i]));
            }
            return points;
        }


        //Convert EDITCore.CVPoint to Point
        List<List<EDITCore.CVPoint>> WPFPointToCVPoint(List<List<Point>> points)
        {      
            bladderCvPoints = new List<List<EDITCore.CVPoint>>();
            List<List<EDITCore.CVPoint>> cvp = new List<List<EDITCore.CVPoint>>();
            for (int i = startingFrame; i <= endingFrame; i++)
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
