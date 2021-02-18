using Emgu.CV;
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
    /// Interaction logic for Comparator3D.xaml
    /// </summary>
    public partial class Comparator3D : UserControl
    {
        int count = 1000;
        Context context;

        public RenderWindowControl myRenderWindowControl;
        public List<GeometryItemsDropdown> studyItems = new List<GeometryItemsDropdown>();
        System.Windows.Forms.Integration.WindowsFormsHost host;
      
        public vtkRenderer renderer;
        vtkAxesActor axesActor;
        vtkOrientationMarkerWidget axes;

        public Comparator3D(Context context)
        {
            InitializeComponent();
            this.context = context;
            volumeLabel.Content = context.getMessages().volume + Environment.NewLine + "  (" + context.getMessages().mmB3 + ")";
            SurfaceAreaLabel.Content = context.getMessages().surface + Environment.NewLine + context.getMessages().area + " (" + context.getMessages().mmB2 + ")";
            volumeLabel.Visibility = Visibility.Collapsed;
            SurfaceAreaLabel.Visibility = Visibility.Collapsed;
        }

        private void Comparator_Loaded(object sender, RoutedEventArgs e)
        {
            host = new System.Windows.Forms.Integration.WindowsFormsHost();
            myRenderWindowControl = new RenderWindowControl();
            myRenderWindowControl.SetBounds(0, 0, 0, 0); // not too big in case it disappears.
                                                         // Assign the control as the host control's child.

            host.Child = myRenderWindowControl;

            host.Margin = new Thickness(0,0,0,0);
            host.HorizontalAlignment = HorizontalAlignment.Stretch;
            host.VerticalAlignment = VerticalAlignment.Stretch;
           
            rendererGrid.Children.Add(host);
            renderer = myRenderWindowControl.RenderWindow.GetRenderers().GetFirstRenderer();
            renderer.GetActiveCamera().SetPosition(0, 0, 50);
            renderer.GetActiveCamera().SetFocalPoint(0, 0, 5);
        }

        double rendererGridWidth;
        private void Comparator_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            rendererGridWidth = this.ActualWidth - infoGrid.ActualWidth - metricsPanel.ActualWidth;

            if (rendererGridWidth >= 0) rendererGrid.Width = rendererGridWidth - 1;
            if (this.ActualHeight >= 0) rendererGrid.Height = this.ActualHeight;
        }


        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog browserDialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = browserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                addStudy(browserDialog.SelectedPath);
            }
        }

        public void addStudy(string path)
        {
            GeometryItemsDropdown study = new GeometryItemsDropdown(context, this, this.StudiesPanel.Children.Count);
            bool geometriesWereFound = study.checkAndLoadAvailableGeometries(path);
 
            if (geometriesWereFound)
            {
                addDropdownToStudiesPanel(study);
                studyItems.Add(study);

                study.getStudyMetrics().HorizontalAlignment = HorizontalAlignment.Left;
                study.getStudyMetrics().VerticalAlignment = VerticalAlignment.Stretch;
                study.getStudyMetrics().Margin = new Thickness(0, 10, 0, 0);
                metricsPanel.Children.Add(study.getStudyMetrics());
                if (metricsPanel.Children.Count > 0)
                {
                    volumeLabel.Visibility = Visibility.Visible;
                    SurfaceAreaLabel.Visibility = Visibility.Visible;
                }
            }
            else
            {
                CustomMessageBox.Show(context.getMessages().noCorrectStudyFolder, context.getMessages().warning, MessageBoxButton.OK);
            }
        }

        public void removeStudy(GeometryItemsDropdown study)
        {
            StudiesPanel.Children.RemoveAt(study.studyIndex);
           // StudiesPanel.Children.Clear();
            metricsPanel.Children.Remove(study.getStudyMetrics());//RemoveAt(study.studyIndex);

            if (metricsPanel.Children.Count == 0)
            {
                volumeLabel.Visibility = Visibility.Collapsed;
                SurfaceAreaLabel.Visibility = Visibility.Collapsed;
            }

            foreach (Geometry geometry in study.STLGeometries)
            {
                if (geometry.actor != null)
                {
                    renderer.RemoveActor(geometry.actor);
                }
            }
            studyItems.Remove(study);

            int countNew = study.studyIndex;
            foreach (GeometryItemsDropdown existingStudy in studyItems)
            {
                if (existingStudy.studyIndex > countNew)
                {
                    existingStudy.updateIndex(countNew);
                    countNew = countNew + 1;
                }
            }
            if(studyItems.Any()) studyItems[0].Margin = new Thickness(0, 5, 0, 0);
            renderer.GetRenderWindow().Render();
        }

        private void addDropdownToStudiesPanel(GeometryItemsDropdown dropdown)
        {
            dropdown.HorizontalAlignment = HorizontalAlignment.Center;
            dropdown.VerticalAlignment = VerticalAlignment.Top;
            //Add geometries
            if (StudiesPanel.Children.Count > 0)
            {
                dropdown.Margin = new Thickness(0, -110, 0, 0);
            }
            else
            {
                dropdown.Margin = new Thickness(0, 5, 0, 0);
            }
            StackPanel.SetZIndex(dropdown, count); 
            count = count - 1;
            StudiesPanel.Children.Add(dropdown);
        }
    }
}
