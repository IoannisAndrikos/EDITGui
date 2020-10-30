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
using Emgu.CV.Shape;
using Kitware.VTK;
using Kitware.mummy;
using System.Globalization;
using Path = System.IO.Path;

namespace EDITgui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

       public string workingPath;
        public string workingDir = "Edit_Current_working_study";
        RenderWindowControl myRenderWindowControl;
        System.Windows.Forms.Integration.WindowsFormsHost host;
        UltrasoundPart ultrasound;
        PhotoAcousticPart photoAcoustic;
        vtkRenderer renderer;
        List<Geometry> STLGeometries = new List<Geometry>();

        SaveActions saveActions = new SaveActions();
        coreFunctionality core;

        public static int count = 0;

        Messages messages = new Messages();

        public MainWindow()
        {
            InitializeComponent();
            core = new coreFunctionality();
            Console.WriteLine(Path.GetTempPath());

            //workingPath = "C:/Users/Legion Y540/Desktop/EDIT_STUDIES" + System.IO.Path.DirectorySeparatorChar + workingDir;
            workingPath = Path.GetTempPath() + workingDir;

            core.setExaminationsDirectory(workingPath);
            //core.setExaminationsDirectory("//Mac/Home/Downloads/export");

            ultrasound = new UltrasoundPart(this);
            photoAcoustic = new PhotoAcousticPart(this, ultrasound); //pass ultasound instance in to photoaccoustic in order to exchange data
         

            ultrasound.InitializeCoreFunctionality = core;
            photoAcoustic.InitializeCoreFunctionality = core;

            ultrasound.Width = 772;
            ultrasound.Margin = new Thickness(0, 0, 0, 0);
            ultrasound.HorizontalAlignment = HorizontalAlignment.Left;

            photoAcoustic.Margin = new Thickness(777, 0, 0, 0);
            photoAcoustic.Width = 738;

            this.Rat.Children.Add(ultrasound);
            this.Rat.Children.Add(photoAcoustic);

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
            visualizeGeometries(geometry);
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


        private void Bu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (labelView.Content)
            {
                case "2D":
                    labelView.Content = "3D";
                    Rat.Visibility = Visibility.Hidden;
                    Viewer3D.Visibility = Visibility.Visible;
                    break;
                case "3D":
                    labelView.Content = "2D";
                    Viewer3D.Visibility = Visibility.Hidden;
                    Rat.Visibility = Visibility.Visible;
                    break;
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
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.CheckFileExists = false;
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.Title = messages.saveStudy;
            if (saveFileDialog.ShowDialog() == true)
            {
                //if (saveFileDialog.FileName != null)
                {
                    //save bladder
                    saveActions.writePointsToTXT(saveFileDialog.FileName, ultrasound.getBladderPoints(), SaveActions.FileType.bladderPoints);
                    saveActions.writeMetricsToTXT(saveFileDialog.FileName, ultrasound.getBladderArea(), SaveActions.FileType.Bladder2DArea);
                    saveActions.writeMetricsToTXT(saveFileDialog.FileName, ultrasound.getBladderPerimeter(), SaveActions.FileType.Bladder2DPerimeter);

                    //save thickness
                    saveActions.writePointsToTXT(saveFileDialog.FileName, photoAcoustic.getThicknessPoints(), SaveActions.FileType.thicknessPoints);
                    saveActions.writeMetricsToTXT(saveFileDialog.FileName, photoAcoustic.getMeanThickness(), SaveActions.FileType.MeanThickness);

                    //save geometries
                    foreach (Geometry geometry in STLGeometries)
                    {
                        saveActions.copyFileToFolderOfStudy(saveFileDialog.FileName, geometry.Path, geometry.getSaveActionFileType());
                    }

                    //save Dicom
                    if (ultrasound.ultrasoundDicomFile != null) saveActions.copyFileToFolderOfStudy(saveFileDialog.FileName, ultrasound.ultrasoundDicomFile, SaveActions.FileType.UltrasoundDicomFile);
                    if (photoAcoustic.OXYDicomFile != null) saveActions.copyFileToFolderOfStudy(saveFileDialog.FileName, photoAcoustic.OXYDicomFile, SaveActions.FileType.OXYDicomFile);
                    if (photoAcoustic.DeOXYDicomFile != null) saveActions.copyFileToFolderOfStudy(saveFileDialog.FileName, photoAcoustic.DeOXYDicomFile, SaveActions.FileType.DeOXYDicomFile);

                    //save logfiles
                    saveActions.copyLogFilesToFolderOfStudy(saveFileDialog.FileName, workingPath);

                    //save info
                    saveActions.writeInfoTXTFile(saveFileDialog.FileName, collectAllStudyInfo(), SaveActions.FileType.info);
                }
            }
        }


        private List<StudyInfo> collectAllStudyInfo()
        {
            List<StudyInfo> studyInfo = new List<StudyInfo>();
            studyInfo.Add(new StudyInfo() { infoName = messages.startingFrame, infoValue = ultrasound.startingFrame });
            studyInfo.Add(new StudyInfo() { infoName = messages.endingFrame, infoValue = ultrasound.endingFrame });

            return studyInfo;
        }

        private void Window_Closed(object sender, EventArgs e)
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

    public class StudyInfo
    {
        public string infoName { get; set; }
        public double infoValue { get; set; }
    }


}
