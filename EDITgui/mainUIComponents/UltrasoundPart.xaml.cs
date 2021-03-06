﻿using Emgu.CV;
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
using System.Windows.Markup;
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
        public delegate void zoomUltrasoundChangedHandler(List<Matrix> obj);
        public static event zoomUltrasoundChangedHandler zoomUltrasoundChanged = delegate { };

        public delegate void STLBladderHandler(EDITgui.Geometry geometry);
        public static event STLBladderHandler returnBladderSTL = delegate { };

        public delegate void STLSkinHandler(EDITgui.Geometry geometry);
        public static event STLSkinHandler returnSkinSTL = delegate { };



        private bool registration = false;

        public enum ContourSegmentation
        {
            INSERT_USER_POINTS,
            CORRECTION,
            MANUAL,
            FILL_POINTS,
        }
        //--------Frame Marker---------
        FrameMarker startingFrameMarker;
        FrameMarker endingFrameMarker;
        //-----------------------------

        Point registrationPoint;
        Point zeroPoint = new Point(0, 0);

        List<EDITCore.CVPoint> contourForFix = new List<EDITCore.CVPoint>();

        //-----------for slider----------
        Slider slider;
        public int slider_value = 0;
        public int fileCount;
        public string ultrasoundDicomFile = null;
        public string imagesDir;
        //-------------------------------

        //------------user set intial points on images------
        public List<Point> userPoints = new List<Point>(2);//
        public int startingFrame = -1;
        public int endingFrame = -1;
        //--------------------------------------------------

        //initialazation
        public ContourSegmentation contourSeg = ContourSegmentation.CORRECTION;

        List<double> pixelSpacing = new List<double>(); //x=pixelSpacing[0] y=pixelSpacing[1]
        List<double> imageSize = new List<double>(); //x=pixelSpacing[0] y=pixelSpacing[1]
        double calibration_x;
        double calibration_y;

        List<Point> points = new List<Point>();
        List<Polyline> polylines = new List<Polyline>();
        List<List<EDITCore.CVPoint>> bladderCvPoints = new List<List<EDITCore.CVPoint>>();

        public String bladderGeometryPath = null;

        //create main object
        Context context;

        public UltrasoundPart()
        {
            InitializeComponent();
            PhotoAcousticPart.zoomPhotoAccousticChanged += OnPhotoAccousticZoomChanged;
            contourSeg = ContourSegmentation.INSERT_USER_POINTS;
        }

        public UltrasoundPart(Context context)
        {
            InitializeComponent();
            this.context = context;
            PhotoAcousticPart.zoomPhotoAccousticChanged += OnPhotoAccousticZoomChanged;
            applicationGrid.Children.Add(context.getUltrasoundData()); //add tumor annotation option here
            //applicationGrid.Children.Add(context.getRegistration()); //add tumor annotation option here
            doInsertUserPoints();
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
            openFileDialog.Title = context.getMessages().selectDicom;
            if (openFileDialog.ShowDialog() == true)
            {
                startSpinner();
                clear_canvas();
                context.getCore().repeatSegmentation();
                ultrasoundDicomFile = null;
               
                await Task.Run(() => {
                    string dicomPath = context.getStudyFile().copyFileToWorkspace(context.getStudyFile().getWorkspaceDicomPath(), openFileDialog.FileName, StudyFile.FileType.UltrasoundDicomFile);
                    imagesDir = context.getCore().exportImages(dicomPath);
                    pixelSpacing = context.getCore().pixelSpacing;
                    imageSize = context.getCore().imageSize;
                });
                if (imagesDir != null)
                {
                    context.getMetrics().setPixelSpacing(pixelSpacing);
                    context.getStudySettings().setImageDimsAndPixelSpacing(pixelSpacing, imageSize);
                    fitUIAccordingToDicomImageSize(imageSize[1], imageSize[0]);
                    ultrasoundDicomFile = openFileDialog.FileName;
                    fileCount = Directory.GetFiles(imagesDir, "*.bmp", SearchOption.AllDirectories).Length;
                    BitmapFromPath(imagesDir + Path.DirectorySeparatorChar + "0.bmp");
                    ultrasound_studyname_label.Content = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    frame_num_label.Content = context.getMessages().frame + ":" + " " + "0";
                    slider.Value = 0;
                    context.getPhotoAcousticPart().sliderValueChanged(slider_value);// here we pass slider_value to Photoaccoustic part 
                    slider.TickFrequency = 1 / (double)fileCount;
                    slider.Minimum = 0;
                    slider.Maximum = fileCount - 1;
                    slider.TickFrequency = 1;
                    slider.Visibility = Visibility.Visible;
                    switch_auto_manual.Visibility = Visibility.Visible;
                    context.getUltrasoundData().Visibility = Visibility.Visible;
                   // context.getRegistration().Visibility = Visibility.Visible;
                    calibration_x = image.Source.Width / canvasUltrasound.Width;
                    calibration_y = image.Source.Height / canvasUltrasound.Height;
                }
                else
                {
                    ultrasound_studyname_label.Content = "";
                    frame_num_label.Content = "";
                    metrics_label.Content = "";
                    image.Source = null;
                    switch_auto_manual.Visibility = Visibility.Hidden;
                    slider.Visibility = Visibility.Hidden;
                    context.getUltrasoundData().Visibility = Visibility.Collapsed;
                  //  context.getRegistration().Visibility = Visibility.Collapsed;
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
            context.getMetrics().setPixelSpacing(this.pixelSpacing);
            context.getStudySettings().setImageDimsAndPixelSpacing(this.pixelSpacing, imageSize);
            fitUIAccordingToDicomImageSize(this.imageSize[1], this.imageSize[0]);
            fileCount = Directory.GetFiles(imagesDir, "*.bmp", SearchOption.AllDirectories).Length;
            BitmapFromPath(imagesDir + Path.DirectorySeparatorChar + "0.bmp");
            ultrasound_studyname_label.Content = studyName + " " + context.getMessages().ultrasound;
            frame_num_label.Content = "Frame:" + " " + "0";
            slider.Value = 0;
            context.getPhotoAcousticPart().sliderValueChanged(slider_value);// here we pass slider_value to Photoaccoustic part 
            slider.TickFrequency = 1 / (double)fileCount;
            slider.Minimum = 0;
            slider.Maximum = fileCount - 1;
            slider.TickFrequency = 1;
            slider.Visibility = Visibility.Visible;
            switch_auto_manual.Visibility = Visibility.Visible;
            context.getUltrasoundData().Visibility = Visibility.Visible;
            //context.getRegistration().Visibility = Visibility.Visible;
            calibration_x = image.Source.Width / canvasUltrasound.Width;
            calibration_y = image.Source.Height / canvasUltrasound.Height;
            doRepeatProcess();
        }

        private void initializeBladderList()
        {
            context.getImages().InitiallizeFrames(fileCount);

            //initialize both image modality tumors
            context.getUltrasoundData().initializeTumors(); 
            context.getPhotoAcousticData().initializeTumors();
        }

        public void addRegistrationPoint()
        {
            this.registration = true;
        }

        private async void Extract_bladder_Click(object sender, RoutedEventArgs e)
        {
            string message = context.getCheck().getMessage(checkBeforeExecute.executionType.extract2DBladder);
            if (message != null)
            {
                CustomMessageBox.Show(message, context.getMessages().warning, MessageBoxButton.OK);
                return;
            }

            startSpinner();

            AlgorithmSettings sets = new AlgorithmSettings();

            sets.repeats = int.Parse(Repeats.Text);
            sets.smoothing = int.Parse(Smoothing.Text);
            sets.lamda1 = double.Parse(Lamda1.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            sets.lamda2 = double.Parse(Lamda2.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            sets.levelSize = int.Parse(LevelsetSize.Text);
            sets.filtering = chechBox_FIltering.IsChecked.Value;
            sets.probeArtifactCorrection = chechBox_ArtifactCorrection.IsChecked.Value;

            context.getImages().setUltrasoundFrameSettingsOfAllSequence(sets);


            //the code bellow is to avoid an issue when user marks the starting frame after the ending frame
            if (endingFrame < startingFrame)
            {
                int temp = startingFrame;
                startingFrame = endingFrame;
                endingFrame = temp;
            } 

            await Task.Run(() => {
                bladderCvPoints = context.getCore().Bladder2DExtraction(sets.repeats, sets.smoothing, sets.lamda1, sets.lamda2, sets.levelSize, sets.filtering, startingFrame, endingFrame, userPoints, sets.probeArtifactCorrection);
                if (!bladderCvPoints.Any()) return; 
                context.getImages().fillBladderFromBackEnd(bladderCvPoints, this.startingFrame);
            });
            doCorrection(true, true);
            stopSpinner();
        }


        private async void Recalculate_Click(object sender, RoutedEventArgs e)
        {
            string message = context.getCheck().getMessage(checkBeforeExecute.executionType.recalculateBladder);
            if (message != null)
            {
                CustomMessageBox.Show(message, context.getMessages().warning, MessageBoxButton.OK);
                return;
            }

            AlgorithmSettings sets = new AlgorithmSettings();

            sets.repeats = int.Parse(Repeats.Text);
            sets.smoothing = int.Parse(Smoothing.Text);
            sets.lamda1 = double.Parse(Lamda1.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            sets.lamda2 = double.Parse(Lamda2.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            sets.levelSize = int.Parse(LevelsetSize.Text);
            sets.filtering = chechBox_FIltering.IsChecked.Value;
            sets.probeArtifactCorrection = chechBox_ArtifactCorrection.IsChecked.Value;

            await Task.Run(() =>
            {
                contourForFix = context.getCore().recalculateBladderOfContour(sets.repeats, sets.smoothing, sets.lamda1, sets.lamda2, sets.levelSize, sets.filtering, slider_value, context.getImages().getUniqueFrameCVPoints(context.getImages().getBladderPoints()), sets.probeArtifactCorrection);
                if (!contourForFix.Any()) return;
                context.getImages().fillUniqueFrameBladderFromBackEnd(contourForFix);
            });
            doCorrection(true, true);
            stopSpinner();

        }


        private void Repeat_process_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = CustomMessageBox.Show(context.getMessages().makeUserAwareOfRepeatProcess, context.getMessages().warning, MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                doRepeatProcess();
            }
        }

        public void doRepeatProcess()
        {
            userPoints.Clear();
            initializeBladderList();
            //metrics_label.Visibility = Visibility.Hidden;
            context.getCore().repeatSegmentation();
            startingFrame = -1;
            endingFrame = -1;
            doInsertUserPoints();
            //context.getRegistration().initializeRegistrationPoints();
            context.getMainWindow().cleanVTKRender();
            context.getPhotoAcousticPart().doRepeatProcess(); //trigger repeat process of photoAcousticPart
        }

        private async void Extract_STL_Click(object sender, RoutedEventArgs e)
        {
            string message = context.getCheck().getMessage(checkBeforeExecute.executionType.extract3DBladder);
            if (message != null)
            {
                CustomMessageBox.Show(message, context.getMessages().warning, MessageBoxButton.OK);
                return;
            }

            bladderCvPoints = context.getImages().getAllFramesCVPoints(context.getImages().getTotalBladderPoints());

            if (!bladderCvPoints.Any()) return;
            startSpinner();
            await Task.Run(() => {
                bladderGeometryPath = context.getCore().extractBladderSTL(bladderCvPoints);
            });
            
            if (bladderGeometryPath != null)
            {
                EDITgui.Geometry bladderGeometry = new Geometry() { geometryName = Messages.bladderGeometry, Path = bladderGeometryPath, actor = null };
                returnBladderSTL(bladderGeometry);
            }
            stopSpinner();
        }

        private async void Export_Skin_Click(object sender, RoutedEventArgs e)
        {
            string message = context.getCheck().getMessage(checkBeforeExecute.executionType.extract3DLayer);
            if (message != null)
            {
                CustomMessageBox.Show(message, context.getMessages().warning, MessageBoxButton.OK);
                return;
            }

           // bladderCvPoints = context.getImages().getAllFramesCVPoints(context.getImages().getTotalBladderPoints());
            bladderCvPoints = context.getImages().getAllFramesCVPoints(context.getImages().getTotalThicknessPoints());
            if (!bladderCvPoints.Any()) return;
            startSpinner();
            String STLPath = null;
            await Task.Run(() => {
                STLPath = context.getCore().extractSkin(bladderCvPoints);
            });
            EDITgui.Geometry skinGeometry = new Geometry() { geometryName = Messages.layerGeometry, Path = STLPath, actor = null };
            if(STLPath != null) returnSkinSTL(skinGeometry);
            stopSpinner();
        }


        private void Ultrasound_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                saveFrameAlgorithmSetting();
                slider_value = (int)slider.Value;
                BitmapFromPath(imagesDir + Path.DirectorySeparatorChar + slider_value.ToString() + ".bmp");
                frame_num_label.Content = context.getMessages().frame + ":" + " " + slider_value.ToString();
                switch (context.getMainWindow().currentMode)
                {
                    case MainWindow.Mode.AUTO:
                        if (userPoints.Count == 2){
                            doCorrection(false,false);
                        }
                        break;
                    case MainWindow.Mode.ANOTATION:
                        doCorrection(false,false);
                        break;
                }

                context.getPhotoAcousticPart().sliderValueChanged(slider_value); //here we pass slider_value to Photoaccoustic part
                context.getUltrasoundData().updatePanel(slider_value);
                context.getPhotoAcousticData().updatePanel(slider_value);

                updateFrameAlgorithmSetting();
                updateCanvas();
            }
            catch(Exception ex){
                //TO DO
            }
        }

        private void Ultrasound_slider_Initialized(object sender, EventArgs e)
        {
            slider = sender as Slider;
        }


        public void saveFrameAlgorithmSetting()
        {
            context.getImages().getCurrentFrameSettings().repeats = int.Parse(Repeats.Text);
            context.getImages().getCurrentFrameSettings().smoothing = int.Parse(Smoothing.Text);
            context.getImages().getCurrentFrameSettings().lamda1 = double.Parse(Lamda1.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            context.getImages().getCurrentFrameSettings().lamda2 = double.Parse(Lamda2.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            context.getImages().getCurrentFrameSettings().levelSize = int.Parse(LevelsetSize.Text);
            context.getImages().getCurrentFrameSettings().filtering = chechBox_FIltering.IsChecked.Value;
            context.getImages().getCurrentFrameSettings().probeArtifactCorrection = chechBox_ArtifactCorrection.IsChecked.Value;


            context.getImages().getCurrentFrameSettings().minThickness = double.Parse(context.getPhotoAcousticPart().minThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            context.getImages().getCurrentFrameSettings().maxThickness = double.Parse(context.getPhotoAcousticPart().maxThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            context.getImages().getCurrentFrameSettings().majorThicknessExistence = context.getPhotoAcousticPart().big_tumor.IsChecked.Value;
        }

        public void updateFrameAlgorithmSetting()
        {
            Repeats.Text = context.getImages().getCurrentFrameSettings().repeats.ToString();
            Smoothing.Text = context.getImages().getCurrentFrameSettings().smoothing.ToString();
            Lamda1.Text = string.Format("{0:0.0}", context.getImages().getCurrentFrameSettings().lamda1);
            Lamda2.Text = string.Format("{0:0.0}", context.getImages().getCurrentFrameSettings().lamda2);
            LevelsetSize.Text = context.getImages().getCurrentFrameSettings().levelSize.ToString();
            chechBox_FIltering.IsChecked = context.getImages().getCurrentFrameSettings().filtering;
            chechBox_ArtifactCorrection.IsChecked = context.getImages().getCurrentFrameSettings().probeArtifactCorrection;

            context.getPhotoAcousticPart().minThickness.Text = string.Format("{0:0.0}", context.getImages().getCurrentFrameSettings().minThickness);
            context.getPhotoAcousticPart().maxThickness.Text = string.Format("{0:0.0}", context.getImages().getCurrentFrameSettings().maxThickness);
            context.getPhotoAcousticPart().big_tumor.IsChecked = context.getImages().getCurrentFrameSettings().majorThicknessExistence;
        }

        //----------------------------------------------------------CANVAS OPERATIONS--------------------------------------------------------
        private void draw_points(Selected2DObject contour)
        {
            for (int i = 0; i < contour.points.Count; i++)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Fill = contour.polylineColor;
                ellipse.Width = ViewAspects.pointSize;
                ellipse.Height = ViewAspects.pointSize;
                canvasUltrasound.Children.Add(ellipse);
                Canvas.SetLeft(ellipse, contour.points.ElementAt(i).X);
                Canvas.SetTop(ellipse, contour.points.ElementAt(i).Y);
            }

        }

        private void draw_polyline(Selected2DObject contour)
        {
            List<Point> closeCurvePoints = contour.points.ToList();//new List<Point>();

            switch (contourSeg)
            {
                case ContourSegmentation.CORRECTION:
                  if(closeCurvePoints.Any())  closeCurvePoints.Add(closeCurvePoints[0]);
                    break;
                case ContourSegmentation.FILL_POINTS:
                    if (indexA != 0 && closeCurvePoints.Any())
                    {
                        closeCurvePoints.Add(closeCurvePoints[0]);
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
                    pl.Stroke = contour.polylineColor;
                    pl.StrokeStartLineCap = PenLineCap.Round;
                    pl.StrokeEndLineCap = PenLineCap.Round;
                    canvasUltrasound.Children.Add(pl);
                    polylines.Add(pl);
                }
            }
            closeCurvePoints.Clear();
        }

        private void drawPolylineOfRest2DObjects()
        {
            List<UIElement> list = context.getUltrasoundData().getNoSelectedObject2DPolylines();
            foreach(UIElement pl in list)
            {
                canvasUltrasound.Children.Add(pl);
            }
            list.Clear();
        }

        private void canvasUltrasound_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!contourSeg.Equals(ContourSegmentation.CORRECTION) && !registration)
            {
                Point point = e.GetPosition(image); //position relative to the image

                if (e.ChangedButton == MouseButton.Left)
                {
                    if (context.getMainWindow().currentMode == MainWindow.Mode.AUTO && contourSeg == ContourSegmentation.INSERT_USER_POINTS && userPoints.Count < 2)
                    {
                        clear_canvas();
                        points.Clear();

                        if (userPoints.Count == 0)
                        {
                            canvasUltrasound.Children.Add(getStartingFrameMarker());
                            Canvas.SetLeft(startingFrameMarker, point.X );
                            Canvas.SetTop(startingFrameMarker, point.Y );
                            userPoints.Add(point);
                            startingFrame = slider_value;
                        }
                        else if (userPoints.Count > 0 && userPoints.Count < 2)
                        {
                            canvasUltrasound.Children.Add(getEndingFrameMarker());
                            Canvas.SetLeft(endingFrameMarker, point.X);
                            Canvas.SetTop(endingFrameMarker, point.Y);
                            userPoints.Add(point);
                            endingFrame = slider_value;
                        }
                        clear_canvas();
                        display();
                    }
                    else if (contourSeg == ContourSegmentation.FILL_POINTS)
                    {
                        clear_canvas();
                        if (indexA > context.getUltrasoundData().getSelectedObject2D().points.Count - 1) indexA = 0;

                        double d1, d2;
                        if (indexA != 0)
                        {
                            d1 = Math.Sqrt(Math.Pow(point.X - context.getUltrasoundData().getSelectedObject2D().points[indexA - 1].X, 2) + Math.Pow(point.Y - context.getUltrasoundData().getSelectedObject2D().points[indexA - 1].Y, 2));
                        }
                        else
                        {
                            int num = context.getUltrasoundData().getSelectedObject2D().points.Count - 1;
                            d1 = Math.Sqrt(Math.Pow(point.X - context.getUltrasoundData().getSelectedObject2D().points[num].X, 2) + Math.Pow(point.Y - context.getUltrasoundData().getSelectedObject2D().points[num].Y, 2));
                        }
                        d2 = Math.Sqrt(Math.Pow(point.X - context.getUltrasoundData().getSelectedObject2D().points[indexA].X, 2) + Math.Pow(point.Y - context.getUltrasoundData().getSelectedObject2D().points[indexA].Y, 2));

                        if (d2 > d1)
                        {
                            context.getUltrasoundData().getSelectedObject2D().points.Insert(indexA++, point);
                        }
                        else
                        {
                            context.getUltrasoundData().getSelectedObject2D().points.Insert(indexA, point);
                        }
                        display();
                    }
                    else if (contourSeg == ContourSegmentation.MANUAL)
                    {
                        context.getUltrasoundData().getSelectedObject2D().points.Add(point);
                        updateCanvas();
                    }
                    context.getUltrasoundData().updateSelectedObjectMetrics();
                }
                else if (e.ChangedButton == MouseButton.Right && contourSeg == ContourSegmentation.MANUAL)
                {
                    if (context.getUltrasoundData().getSelectedObject2D().points.Any())
                    {
                        context.getUltrasoundData().getSelectedObject2D().points.RemoveAt(context.getUltrasoundData().getSelectedObject2D().points.Count - 1);
                        context.getUltrasoundData().updateSelectedObjectMetrics();
                        if (contourSeg == ContourSegmentation.MANUAL)
                        {
                            clear_canvas();
                        }
                        else if (contourSeg == ContourSegmentation.CORRECTION)
                        {
                            Ellipse ellipse = new Ellipse();
                            ellipse.Width = ViewAspects.pointSize;
                            ellipse.Height = ViewAspects.pointSize;
                            canvasUltrasound.Children.Remove(ellipse);
                        }
                        display();
                    }
                }
                context.getSaveActions().dataUpdatedWithoutSave();
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
                    context.getSaveActions().dataUpdatedWithoutSave();
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
                        int j = context.getUltrasoundData().getSelectedObject2D().points.IndexOf(initialPoisition1);
                        context.getUltrasoundData().getSelectedObject2D().points[j] = final_position;
                        context.getUltrasoundData().updateSelectedObjectMetrics();
                        context.getPhotoAcousticPart().updateCanvas();
                        //update_centerline();
                    }
                    catch { }

                    updateCanvas();

                    context.getSaveActions().dataUpdatedWithoutSave();
                }
            }
        }

        Point _start;
        Rectangle rectRemovePoints;
        private void canvasUltrasound_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!selectedObjectHasPoints() || contourSeg == ContourSegmentation.MANUAL) return;

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
            if (!selectedObjectHasPoints() || contourSeg == ContourSegmentation.MANUAL) return;

            try
            {
                List<Point> pointsToRemove = new List<Point>();
                int count = 0;
                for (int i = 0; i < context.getUltrasoundData().getSelectedObject2D().points.Count; i++)
                {
                    if (context.getUltrasoundData().getSelectedObject2D().points[i].X >= Canvas.GetLeft(rectRemovePoints) && context.getUltrasoundData().getSelectedObject2D().points[i].Y >= Canvas.GetTop(rectRemovePoints) &&
                        context.getUltrasoundData().getSelectedObject2D().points[i].X <= Canvas.GetLeft(rectRemovePoints) + rectRemovePoints.Width && context.getUltrasoundData().getSelectedObject2D().points[i].Y <= Canvas.GetTop(rectRemovePoints) + rectRemovePoints.Height)
                    {
                        pointsToRemove.Add(context.getUltrasoundData().getSelectedObject2D().points[i]);
                        if (count == 0) indexA = i;
                        count++;
                    }

                }

                canvasUltrasound.Children.Remove(rectRemovePoints);
                if (count == context.getUltrasoundData().getSelectedObject2D().points.Count)
                {
                    doManual();
                    context.getUltrasoundData().updateSelectedObjectMetrics();
                    return;
                }
                context.getUltrasoundData().getSelectedObject2D().points.RemoveAll(item => pointsToRemove.Contains(item));
                if (!contourSeg.Equals(ContourSegmentation.MANUAL) && count > 0) doFillPoints();
                if (pointsToRemove.Any()) context.getSaveActions().dataUpdatedWithoutSave();
                pointsToRemove.Clear();
            }
            catch(Exception ex)
            {

            }
        }


        private void canvasUltrasound_MouseMove(object sender, MouseEventArgs e)
        {
            if (!selectedObjectHasPoints() || contourSeg.Equals(ContourSegmentation.MANUAL)) return;

            if (e.RightButton == MouseButtonState.Pressed)
            {
                try
                {
                    var mouse = Mouse.GetPosition(image);
                    Canvas.SetLeft(rectRemovePoints, _start.X > mouse.X ? mouse.X : _start.X);
                    Canvas.SetTop(rectRemovePoints, _start.Y > mouse.Y ? mouse.Y : _start.Y);

                    rectRemovePoints.Width = Math.Abs(mouse.X - _start.X);
                    rectRemovePoints.Height = Math.Abs(mouse.Y - _start.Y);
                }catch(Exception ex)
                {

                } 
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

        public void updateCanvas()
        {
            clear_canvas();
            display();
        }

        public void clear_canvas()
        {
            canvasUltrasound.Children.Clear();
            canvasUltrasound.Children.Add(image);
        }

        public void lazy_clear_canvas()
        {
            canvasUltrasound.Children.Clear();
            canvasUltrasound.Children.Add(image);
        }

        public void display()
        {
            if(image.Source != null)
            {
                Selected2DObject selectedObject = context.getUltrasoundData().getSelectedObject2D();
                drawPolylineOfRest2DObjects();
                draw_polyline(selectedObject);
                draw_points(selectedObject);
                metrics_label.Foreground = selectedObject.polylineColor;
                if (metrics_label.IsVisible)
                {
                    int a = 7;
                }

                metrics_label.Content = selectedObject.metrics;

                if (slider_value == startingFrame && !context.getImages().getBladderPoints().Any() && userPoints.Count > 0)
                {
                    canvasUltrasound.Children.Add(getStartingFrameMarker());
                    Canvas.SetLeft(startingFrameMarker, userPoints[0].X);
                    Canvas.SetTop(startingFrameMarker, userPoints[0].Y);
                }
                if (slider_value == endingFrame && !context.getImages().getBladderPoints().Any() && userPoints.Count > 1)
                {
                    canvasUltrasound.Children.Add(getEndingFrameMarker());
                    Canvas.SetLeft(endingFrameMarker, userPoints[1].X);
                    Canvas.SetTop(endingFrameMarker, userPoints[1].Y);
                }
            }
        }

        public void eraseImageView()
        {
            canvasUltrasound.Children.Clear();
            ultrasound_studyname_label.Content = "";
            frame_num_label.Content = "";
            metrics_label.Content = "";
            image.Source = null;
            switch_auto_manual.Visibility = Visibility.Hidden;
            slider.Visibility = Visibility.Hidden;
            context.getUltrasoundData().Visibility = Visibility.Collapsed;
        }


        private FrameMarker getStartingFrameMarker()
        {
            if (startingFrameMarker == null)
            {
                startingFrameMarker = new FrameMarker(FrameMarker.Type.starting);
            }
            
            return startingFrameMarker;
        }


        private FrameMarker getEndingFrameMarker()
        {
            if (endingFrameMarker == null)
            {
                endingFrameMarker = new FrameMarker(FrameMarker.Type.ending);
            }
            return endingFrameMarker;
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
            Thickness waitMargin = Wait.Margin;
            Thickness tumorsPanelMargin = context.getUltrasoundData().Margin;

            ImageRectangleMargin.Left = value;
            ImageBorderMargin.Left = value + 1;
            waitMargin.Left = 736 - width / 2 - Wait.Width / 2;
            tumorsPanelMargin.Left = value + 10;

            image_Rectangle.Margin = ImageRectangleMargin;
            imageborder.Margin = ImageBorderMargin;
            Wait.Margin = waitMargin;
            context.getUltrasoundData().Margin = tumorsPanelMargin;

            frame_actions_infos.Width = width;
            ultrasound_Buttons.Width = width;

            image_Rectangle.Width = width + 2;
            imageborder.Width = width;
            image.Width = width;
            image.MinWidth = width;

        }

        private void Switch_auto_manual_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.contourSeg == ContourSegmentation.MANUAL)
            {
                context.getUltrasoundData().updateSelectedObjectMetrics();
                doCorrection();
            }
            else if (this.contourSeg == ContourSegmentation.CORRECTION)
            {
                doManual();
            }
            else if (this.contourSeg == ContourSegmentation.FILL_POINTS)
            {
                context.getUltrasoundData().updateSelectedObjectMetrics();
                doCorrection();
            }
        }

        private void ClosedSurface_Click(object sender, RoutedEventArgs e)
        {
            context.getCore().fillHoles = closedSurface.IsChecked.Value;
        }

        public void startSpinner()
        {
            ((Storyboard)FindResource("WaitStoryboard")).Begin();
            //applicationGrid.IsEnabled = false;
            context.getMainWindow().Panel2D.IsEnabled = false;
            Wait.Visibility = Visibility.Visible;
        }

        public void stopSpinner()
        {
            ((Storyboard)FindResource("WaitStoryboard")).Stop();
            //applicationGrid.IsEnabled = true;
            context.getMainWindow().Panel2D.IsEnabled = true;
            Wait.Visibility = Visibility.Hidden;
            context.getSaveActions().dataUpdatedWithoutSave();
        }

        public void doCorrection(bool update = true, bool updatePhotoAcoustic = true)
        {
            this.switch_auto_manual.doCorrectionState();
            contourSeg = ContourSegmentation.CORRECTION;
            metrics_label.Visibility = Visibility.Visible;
            if(update) updateCanvas();
            if(updatePhotoAcoustic) context.getPhotoAcousticPart().updateCanvas();
        }

        public void doManual()
        {
           this.switch_auto_manual.doManualState();
            contourSeg = ContourSegmentation.MANUAL;
            metrics_label.Visibility = Visibility.Hidden;
            if (selectedObjectHasPoints()) context.getUltrasoundData().getSelectedObject2D().points.Clear();
            updateCanvas();
            context.getPhotoAcousticPart().updateCanvas();
        }

        public void doFillPoints()
        {
            this.switch_auto_manual.doFillPointState();
            contourSeg = ContourSegmentation.FILL_POINTS;
            metrics_label.Visibility = Visibility.Hidden;
            updateCanvas();
        }

        public void doInsertUserPoints()
        {
            this.switch_auto_manual.doCorrectionState();
            if(context.getMainWindow().currentMode == MainWindow.Mode.ANOTATION)
            {
                contourSeg = ContourSegmentation.CORRECTION;
            }
            else
            {
                contourSeg = ContourSegmentation.INSERT_USER_POINTS;
            }

            updateCanvas();
        }


        public bool areTherePoints()
        {
            return (context.getImages().getBladderPoints().Any());
        }

        public bool selectedObjectHasPoints()
        {
            return (context.getUltrasoundData().getSelectedObject2D().points.Any());
        }


        public bool sliderIsBetwweenFrameOfInterest()
        {
            return (slider_value >= startingFrame && slider_value <= endingFrame);
        }

        //------- SOME GETTERS---------------

        public int getStartingFrame()
        {
            return startingFrame;
        }

        public int getEndingFrame()
        {
            return endingFrame;
        }

        public bool autoExecutionUserPointsWereSet()
        {
            return (context.getMainWindow().currentMode == MainWindow.Mode.AUTO && startingFrame != -1 && endingFrame != -1);
        }
    }
}
