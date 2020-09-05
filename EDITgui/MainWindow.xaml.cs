﻿using System;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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
        int slider_value = 0;
        int fileCount;
        //-------------------------------

        //------------user set intial points on images------
        List<Point> userPoints = new List<Point>(2);//
        int startingFrame = 0;
        int endingFrame = 0;
        //--------------------------------------------------

        //------------diplay aspects---------
        bool closeCurve = true;
        //-----------------------------------


        ContourSegmentation contourSeg = ContourSegmentation.CORRECTION;


        double calibration_x;
        double calibration_y; 

        List<Point> points = new List<Point>();
        List<Polyline> polylines = new List<Polyline>();
        List<List<EDITCore.CVPoint>> bladderCvPoints = new List<List<EDITCore.CVPoint>>();
        List<List<Point>> bladder = new List<List<Point>>();
        string imagesDir;

        coreFunctionality coreFunctionality = new coreFunctionality();

        public MainWindow()
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
                await Task.Run(() => { imagesDir = coreFunctionality.exportImages(dcmfile, enablelogging); });
                if (imagesDir != null)
                {
                    image.Source = new BitmapImage(new Uri(imagesDir + "/0.bmp"));
                    ultrasound_studyname_label.Content = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    frame_num_label.Content = "Frame:" + " " + "0";
                    fileCount = Directory.GetFiles(imagesDir, "*.bmp", SearchOption.AllDirectories).Length;
                    slider.TickFrequency = 1 / (double)fileCount;
                    slider.Minimum = 0;
                    slider.Maximum = fileCount - 1;
                    slider.TickFrequency = 1;
                    slider.Visibility = Visibility.Visible;
                    calibration_x = image.Source.Width / canvas1.Width;
                    calibration_y = image.Source.Height / canvas1.Height;
                    stopSpinner();
                }
                else
                {
                    stopSpinner();
                    MessageBox.Show("Cannot load the selected DICOM file");
                }
                
               
            }
        }


        private async void Extract_bladder_Click(object sender, RoutedEventArgs e)
        {
            startSpinner();

            int repeats = int.Parse(Repeats.Text);
            int smoothing = int.Parse(Smoothing.Text);
            double lamda1 = double.Parse(Lamda1.Text);
            double lamda2 = double.Parse(Lamda2.Text);
            int levelsetSize = int.Parse(LevelsetSize.Text);
            bool applyEqualizeHist = chechBox_FIltering.IsChecked.Value;

            // Bladder2DExtraction(repeats, smoothing, lamda1, lamda2, levelsetSize, applyEqualizeHist)
            await Task.Run(() => {
                bladderCvPoints = coreFunctionality.Bladder2DExtraction(repeats, smoothing, lamda1, lamda2, levelsetSize, applyEqualizeHist, startingFrame, endingFrame, userPoints);
                bladder = editCVPointToWPFPoint(bladderCvPoints);
            });

            clear_canvas();
            contourSeg = ContourSegmentation.CORRECTION;
            switch_auto_manual.Content = "Correction";
        
            display();
            stopSpinner();
        }

        private void Repeat_process_Click(object sender, RoutedEventArgs e)
        {
            userPoints.Clear();
            bladder.Clear();
            bladderCvPoints.Clear();
            coreFunctionality.repeatSegmentation();
            startingFrame = 0;
            endingFrame = 0;
            clear_canvas();
            contourSeg = ContourSegmentation.INSERT_USER_POINTS;
       
        }

        private async void Extract_STL_Click(object sender, RoutedEventArgs e)
        {
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
            startSpinner();
            await Task.Run(() => { coreFunctionality.extractSkin(); });
            stopSpinner();
        }


        private void Ultrasound_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           // slider = sender as Slider;
            try
            {
                //slider.Minimum = 0;
                //slider.Maximum = fileCount - 1;
                //slider.TickFrequency = 1;
                slider_value = (int)slider.Value;
                string I = imagesDir + "/" + slider_value.ToString() + ".bmp"; //path
                image.Source = new BitmapImage(new Uri(I));
                frame_num_label.Content = "Frame:" + " " + slider_value.ToString();
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
                Ellipse ellipse = new Ellipse();
                ellipse.Fill = Brushes.Cyan;
                ellipse.Width = 2;
                ellipse.Height = 2;
                canvas1.Children.Add(ellipse);
                Canvas.SetLeft(ellipse, points.ElementAt(i).X);
                Canvas.SetTop(ellipse, points.ElementAt(i).Y);
            }

        }

        private void draw_polyline(List<Point> points)
        {
            //draw polylines of centerline
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (contourSeg != ContourSegmentation.FILL_POINTS || i != indexA - 1)
                {
                    Polyline pl = new Polyline();
                    pl.FillRule = FillRule.EvenOdd;
                    pl.StrokeThickness = 0.5;
                    pl.Points.Add(points.ElementAt(i));
                    pl.Points.Add(points.ElementAt(i + 1));
                    pl.Stroke = System.Windows.Media.Brushes.Yellow;
                    pl.StrokeStartLineCap = PenLineCap.Round;
                    pl.StrokeEndLineCap = PenLineCap.Round;
                    canvas1.Children.Add(pl);
                    polylines.Add(pl);
                }
            }

            if (contourSeg != ContourSegmentation.MANUAL)
            {
                if (contourSeg != ContourSegmentation.FILL_POINTS || indexA != 0)
                {
                    Polyline plast = new Polyline();
                    plast.FillRule = FillRule.EvenOdd;
                    plast.StrokeThickness = 0.5;
                    plast.Points.Add(points.ElementAt(points.Count - 1));
                    plast.Points.Add(points.ElementAt(0));
                    plast.Stroke = System.Windows.Media.Brushes.Yellow;
                    plast.StrokeStartLineCap = PenLineCap.Round;
                    plast.StrokeEndLineCap = PenLineCap.Round;
                    canvas1.Children.Add(plast);
                    polylines.Add(plast);
                }
            }
        }


        private void draw_polylineTest(List<Point> points)
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
                    pl.Stroke = System.Windows.Media.Brushes.Yellow;
                    pl.StrokeStartLineCap = PenLineCap.Round;
                    pl.StrokeEndLineCap = PenLineCap.Round;
                    canvas1.Children.Add(pl);
                    polylines.Add(pl);
                }
            }
        }

        private void Canvas1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!contourSeg.Equals(ContourSegmentation.CORRECTION))
            {
                Point point = e.GetPosition(image); //position relative to the image

                Ellipse ellipse = new Ellipse();
               
                ellipse.Width = 5;
                ellipse.Height = 5;

                if (e.ChangedButton == MouseButton.Left)
                {
                    if (contourSeg == ContourSegmentation.INSERT_USER_POINTS && userPoints.Count < 2)
                    {
                        clear_canvas();
                        points.Clear();

                        if (userPoints.Count == 0)
                        {

                            ellipse.Fill = Brushes.Yellow;
                            userPoints.Add(point);
                            startingFrame = slider_value;
                        }
                        else if (userPoints.Count > 0 && userPoints.Count < 2)
                        {
                            ellipse.Fill = Brushes.Red;
                            userPoints.Add(point);
                            endingFrame = slider_value;
                        }
                        canvas1.Children.Add(ellipse);
                        Canvas.SetLeft(ellipse, point.X);
                        Canvas.SetTop(ellipse, point.Y);
                    }
                    else if(contourSeg == ContourSegmentation.FILL_POINTS)
                    {
                        clear_canvas();
                       // bladder[slider_value].Insert(indexA++, point);
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
                    }
                    else if (contourSeg == ContourSegmentation.MANUAL)
                    {
                        clear_canvas();
                        bladder[slider_value].Add(point);
                        display();
                    }
                }
                else if (e.ChangedButton == MouseButton.Right && contourSeg == ContourSegmentation.MANUAL)
                {
                    if (bladder[slider_value].Any())
                    {
                        bladder[slider_value].RemoveAt(bladder[slider_value].Count - 1);
                        if (contourSeg == ContourSegmentation.MANUAL)
                        {
                            clear_canvas();                         }
                        else if (contourSeg == ContourSegmentation.CORRECTION)
                        {
                            canvas1.Children.Remove(ellipse);
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

        private void Canvas1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (contourSeg.Equals(ContourSegmentation.CORRECTION))
            {
                if (e.OriginalSource is Ellipse)
                {
                    Ellipse ellipse = (Ellipse)e.OriginalSource;
                    initialPoisition1.Y = (double)ellipse.GetValue(Canvas.TopProperty);//ellipse.GetValue(Canvas.TopProperty);
                    initialPoisition1.X = (double)ellipse.GetValue(Canvas.LeftProperty);
                    
                    //find the polylines which contain the pressed point
                    for(int i=0; i<polylines.Count; i++)
                    {
                        if (polylines[i].Points[1].Equals(initialPoisition1)) a = i;
                        
                        if (polylines[i].Points[0].Equals(initialPoisition1)) b = i;
                    }
                    ellipse.Opacity = 0.5;
                    if (e.ClickCount == 2)
                    {
                        canvas1.Children.Remove(ellipse);
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

        private void Canvas1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
        private void Canvas1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!areTherePoints() || contourSeg == ContourSegmentation.MANUAL) return;

            _start = Mouse.GetPosition(image);
            rectRemovePoints = new Rectangle();
            rectRemovePoints.Stroke = System.Windows.Media.Brushes.Red;
            rectRemovePoints.StrokeThickness = 0.5;
            Canvas.SetLeft(rectRemovePoints, _start.X);
            Canvas.SetTop(rectRemovePoints, _start.Y);
            canvas1.Children.Add(rectRemovePoints);
            
        }


        int indexA;
        private void Canvas1_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!areTherePoints() || contourSeg == ContourSegmentation.MANUAL) return;

            List<Point> pointsToRemove = new List<Point>();
            int count = 0;
            for (int i=0; i<bladder[slider_value].Count; i++)
            {
                if (bladder[slider_value][i].X >= Canvas.GetLeft(rectRemovePoints) && bladder[slider_value][i].Y >= Canvas.GetTop(rectRemovePoints) &&
                    bladder[slider_value][i].X <= Canvas.GetLeft(rectRemovePoints) + rectRemovePoints.Width && bladder[slider_value][i].Y <= Canvas.GetTop(rectRemovePoints) + rectRemovePoints.Height)
                {
                    pointsToRemove.Add(bladder[slider_value][i]);
                    if(count==0) indexA = i;
                    count++;
                }
               
            }

            if(count == bladder[slider_value].Count)
            {
                doManual();
                return;
            }

            bladder[slider_value].RemoveAll(item => pointsToRemove.Contains(item));
            canvas1.Children.Remove(rectRemovePoints);
            if(!contourSeg.Equals(ContourSegmentation.MANUAL) && count > 0) doFillPoints();
        }


        private void Canvas1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!areTherePoints()) return;

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
                        if (position.Y > canvas1.Height) position.Y = canvas1.Height;
                        if (position.Y < 0) position.Y = 0;
                        if (position.X > canvas1.Width) position.X = canvas1.Width;
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

        void clear_canvas()
        {
            canvas1.Children.Clear();
            canvas1.Children.Add(image);
        }

        void display()
        {
            if (areTherePoints()) {
                draw_points(bladder[slider_value]);
                draw_polylineTest(bladder[slider_value]);
            }
        }

        protected List<Matrix> zoom_out = new List<Matrix>();
        private void Canvas1_MouseWheel(object sender, MouseWheelEventArgs e)
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
            }
            else if (this.switch_auto_manual.Content.Equals("Correction"))
            {
                doManual();
            }
            else if (this.switch_auto_manual.Content.Equals("Fill Points"))
            {
                doCorrection();
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
            closeCurve = true;
            clear_canvas();
            display();
        }


       
        private void doManual()
        {
            this.switch_auto_manual.Content = "Manual";
            contourSeg = ContourSegmentation.MANUAL;
            if (areTherePoints()) bladder[slider_value].Clear();
            closeCurve = false;
            clear_canvas();
            display();
        }

        private void doFillPoints()
        {
            this.switch_auto_manual.Content = "Fill Points";
            contourSeg = ContourSegmentation.FILL_POINTS;
            closeCurve = true;
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

            List<List<Point>> points = new List<List<Point>>(fileCount);
            for(int i = 0; i<fileCount; i++)
            {
                points.Add(new List<Point>(cvp[0].Count));
            }
            int count = startingFrame;
            for(int i=0;  i< cvp.Count; i++)
            {           
                List<Point> contour = new  List<Point>();
                for (int j = 0; j < cvp[i].Count(); j++)
                {
                    contour.Add(new Point(cvp[i][j].X, cvp[i][j].Y )); // * (1 / calibration_x)
                }
                points[count++]=contour;
            }
            return points;
        }




        //Convert EDITCore.CVPoint to Point
        List<List<EDITCore.CVPoint>> WPFPointToCVPoint(List<List<Point>> points)
        {
            bladderCvPoints = new List<List<EDITCore.CVPoint>>();
            List<List<EDITCore.CVPoint>> cvp = new List<List<EDITCore.CVPoint>>();
            for (int i = startingFrame; i <=endingFrame; i++) 
            {
                List <EDITCore.CVPoint> contour = new List<EDITCore.CVPoint>();
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
