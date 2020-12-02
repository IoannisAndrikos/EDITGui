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
    /// Interaction logic for gearButton.xaml
    /// </summary>
    public partial class gearButton : UserControl
    {
        public gearButton()
        {
            InitializeComponent();
        }


        public void setActive()
        {
            gear.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF335B5B"));
        }

        public void setNonActive()
        {
            gear.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF333333")); 
        }


    }
}
