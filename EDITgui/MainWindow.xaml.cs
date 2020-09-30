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
using Kitware.VTK;
using Kitware.mummy;

namespace EDITgui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        RenderWindowControl myRenderWindowControl;
        System.Windows.Forms.Integration.WindowsFormsHost host;
        UltrasoundPart ultrasound;
        PhotoAcousticPart photoAcoustic;
        vtkRenderer renderer;
        List<Geometry> STLGeometries = new List<Geometry>();

        public static int count = 0;

        public MainWindow()
        {
            InitializeComponent();
            coreFunctionality core = new coreFunctionality();
            core.setExaminationsDirectory("C:/Users/Legion Y540/Desktop/EDIT_STUDIES");

            ultrasound = new UltrasoundPart(this);
            photoAcoustic = new PhotoAcousticPart(this, ultrasound); //pass ultasound instance in to photoaccoustic in order to exchange data
          

            ultrasound.InitializeCoreFunctionality = core;
            photoAcoustic.InitializeCoreFunctionality = core;

            ultrasound.Width = 772;
            ultrasound.Margin = new Thickness(0, 0, 0, 0);
            ultrasound.HorizontalAlignment = HorizontalAlignment.Left;

            photoAcoustic.Margin = new Thickness(777, 0, -4.5, 0);
            photoAcoustic.Width = 738;

            this.Rat.Children.Add(ultrasound);
            this.Rat.Children.Add(photoAcoustic);

            UltrasoundPart.returnBladderSTL += OnAddAvailableGeometry;
            UltrasoundPart.returnSkinSTL += OnAddAvailableGeometry;
            PhotoAcousticPart.returnThicknessSTL += OnAddAvailableGeometry;
        }

        public void OnAddAvailableGeometry(Geometry geometry)
        {

            Geometry currentGeometry = STLGeometries.Find(x => x.TypeName == geometry.TypeName);
            

            if (currentGeometry != null)
            {
                STLGeometries.Remove(currentGeometry);
                geometry.checkbox = currentGeometry.checkbox;
                STLGeometries.Add(geometry);
                renderer.RemoveActor(currentGeometry.actor);
                visualizeGeometries(geometry);
                myRenderWindowControl.RenderWindow.Render();
            }
            else
            {
                CheckBox ch = new CheckBox();
                ch.Content = geometry.TypeName;
                geometry.checkbox = ch;
                STLGeometries.Add(geometry);
                this.STLcombobox.Items.Add(ch);
                ch.Checked += new RoutedEventHandler(OnGeometryComboboxChecked);
                ch.Unchecked += new RoutedEventHandler(OnGeometryComboboxUnchecked);
            }
           

        

            //manage STL combobox
            //this.STLcombobox.Items.Clear();
            //foreach(Geometry geom in STLGeometries)
            //{
            //    CheckBox ch = new CheckBox();
            //    ch.Content = geom.TypeName;
            //    this.STLcombobox.Items.Add(ch);
            //    ch.Checked += new RoutedEventHandler(OnGeometryComboboxChecked);
            //    ch.Unchecked += new RoutedEventHandler(OnGeometryComboboxUnchecked);
            //}
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the interop host control.
            host = new System.Windows.Forms.Integration.WindowsFormsHost();
            myRenderWindowControl = new RenderWindowControl();
            myRenderWindowControl.SetBounds(0, 0, 30, 30); // not too big in case it disappears.
                                                           // Assign the control as the host control's child.
            host.Child = myRenderWindowControl;

            host.Margin = new Thickness(0, 0, 0, 0);
            tempRenderer.Children.Add(host);
            renderer = myRenderWindowControl.RenderWindow.GetRenderers().GetFirstRenderer();

            renderer.GetActiveCamera().SetPosition(0, 0, 50);
            renderer.GetActiveCamera().SetFocalPoint(0, 0, 5);

            tempRenderer.Children.Remove(host);
            rendererGrid.Children.Add(host);

        }

        private void Viewer3D_Loaded(object sender, RoutedEventArgs e)
        {
          
        }

        public void OnGeometryComboboxChecked(object sender, RoutedEventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            Geometry geometry = STLGeometries.Find(x => x.TypeName == ch.Content);
            visualizeGeometries(geometry);
        }


        public void visualizeGeometries(Geometry geometry)
        {
            vtkSTLReader reader = vtkSTLReader.New();

            reader.SetFileName(geometry.Path);
            reader.Update();

            vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            mapper.SetInputConnection(reader.GetOutputPort());
            mapper.Update();

            vtkActor actor = vtkActor.New();
            actor.SetMapper(mapper);

            if (geometry.checkbox.IsChecked == true)
            {
                switch (geometry.TypeName)
                {
                    case "Bladder":
                        actor.GetProperty().SetColor(1, 1, 1);
                        actor.GetProperty().SetOpacity(1);
                        break;

                    case "Thickness":
                        actor.GetProperty().SetColor(1, 0, 0);
                        actor.GetProperty().SetOpacity(0.8);
                        break;
                    case "Skin":
                        actor.GetProperty().SetColor(0, 1, 0);
                        actor.GetProperty().SetOpacity(0.3);
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
            Geometry geometry = STLGeometries.Find(x => x.TypeName == ch.Content);
           

            if (geometry.checkbox.IsChecked == false)
            {
                renderer.RemoveActor(geometry.actor);
            }
            myRenderWindowControl.RenderWindow.Render();
        }
    }

    public class Geometry
    {
        public string TypeName { get; set; }
        public string Path { get; set; }
        public CheckBox checkbox { get; set; }
        public vtkActor actor { set; get; }
    }
}
