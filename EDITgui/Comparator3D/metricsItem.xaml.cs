using System;
using System.Collections.Generic;
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

namespace EDITgui
{
    /// <summary>
    /// Interaction logic for metricsItem.xaml
    /// </summary>
    public partial class metricsItem : UserControl
    {
       
        public metricsItem(String title)
        {
            InitializeComponent();
            this.title.Content = title;
        }

        public void visualizeMetrics(Geometry geometry)
        {
            switch (geometry.geometryName)
            {
                case Messages.bladderGeometry:
                    this.bladder_Volume.Content = geometry.volumeLabel.Content.ToString();
                    this.bladder_SurfaseArea.Content = geometry.surfaceAreaLabel.Content.ToString();
                    break;
                case Messages.outerWallGeometry:
                    this.OuterWall_Volume.Content = geometry.volumeLabel.Content.ToString();
                    this.OuterWall_SurfaseArea.Content = geometry.surfaceAreaLabel.Content.ToString();
                    break;
                case Messages.layerGeometry:
                    this.Layer_Volume.Content = geometry.volumeLabel.Content.ToString();
                    this.Layer_SurfaseArea.Content = geometry.surfaceAreaLabel.Content.ToString();
                    break;
            }
        }

        public void hideMetrics(Geometry geometry)
        {
            switch (geometry.geometryName)
            {
                case Messages.bladderGeometry:
                    this.bladder_Volume.Content = "-";
                    this.bladder_SurfaseArea.Content = "-";
                    break;
                case Messages.outerWallGeometry:
                    this.OuterWall_Volume.Content = "-";
                    this.OuterWall_SurfaseArea.Content = "-";
                    break;
                case Messages.layerGeometry:
                    this.Layer_Volume.Content = "-";
                    this.Layer_SurfaseArea.Content = "-";
                    break;
            }
        }

    }
}
