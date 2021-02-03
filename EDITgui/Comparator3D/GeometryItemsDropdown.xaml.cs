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
        private Comparator3D comparator;

        public GeometryItemsDropdown(Context context, Comparator3D comparator)
        {
            InitializeComponent();
            this.comparator = comparator;
            this.context = context;
            this.toolPaths = new StudyFile();
            Content.Visibility = Visibility.Collapsed;
            openCloseButton.Content = "\u2B9f";
        }

        private void OpenCloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Content.Visibility == Visibility.Collapsed)
            {
                openCloseButton.Content = "\u2B9d";
                Content.Visibility = Visibility.Visible;
            }
            else
            {
                openCloseButton.Content = "\u2B9f";
                Content.Visibility = Visibility.Collapsed;
            }
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

        public void visualizeGeometries(Geometry geometry)
        {
            vtkSTLReader reader = vtkSTLReader.New();
            vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            string extension = Path.GetExtension(geometry.Path);

            if (extension == ".stl")
            {
                reader.SetFileName(geometry.Path);
                reader.Update();
                if (geometry.checkbox.IsChecked == true)
                {
                    vtkMassProperties mass = vtkMassProperties.New();
                    mass.SetInput(reader.GetOutput());
                    mass.Update();
                    geometry.volumeLabel.Content = geometry.geometryName.ToString() + " = " + Math.Round(mass.GetVolume(), 2) + " mm\xB3";
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
               comparator.renderer.AddActor(actor);
            }

            comparator.renderer.GetRenderWindow().Render();
           // comparator.myRenderWindowControl.RenderWindow.Render();
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
                comparator.renderer.RemoveActor(geometry.actor);
            }
            comparator.renderer.GetRenderWindow().Render();
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


    }
}
