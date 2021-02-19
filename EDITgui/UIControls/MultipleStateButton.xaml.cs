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
    /// Interaction logic for MultipleStateButton.xaml
    /// </summary>
    public partial class MultipleStateButton : UserControl
    {
        SolidColorBrush gray = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF333333"));
        SolidColorBrush green = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF335B5B"));

        public enum states { twoDimensional, threeDimensional, compare };

        public states currentState;

        public MultipleStateButton()
        {
            InitializeComponent();
            currentState = states.twoDimensional;
        }

        private void do2D()
        {
            currentState = states.twoDimensional;
            recBackMiddle.Fill = gray;
            recBackRight.Fill = gray;
            recFrontRight.Fill = gray;
            recFrontLeft.Fill = green;
            recBackLeft.Fill = green;
        }

     
        private void do3D()
        {
            currentState = states.threeDimensional;
            recBackMiddle.Fill = green;
            recBackRight.Fill = gray;
            recFrontRight.Fill = gray;
            recFrontLeft.Fill = gray;
            recBackLeft.Fill = gray;
        }


        private void doCompare()
        {
            currentState = states.compare;
            recBackMiddle.Fill = gray;
            recBackRight.Fill = green;
            recFrontRight.Fill = green;
            recFrontLeft.Fill = gray;
            recBackLeft.Fill = gray;
        }


        private void Left_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            do2D();
        }

        private void Middle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            do3D();
        }

        private void Right_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            doCompare();
        }
    }
}
