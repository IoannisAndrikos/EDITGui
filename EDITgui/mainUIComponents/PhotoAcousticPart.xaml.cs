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
using System.Globalization;

using Path = System.IO.Path;

namespace EDITgui
{
    /// <summary>
    /// Interaction logic for UltrasoundPart.xaml
    /// </summary>
    public partial class PhotoAcousticPart : UserControl
    {
        public delegate void zoomPhotoAccousticChangedHandler(List<Matrix> obj);
        public static event zoomPhotoAccousticChangedHandler zoomPhotoAccousticChanged = delegate { };


        public delegate void STLThicknessHandler(EDITgui.Geometry geometry);
        public static event STLThicknessHandler returnThicknessSTL = delegate { };

        Context context;

        SolidColorBrush cyan = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FFFF"));
        SolidColorBrush yellow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3FF00"));

        public enum ContourSegmentation
        {
            CORRECTION,
            MANUAL,
            FILL_POINTS
        }

        enum OxyDeOxyState { OXY, DEOXY}

        public bool wasThicknessModelExtracted = false;

        //-----------for slider----------
        int slider_value = 0;
        int fileCount;
        public string OXYDicomFile = null;
        public string DeOXYDicomFile = null;
        //-------------------------------

        //initialization
        public ContourSegmentation contourSeg = ContourSegmentation.CORRECTION;
        OxyDeOxyState currentOxyDeOxyState = OxyDeOxyState.OXY;

        List<double> pixelSpacing = new List<double>(); //x=pixelSpacing[0] y=pixelSpacing[1]
        List<double> imageSize = new List<double>();
        double calibration_x;
        double calibration_y;


        List<Point> points = new List<Point>();
        List<Polyline> polylines = new List<Polyline>();
        List<List<EDITCore.CVPoint>> thicknessCvPoints = new List<List<EDITCore.CVPoint>>();
        public List<List<Point>> thickness = new List<List<Point>>();
        List<EDITCore.CVPoint> contourForFix = new List<EDITCore.CVPoint>();
        public List<double> meanThickness = new List<double>();
        public List<List<Point>> bladderUltrasound = new List<List<Point>>();
        public String thicknessGeometryPath = null;

        public List<double> thicknessArea = new List<double>();
        public List<double> thicknessPerimeter = new List<double>();

        string OXYimagesDir;
        string deOXYimagesDir;
        
        public PhotoAcousticPart()
        {
            InitializeComponent();
            UltrasoundPart.sliderValueChanged += OnUltrasoundSliderValueChanged;
            UltrasoundPart.zoomUltrasoundChanged += OnUltrasoundZoomChanged;
            doOXYState();
        }

        public PhotoAcousticPart(Context context)
        {
            InitializeComponent();
            this.context = context;
            UltrasoundPart.sliderValueChanged += OnUltrasoundSliderValueChanged;
            UltrasoundPart.zoomUltrasoundChanged += OnUltrasoundZoomChanged;
            UltrasoundPart.bladderPointChanged += OnBladderPointChanged;
            UltrasoundPart.repeatPhotoAcousticProcess += OnrepeatProcess;
            doOXYState();
        }


        public void OnUltrasoundSliderValueChanged(int obj)
        {
            slider_value = (int)obj;
            if (slider_value < fileCount)
            {
                if (currentOxyDeOxyState == OxyDeOxyState.OXY && OXYDicomFile != null )
                {
                    BitmapFromPath(OXYimagesDir + Path.DirectorySeparatorChar + obj + ".bmp");
                }
                else if (currentOxyDeOxyState == OxyDeOxyState.DEOXY && DeOXYDicomFile != null)
                {
                    BitmapFromPath(deOXYimagesDir + Path.DirectorySeparatorChar + obj + ".bmp");
                }
                frame_num_label.Content = context.getMessages().frame + ": " + slider_value;
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
            //if (!ultrasound.areTherePoints()) return;
            
            bladderUltrasound = newBladder.ToList();
            clear_canvas();
            display();
        }

        private void initializeBladderList()
        {
            thickness.Clear();
            meanThickness.Clear();
            thicknessArea.Clear();
            thicknessPerimeter.Clear();
            List<Point> emptyList = new List<Point>();
            for (int i = 0; i < fileCount; i++)
            {
                thickness.Add(emptyList.ToList());
                meanThickness.Add(0);
                thicknessArea.Add(0);
                thicknessPerimeter.Add(0);
            }
        }


        public void OnrepeatProcess()
        {
            //thickness.Clear();
            //meanThickness.Clear();
            initializeBladderList();
            thicknessCvPoints.Clear();
            bladderUltrasound.Clear();
            metrics_label.Visibility = Visibility.Hidden;
            contourSeg = ContourSegmentation.CORRECTION;
            clear_canvas();
            display();
        }

        private async void LoadΟΧΥDicom_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = context.getMessages().selectDicom;
            if (openFileDialog.ShowDialog() == true)
            {
                startSpinner();

                context.getCore().repeatSegmentation();
                OXYDicomFile = null;

                await Task.Run(() =>
                {
                    string dicomPath = context.getStudyFile().copyFileToWorkspace(context.getStudyFile().getWorkspaceDicomPath(), openFileDialog.FileName, StudyFile.FileType.OXYDicomFile);
                    OXYimagesDir = context.getCore().exportOXYImages(dicomPath, true); //enablelogging = true
                    pixelSpacing = context.getCore().pixelSpacing;
                    imageSize = context.getCore().imageSize;

                });
                if (OXYimagesDir != null)
                {
                    context.getMetrics().setPixelSpacing(pixelSpacing);
                    fitUIAccordingToDicomImageSize(imageSize[1], imageSize[0]);
                    OXYDicomFile = openFileDialog.FileName;

                    makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Visible);

                    BitmapFromPath(OXYimagesDir + Path.DirectorySeparatorChar + slider_value + ".bmp");
                    OXY_studyname_label.Content = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    frame_num_label.Content = context.getMessages().frame + ": " + slider_value;
                    fileCount = Directory.GetFiles(OXYimagesDir, "*.bmp", SearchOption.AllDirectories).Length;
                    OnrepeatProcess();
                }
                doOXYState();
                stopSpinner();
            }
        }


        private async void LoadDeOXYDicom_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = context.getMessages().selectDicom;
            if (openFileDialog.ShowDialog() == true)
            {
                startSpinner();
                //clear_canvas();
                context.getCore().repeatSegmentation();
                DeOXYDicomFile = null;

                await Task.Run(() =>
                {
                    string dicomPath = context.getStudyFile().copyFileToWorkspace(context.getStudyFile().getWorkspaceDicomPath(), openFileDialog.FileName, StudyFile.FileType.DeOXYDicomFile);
                    deOXYimagesDir = context.getCore().exportDeOXYImages(dicomPath, true); //enablelogging = true
                    pixelSpacing = context.getCore().pixelSpacing;
                    imageSize = context.getCore().imageSize;
                });
                if (deOXYimagesDir != null)
                {
                    context.getMetrics().setPixelSpacing(pixelSpacing);
                    fitUIAccordingToDicomImageSize(imageSize[1], imageSize[0]);
                    DeOXYDicomFile = openFileDialog.FileName;

                    makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Visible);

                    BitmapFromPath(deOXYimagesDir + Path.DirectorySeparatorChar + slider_value + ".bmp"); //imagesDir + "/0.bmp"
                    DeOXY_studyname_label.Content = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    frame_num_label.Content = context.getMessages().frame + ": " + slider_value;
                    fileCount = Directory.GetFiles(deOXYimagesDir, "*.bmp", SearchOption.AllDirectories).Length;
                    // OnrepeatProcess();
                }
                doDeOXYState();
                stopSpinner();

            }
        }



        public void AfterLoadOXYDicom(string studyName, string filename, string imagesDir, List<double> pixelSpacing, List<double> imageSize)
        {
            this.OXYDicomFile = filename;
            this.OXYimagesDir = imagesDir;
            this.pixelSpacing = pixelSpacing;
            this.imageSize = imageSize;
            context.getMetrics().setPixelSpacing(pixelSpacing);
            fitUIAccordingToDicomImageSize(imageSize[1], imageSize[0]);
            makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Visible);
            doOXYState();
            BitmapFromPath(OXYimagesDir + Path.DirectorySeparatorChar + slider_value + ".bmp");
            OXY_studyname_label.Content = studyName + " " + context.getMessages().oxy;
            frame_num_label.Content = context.getMessages().frame + ": " + slider_value;
            fileCount = Directory.GetFiles(OXYimagesDir, "*.bmp", SearchOption.AllDirectories).Length;
            OnrepeatProcess();
        }


        public void AfterLoadDeOXYDicom(string studyName, string filename, string imagesDir, List<double> pixelSpacing, List<double> imageSize)
        {
            this.DeOXYDicomFile = filename;
            this.deOXYimagesDir = imagesDir;
            this.pixelSpacing = pixelSpacing;
            this.imageSize = imageSize;
            context.getMetrics().setPixelSpacing(pixelSpacing);
            fitUIAccordingToDicomImageSize(this.imageSize[1], this.imageSize[0]);
            makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Visible);
            doDeOXYState();
            BitmapFromPath(deOXYimagesDir + Path.DirectorySeparatorChar + slider_value + ".bmp"); //imagesDir + "/0.bmp"
            DeOXY_studyname_label.Content = studyName + " " + context.getMessages().deoxy;
            frame_num_label.Content = context.getMessages().frame + ": " + slider_value;
            fileCount = Directory.GetFiles(deOXYimagesDir, "*.bmp", SearchOption.AllDirectories).Length;
        }


        private async void Extract_thikness_Click(object sender, RoutedEventArgs e)
        {
            string message = context.getCheck().getMessage(checkBeforeExecute.executionType.extract2DThickness);
            if (message != null)
            {
                CustomMessageBox.Show(message, context.getMessages().warning, MessageBoxButton.OK);
                return;
            }

            double minThick = double.Parse(minThickness.Text.Replace(",","."), CultureInfo.InvariantCulture);
            double maxThick = double.Parse(maxThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            bool bigTumor = big_tumor.IsChecked.Value;
            List<List<EDITCore.CVPoint>> bladderCVPoints = context.getUltrasoundPart().getBladderCVPoints();
            if (!bladderCVPoints.Any()) return;

            startSpinner();
            await Task.Run(() =>
            {
                thicknessCvPoints = context.getCore().extractThickness(context.getUltrasoundPart().getBladderCVPoints(), minThick, maxThick, bigTumor);
                if (!thicknessCvPoints.Any()) return;
                thickness = editCVPointToWPFPoint(thicknessCvPoints);
                fiillMetrics(context.getCore().meanThickness);
            });
            bladderUltrasound = context.getUltrasoundPart().getBladderPoints().ToList();
            displayMetrics();
            clear_canvas();
            display();
            stopSpinner();
        }

        private async void Recalculate_Click(object sender, RoutedEventArgs e)
        {
            string message = context.getCheck().getMessage(checkBeforeExecute.executionType.recalculate);
            if (message != null)
            {
                CustomMessageBox.Show(message, context.getMessages().warning, MessageBoxButton.OK);
                return;
            }

            startSpinner();
            bladderUltrasound = context.getUltrasoundPart().getBladderPoints().ToList();

            double minThick = double.Parse(minThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            double maxThick = double.Parse(maxThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            bool bigTumor = big_tumor.IsChecked.Value;

            await Task.Run(() =>
            {
                contourForFix = context.getCore().recalculateThicknessOfContour(slider_value, WPFPointToCVPoint(bladderUltrasound[slider_value]), minThick, maxThick, bigTumor);
                if (!contourForFix.Any()) return;
                thickness[slider_value].AddRange(editCVPointToWPFPoint(contourForFix));
                //contourForFix.Clear();
                // meanThickness.AddRange(coreFunctionality.meanThickness);
                meanThickness[slider_value] = context.getCore().uniqueContourMeanThickness;
               // fillMeanThicknessList(coreFunctionality.meanThickness);
            });
            clear_canvas();
            displayMetrics();
            display();
            stopSpinner();
        }

        private async void Extract_STL_Click(object sender, RoutedEventArgs e)
        {
            string message = context.getCheck().getMessage(checkBeforeExecute.executionType.extract3DThickness);
            if (message != null)
            {
                CustomMessageBox.Show(message, context.getMessages().warning, MessageBoxButton.OK);
                return;
            }

            thicknessCvPoints = WPFPointToCVPoint(thickness);
            if (!thicknessCvPoints.Any()) return;

            startSpinner();
            await Task.Run(() => {
                thicknessGeometryPath = context.getCore().extractThicknessSTL(thicknessCvPoints);
                });
            EDITgui.Geometry thicknessGeometry = new Geometry() { geometryName = "Thickness", Path = thicknessGeometryPath, actor = null };
            if (thicknessGeometryPath != null)
            {
                returnThicknessSTL(thicknessGeometry);
                wasThicknessModelExtracted = true;
            }
            stopSpinner();
        }

        private async void Extract_OXYDeOXY_Click(object sender, RoutedEventArgs e)
        {
            string message = context.getCheck().getMessage(checkBeforeExecute.executionType.extractOXYDeOXY);
            if (message != null)
            {
                CustomMessageBox.Show(message, context.getMessages().warning, MessageBoxButton.OK);
                return;
            }


            thicknessCvPoints = WPFPointToCVPoint(thickness);
            if (!thicknessCvPoints.Any()) return;

            startSpinner();
            List<String> txtPaths = new List<string>();
            await Task.Run(() => {
                txtPaths = context.getCore().extractOXYandDeOXYPoints(context.getUltrasoundPart().getBladderCVPoints(), thicknessCvPoints, context.getUltrasoundPart().bladderGeometryPath, thicknessGeometryPath);
            });

            EDITgui.Geometry OXYGeometry = new Geometry() { geometryName = "OXY", Path = txtPaths[0], actor = null };
            EDITgui.Geometry DeOXYGeometry = new Geometry() { geometryName = "DeOXY", Path = txtPaths[1], actor = null };
            if (txtPaths.Any())
            {
                returnThicknessSTL(OXYGeometry);
                 returnThicknessSTL(DeOXYGeometry);
            }
            stopSpinner();
        }

        //----------------------------------------------------------CANVAS OPERATIONS--------------------------------------------------------
        public bool areTherePoints()
        {
            return (image.Source!=null && thickness.Any() && thickness[slider_value].Any());
        }

        void display()
        {
            if (image.Source !=null && areTherePoints())
            {
                draw_polyline(thickness[slider_value]);
                draw_points(thickness[slider_value]);
            }
            if (image.Source != null && context.getUltrasoundPart().areTherePoints() && bladderUltrasound.Any())
            {
                draw_polyline_ultrasound(bladderUltrasound[slider_value]);
            }

            if (areTherePoints() && thickness.Any() && contourSeg == ContourSegmentation.CORRECTION)
            {
                displayMetrics();
                metrics_label.Visibility = Visibility.Visible;
            }
            else
            {
                metrics_label.Visibility = Visibility.Hidden;
            }
        }


        private void draw_points(List<Point> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Ellipse ellipse = new Ellipse();
                //ellipse.Fill = Brushes.Blue;
                ellipse.Fill = cyan;
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
            }
        }

        private void displayMetrics(bool recalculate = false)
        {
            try
            {
                if (recalculate)
                {
                    thicknessArea[slider_value] = context.getMetrics().calulateArea(thickness[slider_value]);
                    thicknessPerimeter[slider_value] = context.getMetrics().calulatePerimeter(thickness[slider_value]);
                }

                if(context.getMainWindow().currentProcess == MainWindow.process.AUTO)
                {
                    metrics_label.Content = context.getMessages().perimeter + " = " + Math.Round(thicknessPerimeter[slider_value], 2) + " " + context.getMessages().mm + Environment.NewLine +
                                                          context.getMessages().area + " = " + Math.Round(thicknessArea[slider_value], 2) + " " + context.getMessages().mmB2 + Environment.NewLine +
                                                            context.getMessages().meanThickness + " = " + Math.Round(meanThickness[slider_value], 2) + " " + context.getMessages().mm;
                }
                else //is MANUAL
                {
                    metrics_label.Content = context.getMessages().perimeter + " = " + Math.Round(thicknessPerimeter[slider_value], 2) + " " + context.getMessages().mm + Environment.NewLine +
                                                           context.getMessages().area + " = " + Math.Round(thicknessArea[slider_value], 2) + " " + context.getMessages().mmB2;
                }
                //metrics_label.Content = messages.meanThickness + " = " + Math.Round(meanThickness[slider_value], 2) + " " + messages.mm;
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
                         displayMetrics(true);
                    }else if (contourSeg == ContourSegmentation.MANUAL)
                    {
                        clear_canvas();
                        thickness[slider_value].Add(point);
                        display();
                        displayMetrics(true);
                    }
                }
                else if (e.ChangedButton == MouseButton.Right && contourSeg == ContourSegmentation.MANUAL)
                {
                    if (thickness[slider_value].Any())
                    {
                        thickness[slider_value].RemoveAt(thickness[slider_value].Count - 1);
                        displayMetrics(true);
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
               displayMetrics(true);
                return;
            }
            thickness[slider_value].RemoveAll(item => pointsToRemove.Contains(item));
            if (!contourSeg.Equals(ContourSegmentation.MANUAL) && count > 0) doFillPoints();


            pointsToRemove.Clear();
            displayMetrics();
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

        private void fitUIAccordingToDicomImageSize(double height, double width)
        {
            Thickness waitMargin = Wait.Margin;
            Thickness frameNumMargin = frame_num_label.Margin;
            Thickness MetricsLabelMargin = metrics_label.Margin;
            Thickness OXYButtonsMargin = OXY_Buttons.Margin;

            frameNumMargin.Left = 665 - Math.Abs(736 - width);
            MetricsLabelMargin.Left = 580 - Math.Abs(736 - width);
            OXYButtonsMargin.Left = 339 - Math.Abs(736 - width);
            waitMargin.Left = width / 2 - Wait.Width / 2;//344.426 - Math.Abs(736 - width);

            frame_num_label.Margin = frameNumMargin;
            metrics_label.Margin = MetricsLabelMargin;
            OXY_Buttons.Margin = OXYButtonsMargin;
            Wait.Margin = waitMargin;


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


        public void startSpinner()
        {
            ((Storyboard)FindResource("WaitStoryboard")).Begin();
            context.getMainWindow().Rat.IsEnabled = false;
            Wait.Visibility = Visibility.Visible;
        }

        public void stopSpinner()
        {
            ((Storyboard)FindResource("WaitStoryboard")).Stop();
            context.getMainWindow().Rat.IsEnabled = true;
            Wait.Visibility = Visibility.Hidden;
        }

        public void doCorrection()
        {
            this.switch_auto_manual.doCorrectionState();
            contourSeg = ContourSegmentation.CORRECTION;
            displayMetrics();
            clear_canvas();
            display();
        }

        public void doManual()
        {
            this.switch_auto_manual.doManualState();
            contourSeg = ContourSegmentation.MANUAL;
            if (areTherePoints()) thickness[slider_value].Clear();
            displayMetrics();
            clear_canvas();
            display();
        }

        public void doFillPoints()
        {
            this.switch_auto_manual.doFillPointState();
            contourSeg = ContourSegmentation.FILL_POINTS;
            displayMetrics();
            clear_canvas();
            display();
        }

        //--------------------------------------M A N A G E - P O I N T S---------------------------------

        //Convert Point to EDITCore.CVPoint //for many contours
        List<List<Point>> editCVPointToWPFPoint(List<List<EDITCore.CVPoint>> cvp)
        { 
               thickness.Clear();
            //thickness = new List<List<Point>>();
            //bladderPerimeter = new List<double>();

            List<List<Point>> points = new List<List<Point>>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                points.Add(new List<Point>(cvp[0].Count));
            }
            int count = context.getUltrasoundPart().getStartingFrame();
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

            return points;
        }

       
        //Convert EDITCore.CVPoint to Point
        public List<List<EDITCore.CVPoint>> WPFPointToCVPoint(List<List<Point>> points)
        {
            List<List<EDITCore.CVPoint>> cvp = new List<List<EDITCore.CVPoint>>();
            string message = context.getCheck().checkForSegmentationGaps(points);
            if (message != null)
            {
                CustomMessageBox.Show(message, context.getMessages().warning, MessageBoxButton.OK);
                return cvp;
            }

            thicknessCvPoints.Clear();
            List<EDITCore.CVPoint> contour = new List<EDITCore.CVPoint>();
            //thicknessCvPoints = new List<List<EDITCore.CVPoint>>();
           
            //for (int i = ultrasound.getStartingFrame(); i <= ultrasound.getEndingFrame(); i++)
            for (int i = 0; i < thickness.Count; i++)
            {
                if (thickness[i].Any())
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

        private void Oxy_deoxy_switch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (currentOxyDeOxyState)
            {
                case OxyDeOxyState.OXY:
                    doDeOXYState();
                    break;
                case OxyDeOxyState.DEOXY:
                    doOXYState();
                    break;
            }
        }

        private void doOXYState()
        {
            oxy_deoxy_label.Content = "  " + context.getMessages().oxy;
            oxy_deoxy_switch.setCustomDotToLeft();
            currentOxyDeOxyState = OxyDeOxyState.OXY;
            if (OXYDicomFile!=null)
            {
                BitmapFromPath(OXYimagesDir + Path.DirectorySeparatorChar + slider_value + ".bmp");
                frame_num_label.Content = context.getMessages().frame + ":" + " " + slider_value;
                makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Visible);
                DeOXY_studyname_label.Visibility = Visibility.Hidden;
                OXY_studyname_label.Visibility = Visibility.Visible;
                switch_auto_manual.Visibility = Visibility.Visible;
                manage2DObect.Visibility = Visibility.Visible;
            }
            else
            {
                image.Source = null;
                frame_num_label.Content = "";
                OXY_studyname_label.Content = "";
                DeOXY_studyname_label.Content = "";
                makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Hidden);
                switch_auto_manual.Visibility = Visibility.Hidden;
                manage2DObect.Visibility = Visibility.Hidden;
            }
            clear_canvas();
            display();
        }

        private void doDeOXYState()
        {
            oxy_deoxy_label.Content = context.getMessages().deoxy;
            oxy_deoxy_switch.setCustomDotToRight();
            currentOxyDeOxyState = OxyDeOxyState.DEOXY;
            if (DeOXYDicomFile!=null)
            {
                BitmapFromPath(deOXYimagesDir + Path.DirectorySeparatorChar + slider_value + ".bmp");
                frame_num_label.Content = context.getMessages().frame + ":" + " " + slider_value;
                makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Visible);
                OXY_studyname_label.Visibility = Visibility.Hidden;
                DeOXY_studyname_label.Visibility = Visibility.Visible;
                switch_auto_manual.Visibility = Visibility.Visible;
                manage2DObect.Visibility = Visibility.Visible;
            }
            else
            {
                image.Source = null;
                frame_num_label.Content = "";
                DeOXY_studyname_label.Content = "";
                OXY_studyname_label.Content = "";
                makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Hidden);
                switch_auto_manual.Visibility = Visibility.Hidden;
                manage2DObect.Visibility = Visibility.Hidden;
            }
            clear_canvas();
            display();
        }

        private void makeVisibeOrUnvisibleSliderLeftTickBar(Visibility visibility)
        {
            if (context.getUltrasoundPart().ultrasound_slider.IsLoaded)
            {
                UIElement ultrasoundSliderLeftTickBar = (UIElement)context.getUltrasoundPart().ultrasound_slider.Template.FindName("BottomTick", context.getUltrasoundPart().ultrasound_slider);
                ultrasoundSliderLeftTickBar.Visibility = visibility;
            }
        }
        //-----------------------------------------------


        void fiillMetrics(List<double> values)
        {
            int start = context.getUltrasoundPart().getStartingFrame();
            int end = context.getUltrasoundPart().getEndingFrame();
            for (int i=start; i<=end; i++)
            {
                meanThickness[i] = values[i - start];
                thicknessArea[i] = context.getMetrics().calulateArea(thickness[i]);
                thicknessPerimeter[i] = context.getMetrics().calulatePerimeter(thickness[i]);
            }

        }

        //------- SOME GETTERS---------------

        public List<List<Point>> getThicknessPoints()
        {
            return thickness;
        }

        public List<double> getMeanThickness()
        {
            return meanThickness;
        }
        //-----------------------------------------

    }

        
}
