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
        int count = 100;
        Context context;

        public RenderWindowControl myRenderWindowControl;
        List<UIElement> studyItems = new List<UIElement>();
        System.Windows.Forms.Integration.WindowsFormsHost host;
      
        public vtkRenderer renderer;
        vtkAxesActor axesActor;
        vtkOrientationMarkerWidget axes;

        public Comparator3D(Context context)
        {
            InitializeComponent();
            this.context = context;
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
            rendererGridWidth = this.ActualWidth - infoGrid.ActualWidth;

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
            GeometryItemsDropdown dropdown = new GeometryItemsDropdown(context, this);
            dropdown.HorizontalAlignment = HorizontalAlignment.Center;
            dropdown.VerticalAlignment = VerticalAlignment.Top;

            bool geometriesWereFound = dropdown.checkAndLoadAvailableGeometries(path);

            if (geometriesWereFound)
            {
                //Add geometries

                if (StudiesPanel.Children.Count >= 1)
                {
                    dropdown.Margin = new Thickness(0, -120, 0, 0);
                }

                StackPanel.SetZIndex(dropdown, count);
                count = count - 1;
                StudiesPanel.Children.Add(dropdown);
            }
            else
            {
                CustomMessageBox.Show(context.getMessages().noCorrectStudyFolder, context.getMessages().warning, MessageBoxButton.OK);
            }
        }

    }
}
