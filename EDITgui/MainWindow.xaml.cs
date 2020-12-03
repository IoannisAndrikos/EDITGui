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
    
        public vtkRenderer renderer;
        public List<Geometry> STLGeometries = new List<Geometry>();
        public process currentProcess;

        Login authentication;
        Messages messages;
        StudyFile studyFile;
        UltrasoundPart ultrasound;
        PhotoAcousticPart photoAcoustic;
        settings studySettings;
        SaveActions saveActions;
        LoadActions loadActions;
        coreFunctionality core;
        checkBeforeExecute check;

        public static int count = 0;


        public MainWindow()
        {
            InitializeComponent();

            messages = new Messages();
            authentication = new Login(this);
            authentication.Margin = new Thickness(0, 0, 0, 0);
            this.totalGrid.Children.Add(authentication);
        }


        public void doAfterAuthentication()
        {
            authentication.Visibility = Visibility.Collapsed;
            currentProcess = process.AUTO;
            studyFile = new StudyFile();
            core = new coreFunctionality();

            Console.WriteLine(Path.GetTempPath());

            workingPath = studyFile.getWorkspace();
            core.setExaminationsDirectory(workingPath);

            ultrasound = new UltrasoundPart(this);
            photoAcoustic = new PhotoAcousticPart(this, ultrasound); //pass ultasound instance in to photoaccoustic in order to exchange data
            studySettings = new settings(this, core, ultrasound, photoAcoustic);

            check = new checkBeforeExecute(this, ultrasound, photoAcoustic);
            loadActions = new LoadActions(this, core, ultrasound, photoAcoustic, workingPath);
            saveActions = new SaveActions(this, core, ultrasound, photoAcoustic, workingPath);

            ultrasound.setSettings(studySettings, check);
            photoAcoustic.setSettings(studySettings, check);

            ultrasound.InitializeCoreFunctionality = core;
            photoAcoustic.InitializeCoreFunctionality = core;

            ultrasound.Width = 772;
            ultrasound.Margin = new Thickness(0, 0, 0, 0);
            ultrasound.HorizontalAlignment = HorizontalAlignment.Left;

            photoAcoustic.Margin = new Thickness(777, 0, 0, 0);
            photoAcoustic.Width = 738;


            studySettings.Margin = new Thickness(10, 5, 0, 0);
            studySettings.HorizontalAlignment = HorizontalAlignment.Left;
            studySettings.VerticalAlignment = VerticalAlignment.Top;
            studySettings.Height = 217;
            studySettings.Width = 199;

            this.Rat.Children.Add(ultrasound);
            this.Rat.Children.Add(photoAcoustic);
            this.totalGrid.Children.Add(studySettings);

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
                CustomMessageBox.Show(messages.noObject3DLoaded, messages.warning, MessageBoxButton.OK);
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
                   geometry.volumeLabel.Content = geometry.geometryName.ToString() + " = " + Math.Round(mass.GetVolume(), 2)  + "mm\xB3";
                   geometry.surfaceAreaLabel.Content = geometry.geometryName.ToString() + " = " + Math.Round(mass.GetSurfaceArea(), 2) + "mm\xB2"; 
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
                    case "Bladder":
                        actor.GetProperty().SetColor(1, 1, 1);
                        actor.GetProperty().SetOpacity(1);
                        break;
                    case "Thickness":
                        actor.GetProperty().SetColor(1, 1, 0);
                        actor.GetProperty().SetOpacity(0.8);
                        break;
                    case "Layer":
                        actor.GetProperty().SetColor(0, 1, 0);
                        actor.GetProperty().SetOpacity(0.3);
                        break;
                    case "OXY":
                        actor.GetProperty().SetColor(1, 0, 0);
                        actor.GetProperty().SetOpacity(0.6);
                        break;
                    case "DeOXY":
                        actor.GetProperty().SetColor(0, 0, 1);
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
                saveActions.doSave();
            }
            else
            {
                MessageBoxResult result = CustomMessageBox.Show(messages.getOverwriteExistingStudyQuestion(loadedStudyPath), messages.warning, MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    saveActions.saveAvailableData(loadedStudyPath);
                }
                else if (result == MessageBoxResult.No)
                {
                    saveActions.doSave();
                }

            }
        }


        private void LoadStudy_Click(object sender, RoutedEventArgs e)
        {
            loadActions.doLoad();
        }

        public void cleanVTKRender()
        {
            foreach (Geometry g in STLGeometries)
            {
                if (g.actor != null) renderer.RemoveActor(g.actor);
            }
            myRenderWindowControl.RenderWindow.Render();
            volumeMetricsItems.Items.Clear();
            SurfaceAreaMetricsItems.Items.Clear();
            geometryItems.Items.Clear();
            STLGeometries.Clear();
            ultrasound.bladderGeometryPath = null;
            photoAcoustic.thicknessGeometryPath = null;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (this.authentication.isAuthenticated)
            {
                //close logging process
                core.setLoggingOnOff(false);
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

                    Rat.Visibility = Visibility.Collapsed;
                    Viewer3D.Visibility = Visibility.Visible;
                    break;
                case twoStatesButton.states.twoDimensional:

                    Viewer3D.Visibility = Visibility.Collapsed;
                    Rat.Visibility = Visibility.Visible;
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
                case "Bladder":
                    return SaveActions.FileType.Bladder3D;
                case "Thickness":
                    return SaveActions.FileType.Thickness3D;
                case "Layer":
                    return SaveActions.FileType.Layer3D;
                case "OXY":
                    return SaveActions.FileType.OXY3D;
                case "DeOXY":
                    return SaveActions.FileType.DeOXY3D;
            }

            return SaveActions.FileType.Bladder3D; //never

        }
    }

}
