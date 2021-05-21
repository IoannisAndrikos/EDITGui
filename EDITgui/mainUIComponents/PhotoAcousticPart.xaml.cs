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
        public static event STLThicknessHandler returnAvailableGeometry = delegate { };

        Context context;
        public managePhotoacousticImaging photoacousticImaging;

        public enum ContourSegmentation
        {
            CORRECTION,
            MANUAL,
            FILL_POINTS
        }

        public enum OxyDeOxyGNRState { OXY, DEOXY, GNR}

        public bool wasThicknessModelExtracted = false;

        //-----------for slider----------
        public int slider_value = 0;
        public int fileCount;
        //-------------------------------

        //initialization
        public ContourSegmentation contourSeg = ContourSegmentation.CORRECTION;
        public OxyDeOxyGNRState currentOxyDeOxyState = OxyDeOxyGNRState.OXY;

        double calibration_x;
        double calibration_y;

        List<Polyline> polylines = new List<Polyline>();
        List<List<EDITCore.CVPoint>> thicknessCvPoints = new List<List<EDITCore.CVPoint>>();
        List<List<EDITCore.CVPoint>> bladderCvPoints = new List<List<EDITCore.CVPoint>>();
        List<List<List<EDITCore.CVPoint>>> tumorCvPoints = new List<List<List<EDITCore.CVPoint>>>();
        List<EDITCore.CVPoint> contourForFix = new List<EDITCore.CVPoint>();
    
        public String thicknessGeometryPath = null;
        
        public PhotoAcousticPart()
        {
            InitializeComponent();
            UltrasoundPart.zoomUltrasoundChanged += OnUltrasoundZoomChanged;
            doOXYState();
        }

        public PhotoAcousticPart(Context context)
        {
            InitializeComponent();
            this.context = context;
            this.photoacousticImaging = new managePhotoacousticImaging(this.context);
            photoacousticImaging.Margin = new Thickness(0, 0, 0, 0);
            photoacousticImaging.HorizontalAlignment = HorizontalAlignment.Left;
            photoacousticImaging.VerticalAlignment = VerticalAlignment.Top;

            imageModalitiesGrid.Children.Add(photoacousticImaging);
            applicationGrid.Children.Add(context.getPhotoAcousticData());
            //add tumor annotation option here
            UltrasoundPart.zoomUltrasoundChanged += OnUltrasoundZoomChanged;
            doOXYState();
        }


        public void sliderValueChanged(int value)
        {
            //savePhotoacousticFrameAlgorithmSetting();
            slider_value = value;
            if (slider_value < fileCount)
            {
                if (currentOxyDeOxyState == OxyDeOxyGNRState.OXY && photoacousticImaging.OXYDicomFile != null )
                {
                    BitmapFromPath(photoacousticImaging.OXY_ImagesDir + Path.DirectorySeparatorChar + value + ".bmp");
                    frame_num_label.Content = context.getMessages().frame + ": " + slider_value;
                }
                else if (currentOxyDeOxyState == OxyDeOxyGNRState.DEOXY && photoacousticImaging.DeOXYDicomFile != null)
                {
                    BitmapFromPath(photoacousticImaging.DeOXY_ImagesDir + Path.DirectorySeparatorChar + value + ".bmp");
                    frame_num_label.Content = context.getMessages().frame + ": " + slider_value;
                }
                else if (currentOxyDeOxyState == OxyDeOxyGNRState.GNR && photoacousticImaging.GNRDicomFile != null)
                {
                    BitmapFromPath(photoacousticImaging.GNR_ImagesDir + Path.DirectorySeparatorChar + value + ".bmp");
                    frame_num_label.Content = context.getMessages().frame + ": " + slider_value;
                }
                //updatePhotoacousticFrameAlgorithmSetting();
                doCorrection(true, false);
            }
        }

        private void savePhotoacousticFrameAlgorithmSetting()
        {
            context.getImages().getCurrentFrameSettings().minThickness = double.Parse(minThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            context.getImages().getCurrentFrameSettings().maxThickness = double.Parse(maxThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            context.getImages().getCurrentFrameSettings().majorThicknessExistence = big_tumor.IsChecked.Value;
        }

        private void updatePhotoacousticFrameAlgorithmSetting()
        {
            //minThickness.Text = string.Format("{0.0}", context.getImages().getFrameSettings().minThickness.ToString());
            //maxThickness.Text = context.getImages().getFrameSettings().maxThickness.ToString();
            //big_tumor.IsChecked = context.getImages().getFrameSettings().majorThicknessExistence;
        }

        public void OnUltrasoundZoomChanged(List<Matrix> obj)
        {
            var element = canvasPhotoAcoustic;
            element.RenderTransform = new MatrixTransform(obj.LastOrDefault());
            zoom_out = obj;
        }

        public void doRepeatProcess()
        {
            thicknessCvPoints.Clear();
            metrics_label.Visibility = Visibility.Hidden;
            contourSeg = ContourSegmentation.CORRECTION;
            updateCanvas();
        }

      
        private async void Extract_thikness_Click(object sender, RoutedEventArgs e)
        {
            string message = context.getCheck().getMessage(checkBeforeExecute.executionType.extract2DThickness);
            if (message != null)
            {
                CustomMessageBox.Show(message, context.getMessages().warning, MessageBoxButton.OK);
                return;
            }

            AlgorithmSettings sets = new AlgorithmSettings();


           sets.minThickness = double.Parse(minThickness.Text.Replace(",","."), CultureInfo.InvariantCulture);
           sets.maxThickness = double.Parse(maxThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            sets.majorThicknessExistence = big_tumor.IsChecked.Value;

            context.getImages().setPhotoacousticFrameSettingsOfAllSequence(sets);

            List<List<EDITCore.CVPoint>> bladderCVPoints = context.getImages().getAllFramesCVPoints(context.getImages().getTotalBladderPoints());
            if (!bladderCVPoints.Any()) return;

            startSpinner();
            await Task.Run(() =>
            {
                thicknessCvPoints = context.getCore().extractThickness(bladderCVPoints, sets.minThickness, sets.maxThickness, sets.majorThicknessExistence);
                if (!thicknessCvPoints.Any()) return;
                context.getImages().fillThicknessFromBackEnd(thicknessCvPoints, context.getCore().meanThickness, context.getUltrasoundPart().startingFrame);
            });
            doCorrection(true, true);
            stopSpinner();

        }

        private async void Recalculate_Click(object sender, RoutedEventArgs e)
        {
            string message = context.getCheck().getMessage(checkBeforeExecute.executionType.recalculateThickness);
            if (message != null)
            {
                CustomMessageBox.Show(message, context.getMessages().warning, MessageBoxButton.OK);
                return;
            }

            startSpinner();

            AlgorithmSettings sets = new AlgorithmSettings();

            sets.minThickness = double.Parse(minThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            sets.maxThickness = double.Parse(maxThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            sets.majorThicknessExistence = big_tumor.IsChecked.Value;

            await Task.Run(() =>
            {
                contourForFix = context.getCore().recalculateThicknessOfContour(slider_value, context.getImages().getUniqueFrameCVPoints(context.getImages().getBladderPoints()), sets.minThickness, sets.maxThickness, sets.majorThicknessExistence);
                if (!contourForFix.Any()) return;
                context.getImages().fillUniqueFrameThicknessFromBackEnd(contourForFix, context.getCore().uniqueContourMeanThickness);
            });
            doCorrection(true, true);
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

            thicknessCvPoints = context.getImages().getAllFramesCVPoints(context.getImages().getTotalThicknessPoints());
            if (!thicknessCvPoints.Any()) return;

            startSpinner();
            await Task.Run(() => {
                thicknessGeometryPath = context.getCore().extractThicknessSTL(thicknessCvPoints);
                });
            EDITgui.Geometry thicknessGeometry = new Geometry() { geometryName = Messages.outerWallGeometry, Path = thicknessGeometryPath, actor = null };
            if (thicknessGeometryPath != null)
            {
                returnAvailableGeometry(thicknessGeometry);
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

            thicknessCvPoints = context.getImages().getAllFramesCVPoints(context.getImages().getTotalThicknessPoints());
            bladderCvPoints = context.getImages().getAllFramesCVPoints(context.getImages().getTotalBladderPoints());
            if (!thicknessCvPoints.Any() || !bladderCvPoints.Any()) return;

            startSpinner();
            List<String> txtPaths = new List<string>();
            await Task.Run(() => {
                txtPaths = context.getCore().extractOXYandDeOXYPoints(bladderCvPoints, thicknessCvPoints, context.getUltrasoundPart().bladderGeometryPath, thicknessGeometryPath);
            });

            EDITgui.Geometry OXYGeometry = new Geometry() { geometryName = Messages.oxyGeometry, Path = txtPaths[0], actor = null };
            EDITgui.Geometry DeOXYGeometry = new Geometry() { geometryName = Messages.deoxyGeometry, Path = txtPaths[1], actor = null };
            if (txtPaths.Any())
            {
                returnAvailableGeometry(OXYGeometry);
                returnAvailableGeometry(DeOXYGeometry);
            }
            stopSpinner();
        }


        private async void Extract_GNR_Click(object sender, RoutedEventArgs e)
        {
            string message = context.getCheck().getMessage(checkBeforeExecute.executionType.extractOXYDeOXY);
            if (message != null)
            {
                CustomMessageBox.Show(message, context.getMessages().warning, MessageBoxButton.OK);
                return;
            }

            thicknessCvPoints = context.getImages().getAllFramesCVPoints(context.getImages().getTotalThicknessPoints());
            bladderCvPoints = context.getImages().getAllFramesCVPoints(context.getImages().getTotalBladderPoints());
            if (!thicknessCvPoints.Any() || !bladderCvPoints.Any()) return;

            startSpinner();
            string path = null;
            await Task.Run(() => {
                path = context.getCore().extractGNRPoints(bladderCvPoints, thicknessCvPoints, context.getUltrasoundPart().bladderGeometryPath, thicknessGeometryPath);
            });

            EDITgui.Geometry GNRGeometry = new Geometry() { geometryName = Messages.GNRGeometry, Path = path, actor = null };
            if (path!=null)
            {
                returnAvailableGeometry(GNRGeometry);
                
            }
            stopSpinner();

        }

        //async
        private void Extract_Tumor_Click(object sender, RoutedEventArgs e)
        {
            //string message = context.getCheck().getMessage(checkBeforeExecute.executionType.extract2DTumor);
            //if (message != null)
            //{
            //    CustomMessageBox.Show(message, context.getMessages().warning, MessageBoxButton.OK);
            //    return;
            //}

            //thicknessCvPoints = context.getImages().getAllFramesCVPoints(context.getImages().getTotalThicknessPoints());
            //bladderCvPoints = context.getImages().getAllFramesCVPoints(context.getImages().getTotalBladderPoints());
            //if (!thicknessCvPoints.Any() || !bladderCvPoints.Any()) return;

            //Point startingPoint = context.getUltrasoundPart().userPoints[0];

            //startSpinner();
  
            //await Task.Run(() => {
            //    thicknessCvPoints = context.getCore().Tumor2DExtraction2D(startingPoint, bladderCvPoints, thicknessCvPoints, context.getUltrasoundPart().bladderGeometryPath, thicknessGeometryPath);
            //    if(!thicknessCvPoints.Any()) return;
            //    context.getImages().fillTumorFromBackEnd(thicknessCvPoints, context.getUltrasoundPart().startingFrame);
            //});
            //context.getPhotoAcousticPoints2D().updatePanel(slider_value);
            //context.getUltrasoundPoints2D().updatePanel(slider_value);
            //context.getUltrasoundPart().updateCanvas();
            //updateCanvas();
            //stopSpinner();
        }

        private async void Extract_Tumor_3D_Click(object sender, RoutedEventArgs e)
        {
            string message = context.getCheck().getMessage(checkBeforeExecute.executionType.extract3DTumor);
            if (message != null)
            {
                CustomMessageBox.Show(message, context.getMessages().warning, MessageBoxButton.OK);
                return;
            }

            tumorCvPoints = context.getImages().getAllFramesCVPoints(context.getImages().getAllFramesTumorPoints());

            int startingFrame = context.getImages().getFirtTumorFrame() - context.getUltrasoundPart().startingFrame;

            if (!tumorCvPoints.Any()) return;

            //Point startingPoint = context.getUltrasoundPart().userPoints[0];

            startSpinner();
            String txtPath = null;
            await Task.Run(() => {
                txtPath = context.getCore().Tumor3DExtraction(tumorCvPoints, startingFrame, context.getUltrasoundPart().bladderGeometryPath, context.getPhotoAcousticPart().thicknessGeometryPath);
            });

            EDITgui.Geometry TumorGeometry = new Geometry() { geometryName = Messages.tumorGeometry, Path = txtPath, actor = null };
            if (txtPath != null)
            {
                returnAvailableGeometry(TumorGeometry);
            }
            stopSpinner();
        }


        //----------------------------------------------------------CANVAS OPERATIONS--------------------------------------------------------
        public void updateCanvas()
       {
            clear_canvas();
            display();
       }

        public void clear_canvas()
        {
            canvasPhotoAcoustic.Children.Clear();
            canvasPhotoAcoustic.Children.Add(image);
        }


        public void display()
        {
            if(image.Source != null)
            {
                Selected2DObject selectedObject = context.getPhotoAcousticData().getSelectedObject2D();
                drawPolylineOfRest2DObjects();
                draw_polyline(selectedObject);
                draw_points(selectedObject);
                metrics_label.Foreground = selectedObject.polylineColor;
                metrics_label.Content = selectedObject.metrics;
                //draw_polyline_ultrasound(context.getImages().getBladderPoints());
            }
        }

        public void eraseImageView()
        {
            canvasPhotoAcoustic.Children.Clear();
            studyname_label.Content = "";
            frame_num_label.Content = "";
            metrics_label.Content = "";
            image.Source = null;
            switch_auto_manual.Visibility = Visibility.Hidden;
            context.getPhotoAcousticData().Visibility = Visibility.Collapsed;
            photoacousticImaging.uncheckRestCheckboxes();
        }


        private void draw_points(Selected2DObject contour)
        {
            for (int i = 0; i < contour.points.Count; i++)
            {
                Ellipse ellipse = new Ellipse();
                //ellipse.Fill = Brushes.Blue;
                ellipse.Fill = contour.polylineColor;//context.getColors().cyan;
                ellipse.Width = ViewAspects.pointSize;
                ellipse.Height = ViewAspects.pointSize;
                canvasPhotoAcoustic.Children.Add(ellipse);
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
                    if (closeCurvePoints.Any()) closeCurvePoints.Add(closeCurvePoints[0]);
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
                    canvasPhotoAcoustic.Children.Add(pl);
                    polylines.Add(pl);
                }
            }
            closeCurvePoints.Clear();
        }

        //private void draw_polyline_ultrasound(List<Point> points)
        //{
        //    List<Point> closeCurvePoints = points.ToList();//new List<Point>();
        //    if(closeCurvePoints.Any()) closeCurvePoints.Add(points[0]);

        //    for (int i = 0; i < closeCurvePoints.Count - 1; i++)
        //    {
        //        Polyline pl = new Polyline();
        //        pl.FillRule = FillRule.EvenOdd;
        //        pl.StrokeThickness = 0.5;
        //        pl.Points.Add(closeCurvePoints.ElementAt(i));
        //        pl.Points.Add(closeCurvePoints.ElementAt(i + 1));
        //        pl.Stroke = ViewAspects.silver;//context.getColors().silver;
        //        pl.StrokeStartLineCap = PenLineCap.Round;
        //        pl.StrokeEndLineCap = PenLineCap.Round;
        //        canvasPhotoAcoustic.Children.Add(pl);
        //    }
        //    closeCurvePoints.Clear();
        //}

        private void drawPolylineOfRest2DObjects()
        {
            List<UIElement> list = context.getPhotoAcousticData().getNoSelectedObject2DPolylines();
            foreach (UIElement pl in list)
            {
                canvasPhotoAcoustic.Children.Add(pl);
            }

            list.Clear();
        }

        private void canvasPhotoAcoustic_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!contourSeg.Equals(ContourSegmentation.CORRECTION))
            {
                Point point = e.GetPosition(image); //position relative to the image

                if (e.ChangedButton == MouseButton.Left)
                {
                    if (contourSeg == ContourSegmentation.FILL_POINTS)
                    {
                        clear_canvas();
                        if (indexA > context.getPhotoAcousticData().getSelectedObject2D().points.Count - 1) indexA = 0;

                        double d1, d2;
                        if (indexA != 0)
                        {
                            d1 = Math.Sqrt(Math.Pow(point.X - context.getPhotoAcousticData().getSelectedObject2D().points[indexA - 1].X, 2) + Math.Pow(point.Y - context.getPhotoAcousticData().getSelectedObject2D().points[indexA - 1].Y, 2));
                        }
                        else
                        {
                            int num = context.getPhotoAcousticData().getSelectedObject2D().points.Count - 1;
                            d1 = Math.Sqrt(Math.Pow(point.X - context.getPhotoAcousticData().getSelectedObject2D().points[num].X, 2) + Math.Pow(point.Y - context.getPhotoAcousticData().getSelectedObject2D().points[num].Y, 2));
                        }
                        d2 = Math.Sqrt(Math.Pow(point.X - context.getPhotoAcousticData().getSelectedObject2D().points[indexA].X, 2) + Math.Pow(point.Y - context.getPhotoAcousticData().getSelectedObject2D().points[indexA].Y, 2));

                        if (d2 > d1)
                        {
                            context.getPhotoAcousticData().getSelectedObject2D().points.Insert(indexA++, point);
                        }
                        else
                        {
                            context.getPhotoAcousticData().getSelectedObject2D().points.Insert(indexA, point);
                        }
                        display();
                    }
                    else if (contourSeg == ContourSegmentation.MANUAL)
                    {
                        context.getPhotoAcousticData().getSelectedObject2D().points.Add(point);
                    }
                    context.getPhotoAcousticData().updateSelectedObjectMetrics();
                    updateCanvas();
                }
                else if (e.ChangedButton == MouseButton.Right && contourSeg == ContourSegmentation.MANUAL)
                {
                    if (context.getPhotoAcousticData().getSelectedObject2D().points.Any())
                    {
                        context.getPhotoAcousticData().getSelectedObject2D().points.RemoveAt(context.getPhotoAcousticData().getSelectedObject2D().points.Count - 1);
                        context.getPhotoAcousticData().updateSelectedObjectMetrics();
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
                context.getSaveActions().dataUpdatedWithoutSave();
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
                    context.getSaveActions().dataUpdatedWithoutSave();
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
                        int j = context.getPhotoAcousticData().getSelectedObject2D().points.IndexOf(initialPoisition1);
                        context.getPhotoAcousticData().getSelectedObject2D().points[j] = final_position;
                        context.getPhotoAcousticData().updateSelectedObjectMetrics();
                        context.getUltrasoundPart().updateCanvas();
                    }
                    catch { }

                    updateCanvas();
                    context.getSaveActions().dataUpdatedWithoutSave();
                }
            }
        }

        Point _start;
        Rectangle rectRemovePoints;
        private void canvasPhotoAcoustic_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!selectedObjectHasPoints() || contourSeg == ContourSegmentation.MANUAL) return;

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
            if (!selectedObjectHasPoints() || contourSeg == ContourSegmentation.MANUAL) return;

            try
            {
                List<Point> pointsToRemove = new List<Point>();
                int count = 0;
                for (int i = 0; i < context.getPhotoAcousticData().getSelectedObject2D().points.Count; i++)
                {
                    if (context.getPhotoAcousticData().getSelectedObject2D().points[i].X >= Canvas.GetLeft(rectRemovePoints) && context.getPhotoAcousticData().getSelectedObject2D().points[i].Y >= Canvas.GetTop(rectRemovePoints) &&
                        context.getPhotoAcousticData().getSelectedObject2D().points[i].X <= Canvas.GetLeft(rectRemovePoints) + rectRemovePoints.Width && context.getPhotoAcousticData().getSelectedObject2D().points[i].Y <= Canvas.GetTop(rectRemovePoints) + rectRemovePoints.Height)
                    {
                        pointsToRemove.Add(context.getPhotoAcousticData().getSelectedObject2D().points[i]);
                        if (count == 0) indexA = i;
                        count++;
                    }

                }

                canvasPhotoAcoustic.Children.Remove(rectRemovePoints);
                if (count == context.getPhotoAcousticData().getSelectedObject2D().points.Count)
                {
                    doManual();
                    context.getPhotoAcousticData().updateSelectedObjectMetrics();
                    return;
                }
                context.getPhotoAcousticData().getSelectedObject2D().points.RemoveAll(item => pointsToRemove.Contains(item));
                if (!contourSeg.Equals(ContourSegmentation.MANUAL) && count > 0) doFillPoints();
                if (pointsToRemove.Any()) context.getSaveActions().dataUpdatedWithoutSave();
                pointsToRemove.Clear();
            }catch(Exception ex)
            {

            }
        }


        private void canvasPhotoAcoustic_MouseMove(object sender, MouseEventArgs e)
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

        public void fitUIAccordingToDicomImageSize(double height, double width)
        {
            Thickness waitMargin = Wait.Margin;
            waitMargin.Left = width / 2 - Wait.Width / 2;//344.426 - Math.Abs(736 - width);

          
            Wait.Margin = waitMargin;

            OXY_Buttons.Width = width;
            frame_actions_infos.Width = width;

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
                context.getPhotoAcousticData().updateSelectedObjectMetrics();
            }
            else if (this.contourSeg == ContourSegmentation.CORRECTION)
            {
                doManual();
            }
            else if (this.contourSeg == ContourSegmentation.FILL_POINTS)
            {
                doCorrection();
                context.getPhotoAcousticData().updateSelectedObjectMetrics();
            }
        }


        public void startSpinner()
        {
            ((Storyboard)FindResource("WaitStoryboard")).Begin();
            context.getMainWindow().Panel2D.IsEnabled = false;
            Wait.Visibility = Visibility.Visible;
        }

        public void stopSpinner()
        {
            ((Storyboard)FindResource("WaitStoryboard")).Stop();
            context.getMainWindow().Panel2D.IsEnabled = true;
            Wait.Visibility = Visibility.Hidden;
            context.getSaveActions().dataUpdatedWithoutSave();
        }

        public void doCorrection(bool update = true, bool updateUlrasound = true)
        {
            this.switch_auto_manual.doCorrectionState();
            contourSeg = ContourSegmentation.CORRECTION;
            metrics_label.Visibility = Visibility.Visible;
            if (update) updateCanvas();
            if(updateUlrasound) context.getUltrasoundPart().updateCanvas();

        }

        public void doManual()
        {
            this.switch_auto_manual.doManualState();
            contourSeg = ContourSegmentation.MANUAL;
            metrics_label.Visibility = Visibility.Hidden;
            if (selectedObjectHasPoints()) context.getPhotoAcousticData().getSelectedObject2D().points.Clear();
            updateCanvas();
            context.getUltrasoundPart().updateCanvas();
        }

        public void doFillPoints()
        {
            this.switch_auto_manual.doFillPointState();
            contourSeg = ContourSegmentation.FILL_POINTS;
            metrics_label.Visibility = Visibility.Hidden;
            updateCanvas();
        }


        public bool areTherePoints()
        {
            return (image.Source != null && context.getImages().getThicknessPoints().Any());
        }

        public bool selectedObjectHasPoints()
        {
            return (context.getPhotoAcousticData().getSelectedObject2D().points.Any());
        }



        private void Oxy_deoxy_switch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (currentOxyDeOxyState)
            {
                case OxyDeOxyGNRState.OXY:
                    doDeOXYState();
                    break;
                case OxyDeOxyGNRState.DEOXY:
                    doOXYState();
                    break;
            }
        }

        public void doOXYState()
        {
            //oxy_deoxy_label.Content = "  " + context.getMessages().oxy;
            //oxy_deoxy_switch.setCustomDotToLeftAndBlue();
            currentOxyDeOxyState = OxyDeOxyGNRState.OXY;
            if (photoacousticImaging.OXYDicomFile!=null)
            {
                BitmapFromPath(photoacousticImaging.OXY_ImagesDir + Path.DirectorySeparatorChar + slider_value + ".bmp");
                frame_num_label.Content = context.getMessages().frame + ":" + " " + slider_value;
                makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Visible);
                studyname_label.Content = photoacousticImaging.OXYStudyname;
                switch_auto_manual.Visibility = Visibility.Visible;
                context.getPhotoAcousticData().Visibility = Visibility.Visible;
                metrics_label.Visibility = Visibility.Visible;
            }
            else
            {
                image.Source = null;
                frame_num_label.Content = "";
                studyname_label.Content = "";
                metrics_label.Content = "";
                makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Hidden);
                switch_auto_manual.Visibility = Visibility.Hidden;
                context.getPhotoAcousticData().Visibility = Visibility.Collapsed;
                metrics_label.Visibility = Visibility.Hidden;
            }
            updateCanvas();
        }

        public void doDeOXYState()
        {
            //oxy_deoxy_label.Content = context.getMessages().deoxy;
            //oxy_deoxy_switch.setCustomDotToRightAndRed();
            currentOxyDeOxyState = OxyDeOxyGNRState.DEOXY;
            if (photoacousticImaging.DeOXYDicomFile!=null)
            {
                BitmapFromPath(photoacousticImaging.DeOXY_ImagesDir + Path.DirectorySeparatorChar + slider_value + ".bmp");
                frame_num_label.Content = context.getMessages().frame + ":" + " " + slider_value;
                makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Visible);
                studyname_label.Content = photoacousticImaging.DeOXYStudyname;
                switch_auto_manual.Visibility = Visibility.Visible;
                context.getPhotoAcousticData().Visibility = Visibility.Visible;
                metrics_label.Visibility = Visibility.Visible;
            }
            else
            {
                image.Source = null;
                frame_num_label.Content = "";
                studyname_label.Content = "";
                metrics_label.Content = "";
                makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Hidden);
                switch_auto_manual.Visibility = Visibility.Hidden;
                context.getPhotoAcousticData().Visibility = Visibility.Collapsed;
                metrics_label.Visibility = Visibility.Hidden;
            }
            updateCanvas();
        }


        public void doGNRState()
        {
            //oxy_deoxy_label.Content = "  " + context.getMessages().oxy;
            //oxy_deoxy_switch.setCustomDotToLeftAndBlue();
            currentOxyDeOxyState = OxyDeOxyGNRState.GNR;
            if (photoacousticImaging.GNRDicomFile != null)
            {
                BitmapFromPath(photoacousticImaging.GNR_ImagesDir + Path.DirectorySeparatorChar + slider_value + ".bmp");
                frame_num_label.Content = context.getMessages().frame + ":" + " " + slider_value;
                makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Visible);
                studyname_label.Content = photoacousticImaging.GNRStudyname;
                switch_auto_manual.Visibility = Visibility.Visible;
                context.getPhotoAcousticData().Visibility = Visibility.Visible;
                metrics_label.Visibility = Visibility.Visible;
            }
            else
            {
                image.Source = null;
                frame_num_label.Content = "";
                studyname_label.Content = "";
                metrics_label.Content = "";
                makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Hidden);
                switch_auto_manual.Visibility = Visibility.Hidden;
                context.getPhotoAcousticData().Visibility = Visibility.Collapsed;
                metrics_label.Visibility = Visibility.Hidden;
            }
            updateCanvas();
        }

        public void makeVisibeOrUnvisibleSliderLeftTickBar(Visibility visibility)
        {
            if (context.getUltrasoundPart().ultrasound_slider.IsLoaded)
            {
                UIElement ultrasoundSliderLeftTickBar = (UIElement)context.getUltrasoundPart().ultrasound_slider.Template.FindName("BottomTick", context.getUltrasoundPart().ultrasound_slider);
                ultrasoundSliderLeftTickBar.Visibility = visibility;
            }
        }
        //-----------------------------------------------
    }

        
}
