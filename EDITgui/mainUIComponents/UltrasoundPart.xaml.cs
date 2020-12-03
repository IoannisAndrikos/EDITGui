using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using Microsoft.Win32;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace EDITgui
{
    /// <summary>
    /// Interaction logic for UltrasoundPart.xaml
    /// </summary>
    public partial class UltrasoundPart : UserControl
    {

        public delegate void repeatProcessHandler();
        public static event repeatProcessHandler repeatPhotoAcousticProcess = delegate { };

        public delegate void bladderPointChangedHandler(List<List<Point>> newBladder);
        public static event bladderPointChangedHandler bladderPointChanged = delegate { };

        public delegate void sliderValueChangedHandler(int obj);
        public static event sliderValueChangedHandler sliderValueChanged = delegate { };

        public delegate void zoomUltrasoundChangedHandler(List<Matrix> obj);
        public static event zoomUltrasoundChangedHandler zoomUltrasoundChanged = delegate { };

        public delegate void STLBladderHandler(EDITgui.Geometry geometry);
        public static event STLBladderHandler returnBladderSTL = delegate { };

        public delegate void STLSkinHandler(EDITgui.Geometry geometry);
        public static event STLSkinHandler returnSkinSTL = delegate { };

       SolidColorBrush cyan = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#FF00FFFF"));
       SolidColorBrush yellow = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#FFF3FF00"));

        public enum ContourSegmentation
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
        public string ultrasoundDicomFile = null;
        string imagesDir;
        //-------------------------------

        //------------user set intial points on images------
        public List<Point> userPoints = new List<Point>(2);//
        public int startingFrame = -1;
        public int endingFrame = -1;
        //--------------------------------------------------

        //initialazation
        public ContourSegmentation contourSeg = ContourSegmentation.CORRECTION;

        //public bool wasBladderModelExtracted = false;

        List<double> pixelSpacing = new List<double>(); //x=pixelSpacing[0] y=pixelSpacing[1]
        List<double> imageSize = new List<double>(); //x=pixelSpacing[0] y=pixelSpacing[1]
        double calibration_x;
        double calibration_y;

        List<Point> points = new List<Point>();
        List<Polyline> polylines = new List<Polyline>();
        List<List<EDITCore.CVPoint>> bladderCvPoints = new List<List<EDITCore.CVPoint>>();
        public List<List<Point>> bladder = new List<List<Point>>();
        public List<double> bladderArea = new List<double>();
        public List<double> bladderPerimeter = new List<double>();

        public String bladderGeometryPath = null;

        MainWindow mainWindow;

        //create objects of the other classes
        StudyFile studyFile = new StudyFile();
        Messages messages = new Messages();
        coreFunctionality coreFunctionality;
        metricsCalculations metrics = new metricsCalculations();
        settings studySettings;
        checkBeforeExecute check;

        public UltrasoundPart()
        {
            InitializeComponent();
            PhotoAcousticPart.zoomPhotoAccousticChanged += OnPhotoAccousticZoomChanged;
            chechBox_Logger.IsChecked = true;
           // coreFunctionality.setExaminationsDirectory("C:/Users/Legion Y540/Desktop/EDIT_STUDIES");
            contourSeg = ContourSegmentation.INSERT_USER_POINTS;
        }

        public UltrasoundPart(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            PhotoAcousticPart.zoomPhotoAccousticChanged += OnPhotoAccousticZoomChanged;
            chechBox_Logger.IsChecked = true;
            // coreFunctionality.setExaminationsDirectory("C:/Users/Legion Y540/Desktop/EDIT_STUDIES");
            contourSeg = ContourSegmentation.INSERT_USER_POINTS;
        }

        public void setSettings(settings studySettings ,checkBeforeExecute check)
        {
            this.studySettings = studySettings;
            this.check = check;
        }

        public coreFunctionality InitializeCoreFunctionality
        {
            get { return coreFunctionality; }
            set { coreFunctionality = value; }
        }


        public void OnPhotoAccousticZoomChanged(List<Matrix> obj)
        {
            var element = canvasUltrasound;
            element.RenderTransform = new MatrixTransform(obj.LastOrDefault());
            zoom_out = obj;
        }

        private async void LoadDicom_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = messages.selectDicom;
            if (openFileDialog.ShowDialog() == true)
            {
                metrics_label.Visibility = Visibility.Hidden;
                startSpinner();
                if (bladder != null) bladder.Clear();
                clear_canvas();
                coreFunctionality.repeatSegmentation();
                ultrasoundDicomFile = null;
                bool enablelogging = chechBox_Logger.IsChecked.Value;
                await Task.Run(() => {
                    string dicomPath = studyFile.copyFileToWorkspace(studyFile.getWorkspaceDicomPath(), openFileDialog.FileName, StudyFile.FileType.UltrasoundDicomFile);
                    imagesDir = coreFunctionality.exportImages(dicomPath, enablelogging);
                    pixelSpacing = coreFunctionality.pixelSpacing;
                    imageSize = coreFunctionality.imageSize;
                });
                if (imagesDir != null)
                {
                    metrics.setPixelSpacing(pixelSpacing);
                    studySettings.setPixelSpacing(pixelSpacing);
                    fitUIAccordingToDicomImageSize(imageSize[1], imageSize[0]);
                    ultrasoundDicomFile = openFileDialog.FileName;
                    BitmapFromPath(imagesDir + Path.DirectorySeparatorChar + "0.bmp");

                    ultrasound_studyname_label.Content = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    frame_num_label.Content = messages.frame + ":" + " " + "0";
                    fileCount = Directory.GetFiles(imagesDir, "*.bmp", SearchOption.AllDirectories).Length;
                    slider.Value = 0;
                    sliderValueChanged(slider_value);// here we pass slider_value to Photoaccoustic part 
                    slider.TickFrequency = 1 / (double)fileCount;
                    slider.Minimum = 0;
                    slider.Maximum = fileCount - 1;
                    slider.TickFrequency = 1;
                    slider.Visibility = Visibility.Visible;
                    switch_auto_manual.Visibility = Visibility.Visible;
                    calibration_x = image.Source.Width / canvasUltrasound.Width;
                    calibration_y = image.Source.Height / canvasUltrasound.Height;
                }
                else
                {
                    ultrasound_studyname_label.Content = "";
                    frame_num_label.Content = "";
                    image.Source = null;
                    switch_auto_manual.Visibility = Visibility.Hidden;
                    slider.Visibility = Visibility.Hidden;
                }
                doRepeatProcess();
                stopSpinner();
            }
        }

        public void AfterLoadUltrasoundDicom(string studyName, string filename, string imagesDir, List<double> pixelSpacing, List<double> imageSize)
        {
            this.ultrasoundDicomFile = filename;
            this.imagesDir = imagesDir;
            this.pixelSpacing = pixelSpacing;
            this.imageSize = imageSize;
            metrics.setPixelSpacing(this.pixelSpacing);
            studySettings.setPixelSpacing(this.pixelSpacing);
            fitUIAccordingToDicomImageSize(this.imageSize[1], this.imageSize[0]);
            //image.Source = BitmapFromUri(new Uri(imagesDir + "/0.bmp"));
            BitmapFromPath(imagesDir + Path.DirectorySeparatorChar + "0.bmp");
            ultrasound_studyname_label.Content = studyName + " " + messages.ultrasound;
            frame_num_label.Content = "Frame:" + " " + "0";
            fileCount = Directory.GetFiles(imagesDir, "*.bmp", SearchOption.AllDirectories).Length;
            slider.Value = 0;
            sliderValueChanged(slider_value);// here we pass slider_value to Photoaccoustic part 
            slider.TickFrequency = 1 / (double)fileCount;
            slider.Minimum = 0;
            slider.Maximum = fileCount - 1;
            slider.TickFrequency = 1;
            slider.Visibility = Visibility.Visible;
            switch_auto_manual.Visibility = Visibility.Visible;
            calibration_x = image.Source.Width / canvasUltrasound.Width;
            calibration_y = image.Source.Height / canvasUltrasound.Height;
            doRepeatProcess();
        }

        private void initializeBladderList()
        {
            bladder.Clear();
            bladderArea.Clear();
            bladderPerimeter.Clear();

            List<Point> emptyList = new List<Point>();
            for (int i = 0; i < fileCount; i++)
            {
                bladder.Add(emptyList.ToList());
                bladderArea.Add(0);
                bladderPerimeter.Add(0);
            }
        }


        private async void Extract_bladder_Click(object sender, RoutedEventArgs e)
        {
            string message = check.getMessage(checkBeforeExecute.executionType.extract2DBladder);
            if (message != null)
            {
                CustomMessageBox.Show(message, messages.warning, MessageBoxButton.OK);
                return;
            }

            startSpinner();
            int repeats = int.Parse(Repeats.Text);
            int smoothing = int.Parse(Smoothing.Text);
            double lamda1 = double.Parse(Lamda1.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            double lamda2 = double.Parse(Lamda2.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            int levelsetSize = int.Parse(LevelsetSize.Text);
            bool applyEqualizeHist = chechBox_FIltering.IsChecked.Value;

            //the code bellow is to avoid an issue when user marks the starting frame after the ending frame
            if (endingFrame < startingFrame)
            {
                int temp = startingFrame;
                startingFrame = endingFrame;
                endingFrame = temp;
            } 

            await Task.Run(() => {
                bladderCvPoints = coreFunctionality.Bladder2DExtraction(repeats, smoothing, lamda1, lamda2, levelsetSize, applyEqualizeHist, startingFrame, endingFrame, userPoints);
                if (!bladderCvPoints.Any()) return; 
                bladder = editCVPointToWPFPoint(bladderCvPoints);
            });

            doCorrection();
            stopSpinner();
        }

        private void Repeat_process_Click(object sender, RoutedEventArgs e)
        {
            if(bladder.Any())
            {
                MessageBoxResult result = CustomMessageBox.Show(messages.makeUserAwareOfRepeatProcess, messages.warning, MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    doRepeatProcess();
                }
            }
            else
            {
                doRepeatProcess();
            }
        }

        public void doRepeatProcess()
        {
            userPoints.Clear();
            //bladder.Clear();
            //bladderCvPoints.Clear();
            //bladderArea.Clear();
            //bladderPerimeter.Clear();
            initializeBladderList();
            metrics_label.Visibility = Visibility.Hidden;
            coreFunctionality.repeatSegmentation();
            startingFrame = -1;
            endingFrame = -1;
            doInsertUserPoints();
            mainWindow.cleanVTKRender();
            repeatPhotoAcousticProcess(); //trigger repeat process of photoAcousticPart
        }


        double distanceBetweenFrames;
        private async void Extract_STL_Click(object sender, RoutedEventArgs e)
        {
            string message = check.getMessage(checkBeforeExecute.executionType.extract3DBladder);
            if (message != null)
            {
                CustomMessageBox.Show(message, messages.warning, MessageBoxButton.OK);
                return;
            }

            bladderCvPoints = WPFPointToCVPoint(bladder);

            if (!bladderCvPoints.Any()) return;
            startSpinner();
            await Task.Run(() => {
                bladderGeometryPath = coreFunctionality.extractBladderSTL(bladderCvPoints);
            });
            
            if (bladderGeometryPath != null)
            {
                EDITgui.Geometry bladderGeometry = new Geometry() { geometryName = "Bladder", Path = bladderGeometryPath, actor = null };
                returnBladderSTL(bladderGeometry);
            }
            stopSpinner();
        }

        private async void Export_Skin_Click(object sender, RoutedEventArgs e)
        {
            string message = check.getMessage(checkBeforeExecute.executionType.extract3DLayer);
            if (message != null)
            {
                CustomMessageBox.Show(message, messages.warning, MessageBoxButton.OK);
                return;
            }

            bladderCvPoints = WPFPointToCVPoint(bladder);
            if (!bladderCvPoints.Any()) return;
            startSpinner();
            String STLPath = null;
            await Task.Run(() => {
                STLPath = coreFunctionality.extractSkin(bladderCvPoints);
            });
            EDITgui.Geometry skinGeometry = new Geometry() { geometryName = "Layer", Path = STLPath, actor = null };
            if(STLPath != null) returnSkinSTL(skinGeometry);
            stopSpinner();
        }


        private void Ultrasound_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                recalculateMetricsAfterManualSegmentation(); //make sure that we have caclulated metrics before change frame
                slider_value = (int)slider.Value;
                sliderValueChanged(slider_value);// here we pass slider_value to Photoaccoustic part 
                BitmapFromPath(imagesDir + Path.DirectorySeparatorChar + slider_value.ToString() + ".bmp");
                frame_num_label.Content = messages.frame + ":" + " " + slider_value.ToString();
                switch (mainWindow.currentProcess)
                {
                    case MainWindow.process.AUTO:
                        if (userPoints.Count == 2){
                            doCorrection();
                        }
                        break;
                    case MainWindow.process.ANOTATION:
                        doCorrection();
                        break;
                }
                clear_canvas();
                display();
            }
            catch(Exception ex){
                //TO DO
            }
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
                ellipse.Fill = cyan;
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

            polylines.Clear();

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
                    pl.Stroke = yellow;
                    pl.StrokeStartLineCap = PenLineCap.Round;
                    pl.StrokeEndLineCap = PenLineCap.Round;
                    canvasUltrasound.Children.Add(pl);
                    polylines.Add(pl);
                }
            }
        }

        public void displayMetrics(bool recalculate = false)
        {
            if (recalculate)
            {
                bladderArea[slider_value] = metrics.calulateArea(bladder[slider_value]);
                bladderPerimeter[slider_value] = metrics.calulatePerimeter(bladder[slider_value]);
            }
            try
            {
                metrics_label.Content = messages.perimeter + " = " + Math.Round(bladderPerimeter[slider_value], 2) + " " + messages.mm + Environment.NewLine +
                                        messages.area + " = " + Math.Round(bladderArea[slider_value], 2) + " " + messages.mmB2;

            }
            catch (Exception e)
            {
                //TO DO
            }
        }

        private void recalculateMetricsAfterManualSegmentation()
        {
            if(contourSeg == ContourSegmentation.MANUAL && bladder[slider_value].Any())
            {
                bladderArea[slider_value] = metrics.calulateArea(bladder[slider_value]);
                bladderPerimeter[slider_value] = metrics.calulatePerimeter(bladder[slider_value]);
            }
        }


        List<Point> tempBladder = new List<Point>();
        private void canvasUltrasound_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!contourSeg.Equals(ContourSegmentation.CORRECTION))
            {
                Point point = e.GetPosition(image); //position relative to the image

                if (e.ChangedButton == MouseButton.Left)
                {
                    if (mainWindow.currentProcess == MainWindow.process.AUTO && contourSeg == ContourSegmentation.INSERT_USER_POINTS && userPoints.Count < 2)
                    {
                        clear_canvas();
                        points.Clear();

                        if (userPoints.Count == 0)
                        {
                            canvasUltrasound.Children.Add(startingFrameMarker);
                            Canvas.SetLeft(startingFrameMarker, point.X - startingFrameMarker.Width / 2);
                            Canvas.SetTop(startingFrameMarker, point.Y - startingFrameMarker.Height / 2);
                            startingFrameMarker.Visibility = Visibility.Visible;

                            userPoints.Add(point);
                            startingFrame = slider_value;
                        }
                        else if (userPoints.Count > 0 && userPoints.Count < 2)
                        {
                            canvasUltrasound.Children.Add(endingFrameMarker);
                            Canvas.SetLeft(endingFrameMarker, point.X - endingFrameMarker.Width / 2);
                            Canvas.SetTop(endingFrameMarker, point.Y - endingFrameMarker.Height / 2);
                            endingFrameMarker.Visibility = Visibility.Visible;
                            userPoints.Add(point);
                            endingFrame = slider_value;
                        }

                        clear_canvas();
                        display();
                    }
                    else if (contourSeg == ContourSegmentation.FILL_POINTS)
                    {
                        clear_canvas();
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
                    }
                    else if (contourSeg == ContourSegmentation.MANUAL)
                    {
                        bladder[slider_value].Add(point);
                        clear_canvas();
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
                        bladderPointChanged(bladder);
                        displayMetrics(true);
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
                displayMetrics(true);
                return;
            }
            bladder[slider_value].RemoveAll(item => pointsToRemove.Contains(item));
            if (!contourSeg.Equals(ContourSegmentation.MANUAL) && count > 0) doFillPoints();

            pointsToRemove.Clear();

            displayMetrics();
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
        
        public void BitmapFromPath(string imagePath)
        {
            image.Source = new BitmapImage();
            ((BitmapImage)image.Source).BeginInit();
            ((BitmapImage)image.Source).CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            ((BitmapImage)image.Source).CacheOption = BitmapCacheOption.OnLoad;
            ((BitmapImage)image.Source).UriSource = new Uri(imagePath);
            ((BitmapImage)image.Source).EndInit();
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
            if (areTherePoints() && contourSeg == ContourSegmentation.CORRECTION)
            {
                displayMetrics();
                metrics_label.Visibility = Visibility.Visible;
            }
            else
            {
                metrics_label.Visibility = Visibility.Hidden;
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

            if(e.Delta < 0) zoom_out.Remove(zoom_out.LastOrDefault());

            if ((matrix.M11 >= 1 && e.Delta > 0))//((matrix.M11 >= 1 && cout_wh == 0) || (matrix.M11 >= 1.1 && cout_wh > 0))
            {
                matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);
                element.RenderTransform = new MatrixTransform(matrix);
                zoom_out.Add(matrix);
            }
            else if ((matrix.M11 >= 1.1 && e.Delta < 0))
            {
                //matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);
                element.RenderTransform = new MatrixTransform(zoom_out.LastOrDefault());
            }
            zoomUltrasoundChanged(zoom_out);
        }

        private void fitUIAccordingToDicomImageSize(double height, double width)
        {
            double value = 736 - width;

            Thickness ImageRectangleMargin = image_Rectangle.Margin;
            Thickness ImageBorderMargin = imageborder.Margin;
            Thickness StudyLabelMargin = ultrasound_studyname_label.Margin;
            Thickness ConfigMargin = segmentation_config.Margin;
            Thickness waitMargin = Wait.Margin;
            Thickness switchAutoManual = switch_auto_manual.Margin;

            ImageRectangleMargin.Left = value;
            ImageBorderMargin.Left = value + 1;
            StudyLabelMargin.Left = value;
            ConfigMargin.Left = value + 1;
            waitMargin.Left = 736 - width / 2 - Wait.Width / 2;
            switchAutoManual.Left = value + 1;

            image_Rectangle.Margin = ImageRectangleMargin;
            imageborder.Margin = ImageBorderMargin;
            ultrasound_studyname_label.Margin = StudyLabelMargin;
            segmentation_config.Margin = ConfigMargin;
            Wait.Margin = waitMargin;
            switch_auto_manual.Margin = switchAutoManual;


            image_Rectangle.Width = width + 2;
            imageborder.Width = width;
            image.Width = width;
            image.MinWidth = width;

        }

        private void Switch_auto_manual_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.contourSeg == ContourSegmentation.MANUAL)
            {
                doCorrection();
                displayMetrics(true);
            }
            else if (this.contourSeg == ContourSegmentation.CORRECTION)
            {
                doManual();
            }
            else if (this.contourSeg == ContourSegmentation.FILL_POINTS)
            {
                doCorrection();
                displayMetrics(true);
            }
        }

        private void ClosedSurface_Click(object sender, RoutedEventArgs e)
        {
            coreFunctionality.fillHoles = closedSurface.IsChecked.Value;
        }

        private void ChechBox_Logger_Click(object sender, RoutedEventArgs e)
        {
            coreFunctionality.setLoggingOnOff(chechBox_Logger.IsChecked.Value);
        }


        public void startSpinner()
        {
            ((Storyboard)FindResource("WaitStoryboard")).Begin();
            //applicationGrid.IsEnabled = false;
            mainWindow.Rat.IsEnabled = false;
            Wait.Visibility = Visibility.Visible;
        }

        public void stopSpinner()
        {
            ((Storyboard)FindResource("WaitStoryboard")).Stop();
            //applicationGrid.IsEnabled = true;
            mainWindow.Rat.IsEnabled = true;
            Wait.Visibility = Visibility.Hidden;
        }

        public void doCorrection()
        {
            this.switch_auto_manual.doCorrectionState();
            contourSeg = ContourSegmentation.CORRECTION;
            bladderPointChanged(bladder);
            clear_canvas();
            display();
        }



        public void doManual()
        {
           this.switch_auto_manual.doManualState();
            contourSeg = ContourSegmentation.MANUAL;
            if (areTherePoints()) bladder[slider_value].Clear();
            clear_canvas();
            display();
        }

        public void doFillPoints()
        {
            this.switch_auto_manual.doFillPointState();
            contourSeg = ContourSegmentation.FILL_POINTS;
            clear_canvas();
            display();
        }

        public void doInsertUserPoints()
        {
            this.switch_auto_manual.doCorrectionState();
            if(mainWindow.currentProcess == MainWindow.process.ANOTATION)
            {
                contourSeg = ContourSegmentation.CORRECTION;
            }
            else
            {
                contourSeg = ContourSegmentation.INSERT_USER_POINTS;
            }
            
            clear_canvas();
            display();
        }



        public bool areTherePoints()
        {
            return (image.Source!=null && bladder.Any() && bladder[slider_value].Any());
        }

        public bool sliderIsBetwweenFrameOfInterest()
        {
            return (slider_value >= startingFrame && slider_value <= endingFrame);
        }

        //--------------------------------------M A N A G E - P O I N T S---------------------------------

        //Convert Point to EDITCore.CVPoint
        List<List<Point>> editCVPointToWPFPoint(List<List<EDITCore.CVPoint>> cvp)
        {
            bladder.Clear();
            bladderArea.Clear();
            bladderPerimeter.Clear();

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
                points[count++] = contour.ToList();
                contour.Clear();
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
        public List<List<EDITCore.CVPoint>> WPFPointToCVPoint(List<List<Point>> points)
        {
            List<List<EDITCore.CVPoint>> cvp = new List<List<EDITCore.CVPoint>>();

            string message = check.checkForSegmentationGaps(points);
            if (message != null)
            {
                CustomMessageBox.Show(message, messages.warning, MessageBoxButton.OK);
                return cvp;
            }


            bladderCvPoints.Clear();
            List<EDITCore.CVPoint> contour = new List<EDITCore.CVPoint>();
           
            //for (int i = startingFrame; i <= endingFrame; i++)
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Any())
                {
                    for (int j = 0; j < points[i].Count(); j++)
                    {
                        contour.Add(new EDITCore.CVPoint(points[i][j].X, points[i][j].Y));
                    }
                    cvp.Add(contour.ToList());
                    contour.Clear();
                }
            }
            return cvp;
        }

        //------- SOME GETTERS---------------

       public List<List<EDITCore.CVPoint>> getBladderCVPoints()
        {
            bladderCvPoints = WPFPointToCVPoint(bladder);
            return bladderCvPoints;
        }

        public int getStartingFrame()
        {
            return startingFrame;
        }

        public int getEndingFrame()
        {
            return endingFrame;
        }

        public List<List<Point>> getBladderPoints()
        {
            return bladder;
        }

        public List<double> getBladderArea()
        {
            return bladderArea;
        }

        public List<double> getBladderPerimeter()
        {
            return bladderPerimeter;
        }

        public bool autoExecutionUserPointsWereSet()
        {
            return (mainWindow.currentProcess == MainWindow.process.AUTO && startingFrame != -1 && endingFrame != -1);
        }
    }
}
