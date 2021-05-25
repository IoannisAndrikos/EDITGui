using System;
using System.Collections.Generic;
using System.Globalization;
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
using Kitware.mummy;
using Kitware.VTK;
using Path = System.IO.Path;

namespace EDITgui
{
    /// <summary>
    /// Interaction logic for GeometryItemsDropdown.xaml
    /// </summary>
    public partial class GeometryItemsDropdown : UserControl
    {
        public string studyPath;
        StudyFile toolPaths;
        public List<Geometry> STLGeometries = new List<Geometry>();
        private Context context;
        metricsItem studyMetrics;
        private Comparator3D comparator;
        vtkIterativeClosestPointTransform icp;
        vtkTransformPolyDataFilter icpTransformFilter;
 
        public int studyIndex;

        private int rotationZdegrees = 0;



        public GeometryItemsDropdown(Context context, Comparator3D comparator, int index)
        {
            InitializeComponent();
            this.comparator = comparator;
            this.context = context;
            this.studyIndex = index;
            this.toolPaths = new StudyFile();
            Content.Visibility = Visibility.Collapsed;
            openCloseButton.Content = "\u2B9f";
        }

        public metricsItem getStudyMetrics()
        {
            return this.studyMetrics;
        }

        private void OpenCloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Content.Visibility == Visibility.Collapsed)
            {
                openCloseButton.Content = "\u2B9d";
                Content.Visibility = Visibility.Visible;
                studyInfoPanel.Fill = ViewAspects.greenUI; 
            }
            else
            {
                openCloseButton.Content = "\u2B9f";
                Content.Visibility = Visibility.Collapsed;
                studyInfoPanel.Fill = ViewAspects.blackUI;
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            comparator.removeStudy(this);
        }


        public void updateIndex(int index)
        {
            foreach(Geometry geometry in STLGeometries)
            {
                if (geometry.checkbox.IsChecked == true)
                {
                    vtkTransform translateAlongWithXAxis = vtkTransform.New();
                    translateAlongWithXAxis.Translate((studyIndex-1)*15, 0, 0);
                    translateAlongWithXAxis.Update();
                    geometry.actor.SetUserTransform(translateAlongWithXAxis);
                }
            }
            this.studyIndex = index;
        }

        public bool checkAndLoadAvailableGeometries(string path)
        {
            this.studyPath = path;
            bool geometriesWereFound = false;

            title.Content = Path.GetFileNameWithoutExtension(path);

            string object3D;
            EDITgui.Geometry geometry;

            string Oblect3DFile = toolPaths.getFolderName(studyPath, StudyFile.FileType.Bladder3D, false) + toolPaths.getProperFileName(StudyFile.FileType.Bladder3D);
            if (File.Exists(Oblect3DFile))
            {
                geometry = new Geometry() { geometryName = Messages.bladderGeometry, Path = Oblect3DFile, actor = null };
                OnAddAvailableGeometry(geometry);
                geometriesWereFound = true;
            }

            Oblect3DFile = toolPaths.getFolderName(studyPath, StudyFile.FileType.Thickness3D, false) + toolPaths.getProperFileName(StudyFile.FileType.Thickness3D);
            if (File.Exists(Oblect3DFile))
            {
                geometry = new Geometry() { geometryName = Messages.outerWallGeometry, Path = Oblect3DFile, actor = null };
                OnAddAvailableGeometry(geometry);
                geometriesWereFound = true;
            }
            Oblect3DFile = toolPaths.getFolderName(studyPath, StudyFile.FileType.Layer3D, false) + toolPaths.getProperFileName(StudyFile.FileType.Layer3D);
            if (File.Exists(Oblect3DFile))
            {
                geometry = new Geometry() { geometryName = Messages.layerGeometry, Path = Oblect3DFile, actor = null };
                OnAddAvailableGeometry(geometry);
                geometriesWereFound = true;
            }
            Oblect3DFile = toolPaths.getFolderName(studyPath, StudyFile.FileType.OXY3D, false) + toolPaths.getProperFileName(StudyFile.FileType.OXY3D);
            if (File.Exists(Oblect3DFile))
            {
                geometry = new Geometry() { geometryName = Messages.oxyGeometry, Path = Oblect3DFile, actor = null };
                OnAddAvailableGeometry(geometry);
                geometriesWereFound = true;
            }
            Oblect3DFile = toolPaths.getFolderName(studyPath, StudyFile.FileType.DeOXY3D, false) + toolPaths.getProperFileName(StudyFile.FileType.DeOXY3D);
            if (File.Exists(Oblect3DFile))
            {
                geometry = new Geometry() { geometryName = Messages.deoxyGeometry, Path = Oblect3DFile, actor = null };
                OnAddAvailableGeometry(geometry);
                geometriesWereFound = true;
            }
            if (geometriesWereFound)
            {
                if (studyIndex > 0) findICPTransformationMatrix();
                studyMetrics = new metricsItem(this.title.Content.ToString());
            }

            return geometriesWereFound;
        }


        public void OnAddAvailableGeometry(Geometry geometry)
        {      
            string extension = Path.GetExtension(geometry.Path);

            CheckBox ch = new CheckBox();
            ch.Style = (Style)FindResource("CheckBoxStyle1");
            ch.Foreground = Brushes.White;
            if (extension == ".stl")
            {
                Label volumeLabel = new Label();
                volumeLabel.Foreground = Brushes.White;
                Label surfaceAreaLabel = new Label();
                surfaceAreaLabel.Foreground = Brushes.White;
                geometry.volumeLabel = volumeLabel;
                geometry.surfaceAreaLabel = surfaceAreaLabel;
                //this.volumeMetricsItems.Items.Add(volumeLabel);
                //this.SurfaceAreaMetricsItems.Items.Add(surfaceAreaLabel);
            }

            geometry.checkbox = ch;
            ch.Content = geometry.geometryName;
            ch.Margin = new Thickness(8, 8, 0, 0);
            ch.HorizontalAlignment = HorizontalAlignment.Stretch;
            ch.VerticalAlignment = VerticalAlignment.Stretch;
            STLGeometries.Add(geometry);
            this.geometriesPanel.Children.Add(ch);

            ch.Checked += new RoutedEventHandler(OnGeometryComboboxChecked);
            ch.Unchecked += new RoutedEventHandler(OnGeometryComboboxUnchecked);
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
            }
            catch (Exception)
            {
                CustomMessageBox.Show(context.getMessages().noObject3DLoaded, context.getMessages().warning, MessageBoxButton.OK);
            }

        }

        public void visualizeGeometries(Geometry geometry, bool updateRenderer = true)
        {
            vtkSTLReader reader = vtkSTLReader.New();
            vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            string extension = Path.GetExtension(geometry.Path);

            vtkPolyData polyData = vtkPolyData.New();

            if (extension == ".stl")
            {
                reader.SetFileName(geometry.Path);
                reader.Update();
                if (geometry.checkbox.IsChecked == true)
                {
                    vtkMassProperties mass = vtkMassProperties.New();
                    polyData = reader.GetOutput();
                    mass.SetInput(reader.GetOutput());
                    mass.Update();
                    geometry.volumeLabel.Content = Math.Round(mass.GetVolume(), 2);
                    geometry.surfaceAreaLabel.Content = Math.Round(mass.GetSurfaceArea(), 2);
                    studyMetrics.visualizeMetrics(geometry);
                }
                mapper.SetInput(reader.GetOutput());
                mapper.Update();

            }
            else if (extension == ".txt")
            {
                polyData.SetPoints(txtPointsToPolyData(geometry.Path));
                vtkVertexGlyphFilter glyphFilter = vtkVertexGlyphFilter.New();
                glyphFilter.SetInput(polyData);
                glyphFilter.Update();
                //mapper.SetInputConnection(glyphFilter.GetOutputPort());
                polyData = glyphFilter.GetOutput();
                mapper.SetInput(glyphFilter.GetOutput());
            }

            if (studyIndex > 0 && icp != null)
            {
                icpTransformFilter = vtkTransformPolyDataFilter.New();
                if (rotationZdegrees == 180)
                {
                    vtkTransform transform = vtkTransform.New();
                    transform.RotateZ(rotationZdegrees);
                    vtkTransformPolyDataFilter rotation = vtkTransformPolyDataFilter.New();
                    rotation.SetTransform(transform);
                    rotation.SetInput(polyData);
                    rotation.Update();

                    icpTransformFilter.SetInput(rotation.GetOutput());
                }
                else
                {
                    icpTransformFilter.SetInput(polyData);
                }   
                
                icpTransformFilter.SetTransform(icp);
                icpTransformFilter.Update();
                mapper.SetInputConnection(icpTransformFilter.GetOutputPort());
            }

            vtkTransform translateAlongWithXAxis = vtkTransform.New();
            translateAlongWithXAxis.Translate(studyIndex * 15, 0, 0);
            translateAlongWithXAxis.Update();

            vtkActor actor = vtkActor.New();
            actor.SetMapper(mapper);
            geometry.actor = actor;

            //translate geometry along with x-axis
            geometry.actor.SetUserTransform(translateAlongWithXAxis);

            context.getPallet().updateGeometryColor(geometry);

            if (geometry.checkbox.IsChecked == true)
            {
                comparator.renderer.AddActor(geometry.actor);
            }

            if (updateRenderer)
            {
                comparator.renderer.ResetCamera();
                comparator.renderer.GetRenderWindow().Render();
            }
        }


        public void OnGeometryComboboxUnchecked(object sender, RoutedEventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            Geometry geometry = STLGeometries.Find(x => x.geometryName == ch.Content);
            string extension = Path.GetExtension(geometry.Path);
            studyMetrics.hideMetrics(geometry);

            if (geometry.checkbox.IsChecked == false)
            {
                comparator.renderer.RemoveActor(geometry.actor);
            }
            comparator.renderer.GetRenderWindow().Render();
        }


        private void findICPTransformationMatrix()
        {

            vtkSTLReader STLreaderTarget = vtkSTLReader.New();
            STLreaderTarget.SetFileName(comparator.studyItems[0].STLGeometries.Find(x => x.geometryName == Messages.bladderGeometry).Path);
            STLreaderTarget.Update();
            vtkPolyData polyDataTarget = vtkPolyData.New();
            polyDataTarget = STLreaderTarget.GetOutput();

            vtkSTLReader STLreaderSolution = vtkSTLReader.New();
            STLreaderSolution.SetFileName(this.STLGeometries.Find(x => x.geometryName == Messages.bladderGeometry).Path);
            STLreaderSolution.Update();
            vtkPolyData polyDataSolution = vtkPolyData.New();
            polyDataSolution = STLreaderSolution.GetOutput();


            icp = vtkIterativeClosestPointTransform.New();

            if (rotationZdegrees == 180)
            {
                vtkTransform transform = vtkTransform.New();
                transform.RotateZ(rotationZdegrees);
                vtkTransformPolyDataFilter rotation = vtkTransformPolyDataFilter.New();
                rotation.SetTransform(transform);
                rotation.SetInput(polyDataSolution);

                rotation.Update();

                icp.SetSource(rotation.GetOutput());
            }
            else if (rotationZdegrees == 0)
            {
                icp.SetSource(polyDataSolution);
            }
          
            icp.SetTarget(polyDataTarget);
            icp.GetLandmarkTransform().SetModeToRigidBody();
            //icp.SetMaximumNumberOfIterations(1000);
            icp.StartByMatchingCentroidsOff();
            icp.Modified();
            icp.Update();

            //Console.WriteLine(icp.GetMatrix());
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

     

        private void Rotate3D_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (rotationZdegrees == 0)
            {
                rotationZdegrees = 180;
            }
            else
            {
                rotationZdegrees = 0;
            }
            findICPTransformationMatrix();

            foreach (Geometry geometry in this.STLGeometries)
            {

                if (geometry.checkbox.IsChecked == true)
                {
                    comparator.renderer.RemoveActor(geometry.actor);
                    visualizeGeometries(geometry, false);
                }
            }

            comparator.renderer.GetRenderWindow().Render();
        }
    }
}
