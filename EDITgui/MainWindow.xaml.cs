﻿using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Shape;
using Emgu.CV.UI;
using Kitware.mummy;
using Kitware.VTK;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum process{ AUTO, ANOTATION}

        public string workingPath;
        public string loadedStudyPath = null; //is filled if study is loaded
        public RenderWindowControl myRenderWindowControl;
        System.Windows.Forms.Integration.WindowsFormsHost host;
        vtkAxesActor axesActor;
        vtkOrientationMarkerWidget axes;

        public vtkRenderer renderer;
        public List<Geometry> STLGeometries = new List<Geometry>();
        public process currentProcess;

        Login user;
        Context context;

        public static int count = 0;


        public MainWindow()
        {
            InitializeComponent();
            user = new Login(this);
            user.Margin = new Thickness(0, 0, 0, 0);
            user.Visibility = Visibility.Collapsed; //-------------
            this.totalGrid.Children.Add(user);
        }


        public void doAfterUserAuthentication()
        {
            //user.Visibility = Visibility.Collapsed;
            currentProcess = process.AUTO;

            context = new Context(this, user);

            Console.WriteLine(Path.GetTempPath());

            workingPath = context.getStudyFile().getWorkspace();
            context.getCore().setExaminationsDirectory(workingPath);

            context.getUltrasoundPart().Width = 772;
            context.getUltrasoundPart().Margin = new Thickness(0, 0, 0, 0);
            context.getUltrasoundPart().HorizontalAlignment = HorizontalAlignment.Left;

            context.getPhotoAcousticPart().Margin = new Thickness(777, 0, 0, 0);
            context.getPhotoAcousticPart().Width = 738;

            context.getStudySettings().Margin = new Thickness(10, 5, 0, 0);
            context.getStudySettings().HorizontalAlignment = HorizontalAlignment.Left;
            context.getStudySettings().VerticalAlignment = VerticalAlignment.Top;
            context.getStudySettings().Height = 217;
            context.getStudySettings().Width = 199;

            context.getUltrasoundPoints2D().Margin = new Thickness(5, 97.6, 0, 0);
            context.getUltrasoundPoints2D().HorizontalAlignment = HorizontalAlignment.Left;
            context.getUltrasoundPoints2D().VerticalAlignment = VerticalAlignment.Top;
            context.getUltrasoundPoints2D().Height = 454.4;


            context.getSlicer().Margin = new Thickness(28.135, 267.588, 0, 0);
            context.getSlicer().HorizontalAlignment = HorizontalAlignment.Left;
            context.getSlicer().VerticalAlignment = VerticalAlignment.Top;
            context.getSlicer().Width = 201.87;
            context.getSlicer().Height = 445.037;

            context.getRegistration().Margin = new Thickness(585, 420.279, 0, 0);
            context.getRegistration().HorizontalAlignment = HorizontalAlignment.Left;
            context.getRegistration().VerticalAlignment = VerticalAlignment.Top;
            context.getRegistration().Height = 133;


            context.getPhotoAcousticPoints2D().Margin = new Thickness(11, 95, 0, 0);
            context.getPhotoAcousticPoints2D().HorizontalAlignment = HorizontalAlignment.Left;
            context.getPhotoAcousticPoints2D().VerticalAlignment = VerticalAlignment.Top;
            context.getPhotoAcousticPoints2D().Height = 454.4;

            this.components2D.Children.Add(context.getUltrasoundPart());
            this.components2D.Children.Add(context.getPhotoAcousticPart());
      
            this.totalGrid.Children.Add(context.getStudySettings());
            this.Viewer3D.Children.Add(context.getSlicer());
            

            UltrasoundPart.returnBladderSTL += OnAddAvailableGeometry;
            UltrasoundPart.returnSkinSTL += OnAddAvailableGeometry;
            PhotoAcousticPart.returnThicknessSTL += OnAddAvailableGeometry;
        }

        public void OnAddAvailableGeometry(Geometry geometry)
        {

            Geometry currentGeometry = STLGeometries.Find(x => x.geometryName == geometry.geometryName);
            string extension = Path.GetExtension(geometry.Path);

            if (currentGeometry != null)
            {
                STLGeometries.Remove(currentGeometry);
                geometry.checkbox = currentGeometry.checkbox;
                geometry.volumeLabel = currentGeometry.volumeLabel;
                geometry.surfaceAreaLabel = currentGeometry.surfaceAreaLabel;
                STLGeometries.Add(geometry);
                renderer.RemoveActor(currentGeometry.actor);
                visualizeGeometries(geometry);
                myRenderWindowControl.RenderWindow.Render();
            }
            else
            {
                CheckBox ch = new CheckBox();
                ch.Foreground = Brushes.White;
                if(extension == ".stl")
                {
                    Label volumeLabel = new Label();
                    volumeLabel.Foreground = Brushes.White;
                    Label surfaceAreaLabel = new Label();
                    surfaceAreaLabel.Foreground = Brushes.White;
                    geometry.volumeLabel = volumeLabel;
                    geometry.surfaceAreaLabel = surfaceAreaLabel;
                    this.volumeMetricsItems.Items.Add(volumeLabel);
                    this.SurfaceAreaMetricsItems.Items.Add(surfaceAreaLabel);

                }
                
                geometry.checkbox = ch;
               
                ch.Content = geometry.geometryName;
                STLGeometries.Add(geometry);
                this.geometryItems.Items.Add(ch);
              
                ch.Checked += new RoutedEventHandler(OnGeometryComboboxChecked);
                ch.Unchecked += new RoutedEventHandler(OnGeometryComboboxUnchecked);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           

        }

        

        public void OnGeometryComboboxChecked(object sender, RoutedEventArgs e)
        {
          //  host.Visibility = Visibility.Visible;
            CheckBox ch = (sender as CheckBox);
            Geometry geometry = STLGeometries.Find(x => x.geometryName == ch.Content);

            string extension = Path.GetExtension(geometry.Path);
            if (extension == ".stl")
            {
                geometry.volumeLabel.Visibility = Visibility.Visible;
                geometry.surfaceAreaLabel.Visibility = Visibility.Visible;
            }
            try
            {
                visualizeGeometries(geometry);
            }catch(Exception)
            {
                CustomMessageBox.Show(context.getMessages().noObject3DLoaded, context.getMessages().warning, MessageBoxButton.OK);
            }
            
        }


        public void visualizeGeometries(Geometry geometry)
        {
            vtkSTLReader reader = vtkSTLReader.New();
            vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            string extension = Path.GetExtension(geometry.Path);


            if(extension == ".stl")
            {
                reader.SetFileName(geometry.Path);
                reader.Update();
                if (geometry.checkbox.IsChecked == true)
                {
                   vtkMassProperties mass = vtkMassProperties.New();
                   mass.SetInput(reader.GetOutput());
                   mass.Update();
                   geometry.volumeLabel.Content = geometry.geometryName.ToString() + " = " + Math.Round(mass.GetVolume(), 2)  + " mm\xB3";
                   geometry.surfaceAreaLabel.Content = geometry.geometryName.ToString() + " = " + Math.Round(mass.GetSurfaceArea(), 2) + " mm\xB2"; 
                }


                mapper.SetInputConnection(reader.GetOutputPort());
                mapper.Update();

            }
            else if (extension == ".txt")
            {
                vtkPolyData polyData = vtkPolyData.New();
                polyData.SetPoints(txtPointsToPolyData(geometry.Path));
                vtkVertexGlyphFilter glyphFilter = vtkVertexGlyphFilter.New();
                glyphFilter.SetInput(polyData);
                glyphFilter.Update();

                mapper.SetInputConnection(glyphFilter.GetOutputPort());
            }

           
            vtkActor actor = vtkActor.New();
            actor.SetMapper(mapper);

            if (geometry.checkbox.IsChecked == true)
            {
                switch (geometry.geometryName)
                {
                    case Messages.bladderGeometry:
                        actor.GetProperty().SetColor(1, 1, 1);
                        actor.GetProperty().SetOpacity(1);
                        break;
                    case Messages.outerWallGeometry:
                        actor.GetProperty().SetColor(1, 1, 0);
                        actor.GetProperty().SetOpacity(0.8);
                        break;
                    case Messages.layerGeometry:
                        actor.GetProperty().SetColor(0, 1, 0);
                        actor.GetProperty().SetOpacity(0.3);
                        break;
                    case Messages.oxyGeometry:
                        actor.GetProperty().SetColor(1, 0, 0);
                        actor.GetProperty().SetOpacity(0.6);
                        break;
                    case Messages.deoxyGeometry:
                        actor.GetProperty().SetColor(0, 0, 1);
                        actor.GetProperty().SetOpacity(0.6);
                        break;
                    case Messages.tumorGeometry:
                        actor.GetProperty().SetColor(1, 0, 1);
                        actor.GetProperty().SetOpacity(0.6);
                        break;
                }
                geometry.actor = actor;
                renderer.AddActor(actor);
            }
            myRenderWindowControl.RenderWindow.Render();
        }


        public void OnGeometryComboboxUnchecked(object sender, RoutedEventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            Geometry geometry = STLGeometries.Find(x => x.geometryName == ch.Content);
            string extension = Path.GetExtension(geometry.Path);
            if (extension == ".stl")
            {
                geometry.volumeLabel.Visibility = Visibility.Hidden;
                geometry.surfaceAreaLabel.Visibility = Visibility.Hidden;
            }

            if (geometry.checkbox.IsChecked == false)
            {
                renderer.RemoveActor(geometry.actor);
            }
            myRenderWindowControl.RenderWindow.Render();
        }

        private void Viewer3D_Loaded(object sender, RoutedEventArgs e)
        {
            host = new System.Windows.Forms.Integration.WindowsFormsHost();
            myRenderWindowControl = new RenderWindowControl();

            myRenderWindowControl.SetBounds(0, 0, 0, 0); // not too big in case it disappears.
                                                         // Assign the control as the host control's child.
            host.Child = myRenderWindowControl;

            host.Margin = new Thickness(0, 0, 0, 0);
            rendererGrid.Children.Add(host);
            renderer = myRenderWindowControl.RenderWindow.GetRenderers().GetFirstRenderer();
            host.Visibility = Visibility.Hidden;

            renderer.GetActiveCamera().SetPosition(0, 0, 50);
            renderer.GetActiveCamera().SetFocalPoint(0, 0, 5);

            axesActor = vtkAxesActor.New();
            axes = vtkOrientationMarkerWidget.New();
            axes.SetOrientationMarker(axesActor);
            axes.SetInteractor(myRenderWindowControl.RenderWindow.GetInteractor());
            axes.EnabledOn();
            axes.InteractiveOff();

            // 
            //tempRenderer.Children.Remove(host);
            //rendererGrid.Children.Add(host);
            count++;
        }

        private void RendererGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if(count == 1)
            {
               Dispatcher.BeginInvoke(new Action(() => host.Visibility = Visibility.Visible));
               // Dispatcher.Invoke(new Action(() => { host.Visibility = Visibility.Visible; }), System.Windows.Threading.DispatcherPriority.ContextIdle, null);
            }
           
        }

        vtkImplicitPlaneRepresentation implPlaneWidget;
        vtkImplicitPlaneWidget2 planeWidget;
        double[] d;
        public void applySlicer()
        {
            if (implPlaneWidget != null)
            {
                planeWidget.SetInteractor(null);
            }

            if (imageActor != null)
            {
                renderer.RemoveActor(imageActor);
            }

            implPlaneWidget = vtkImplicitPlaneRepresentation.New();

            implPlaneWidget.SetVisibility(1); //set implPlaneWidget visible

            implPlaneWidget.SetPlaceFactor(1.01);
            implPlaneWidget.GetPlaneProperty().SetColor(1, 0, 1);
            implPlaneWidget.OutlineTranslationOff();
            implPlaneWidget.ScaleEnabledOff();

            d = STLGeometries.Find(x => x.geometryName == Messages.bladderGeometry).actor.GetBounds();
            d[0] = d[0] - double.Parse(context.getPhotoAcousticPart().minThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            d[1] = d[1] + double.Parse(context.getPhotoAcousticPart().maxThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture);

            d[2] = d[2] - double.Parse(context.getPhotoAcousticPart().minThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            d[3] = d[3] + double.Parse(context.getPhotoAcousticPart().maxThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            implPlaneWidget.PlaceWidget(DoubleArrayToIntPtr(d));

            implPlaneWidget.SetOrigin(0, 0, 0);
            implPlaneWidget.SetNormal(0, 0, 1);
            implPlaneWidget.GetNormalProperty().SetOpacity(0);
            implPlaneWidget.GetOutlineProperty().SetOpacity(0);

            planeWidget = vtkImplicitPlaneWidget2.New();
            planeWidget.SetInteractor(myRenderWindowControl.RenderWindow.GetInteractor());
            planeWidget.SetRepresentation(implPlaneWidget);
            planeWidget.InteractionEvt += PlaneWidget_InteractionEvt;
            myRenderWindowControl.RenderWindow.GetInteractor().Initialize();
            planeWidget.On();
            // renderer.ResetCamera();
            myRenderWindowControl.RenderWindow.Render();
        }

        double[] origin;
        private void PlaneWidget_InteractionEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            if (imageActor != null)
            {
                renderer.RemoveActor(imageActor);
            }
            implPlaneWidget.VisibilityOn();
            origin = implPlaneWidget.GetOrigin();
            context.getSlicer().updateImages(Convert.ToInt32(origin[2] / context.getStudySettings().distaceBetweenFrames));
        }

        vtkActor imageActor = vtkActor.New();
        public void overlayImage(string imagePath)
        {
            if (imageActor != null)
            {
                renderer.RemoveActor(imageActor);
            }


            vtkBMPReader BMPReader = vtkBMPReader.New();
            BMPReader.SetFileName(imagePath);
           
            BMPReader.Update();

            int[] dims = BMPReader.GetOutput().GetDimensions();
            origin = implPlaneWidget.GetOrigin();


            vtkImageChangeInformation changeInformation = vtkImageChangeInformation.New();
            changeInformation.SetInput(BMPReader.GetOutput());
         

            changeInformation.SetOriginTranslation(-(dims[0] / 2) * context.getStudySettings().xspace, -(dims[1] / 2) * context.getStudySettings().yspace, -origin[2]);

           

            double[] sc = { context.getStudySettings().xspace, context.getStudySettings().yspace, origin[2] };
       
            changeInformation.SetSpacingScale(DoubleArrayToIntPtr(sc));
            changeInformation.Update();


            vtkImageData imageData = vtkImageData.New();
            imageData = changeInformation.GetOutput();
            
            vtkTransform transform = vtkTransform.New();
            transform.RotateX(180);

            vtkDataSetMapper imageMapper = vtkDataSetMapper.New();
            imageMapper.SetInput(imageData);
            
            imageActor.SetMapper(imageMapper);
            imageActor.SetUserTransform(transform);

            implPlaneWidget.VisibilityOff();

            renderer.AddActor(imageActor);

            myRenderWindowControl.RenderWindow.Render();
        }


        public IntPtr DoubleArrayToIntPtr(double[] d)
        {
            IntPtr p = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(sizeof(double) * d.Length);
            System.Runtime.InteropServices.Marshal.Copy(d, 0, p, d.Length);
            return p;
        }



        private vtkPoints txtPointsToPolyData(string filename)
        {
            vtkPoints points = vtkPoints.New();
            vtkPoints pointslist = vtkPoints.New();
    
         
            StreamReader sr = new StreamReader(filename);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                double x, y, z;

                string[] col = line.Split(' ');

                x = double.Parse(col[0].Replace(",", "."), CultureInfo.InvariantCulture);  
                y = double.Parse(col[1].Replace(",", "."), CultureInfo.InvariantCulture);
                z = double.Parse(col[2].Replace(",", "."), CultureInfo.InvariantCulture);

                points.InsertNextPoint(x, y, z);

            }
            sr.Close();

            return points;
        }


        private void SaveStudy_Click(object sender, RoutedEventArgs e)
        {
            if (loadedStudyPath == null)
            {
                context.getSaveActions().doSave();
            }
            else
            {
                MessageBoxResult result = CustomMessageBox.Show(context.getMessages().getOverwriteExistingStudyQuestion(loadedStudyPath), context.getMessages().warning, MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                   context.getSaveActions().saveAvailableData(loadedStudyPath);
                }
                else if (result == MessageBoxResult.No)
                {
                    context.getSaveActions().doSave();
                }
            }
        }


        private void LoadStudy_Click(object sender, RoutedEventArgs e)
        {
            context.getLoadActions().doLoad();
        }

        public void cleanVTKRender()
        {
            foreach (Geometry g in STLGeometries)
            {
                if (g.actor != null) renderer.RemoveActor(g.actor);
            }

            if (imageActor != null)
            {
                renderer.RemoveActor(imageActor);
            }

            myRenderWindowControl.RenderWindow.Render();
            volumeMetricsItems.Items.Clear();
            SurfaceAreaMetricsItems.Items.Clear();
            geometryItems.Items.Clear();
            STLGeometries.Clear();
            context.getUltrasoundPart().bladderGeometryPath = null;
            context.getPhotoAcousticPart().thicknessGeometryPath = null;
            if(implPlaneWidget!=null)  implPlaneWidget.SetVisibility(0);
            context.getSlicer().InitializeView();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (this.user.isAuthenticated)
            {
                //close logging process
                context.getCore().setLoggingOnOff(false);
                //Delete working directory
                if (Directory.Exists(workingPath))
                {
                    Directory.Delete(workingPath, true);
                }
            } 
        }

        private void Switch_2D_3D_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (switch_2D_3D.currentState)
            {
                case twoStatesButton.states.threeDimensional:

                    components2D.Visibility = Visibility.Collapsed;
                    Viewer3D.Visibility = Visibility.Visible;
                    break;
                case twoStatesButton.states.twoDimensional:

                    Viewer3D.Visibility = Visibility.Collapsed;
                    components2D.Visibility = Visibility.Visible;
                    break;
            }
        }
    }

    public class Geometry
    {
        public string geometryName { get; set; }
        public string Path { get; set; }
        public CheckBox checkbox { get; set; }
        public Label volumeLabel { get; set; }
        public Label surfaceAreaLabel { get; set; }
        public vtkActor actor { set; get; }

        public SaveActions.FileType getSaveActionFileType()
        {
            switch (geometryName)
            {
                case Messages.bladderGeometry:
                    return SaveActions.FileType.Bladder3D;
                case Messages.outerWallGeometry:
                    return SaveActions.FileType.Thickness3D;
                case Messages.layerGeometry:
                    return SaveActions.FileType.Layer3D;
                case Messages.oxyGeometry:
                    return SaveActions.FileType.OXY3D;
                case Messages.deoxyGeometry:
                    return SaveActions.FileType.DeOXY3D;
                case Messages.tumorGeometry:
                    return SaveActions.FileType.Tumors;
            }

            return SaveActions.FileType.Bladder3D; //never

        }
    }

}
