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
    /// Interaction logic for twoStatesButton.xaml
    /// </summary>
    public partial class twoStatesButton : UserControl
    {
        public enum states { twoDimensional, threeDimensional };

        public states currentState;

        public twoStatesButton()
        {
            InitializeComponent();
            currentState = states.twoDimensional;
        }

        SolidColorBrush gray = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF333333"));
        SolidColorBrush green = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF335B5B"));


        private void do2D()
        {
            currentState = states.twoDimensional;
            rec3DBack.Fill = gray;
            rec3DFront.Fill = gray;
            rec2DBack.Fill = green;
            rec2DFront.Fill = green;
        }

        private void do3D()
        {
            currentState = states.threeDimensional;
            rec2DBack.Fill = gray;
            rec2DFront.Fill = gray;
            rec3DBack.Fill = green;
            rec3DFront.Fill = green;
        }

        private void Rec2DFront_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            do2D();
        }

        private void D2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            do2D();
        }

        private void Rec2DBack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            do2D();
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            do2D();
        }

        private void Left_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            do2D();
        }

        private void D3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            do3D();
        }

        private void Rec3DFront_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            do3D();
        }

        private void Rec3DBack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            do3D();
        }

        private void Label_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            do3D();
        }

    }
}
