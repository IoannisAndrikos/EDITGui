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
    /// Interaction logic for FrameMarker.xaml
    /// </summary>
    public partial class FrameMarker : UserControl
    {

        public enum Type { starting, ending}

        public FrameMarker()
        {
            InitializeComponent();
        }

        public FrameMarker(Type type)
        {
            InitializeComponent();
            if (type == Type.starting)
            {
                rec1.Fill = ViewAspects.greenMarker;
                rec2.Fill = ViewAspects.greenMarker;
            } else if (type == Type.ending)
            {
                rec1.Fill = ViewAspects.redMarker;
                rec2.Fill = ViewAspects.redMarker;
            }
        }
    }
}
