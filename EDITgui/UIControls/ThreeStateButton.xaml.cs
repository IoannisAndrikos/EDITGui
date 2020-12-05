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
    /// Interaction logic for ThreeStateButton.xaml
    /// </summary>
    public partial class ThreeStateButton : UserControl
    {
        public enum CurrentState { CORRECTION, MANUAL, FILL_POINTS}

        CurrentState myState;

        public ThreeStateButton()
        {
            InitializeComponent();
            myState = CurrentState.CORRECTION;
        }



        public void doFillPointState()
        {
            myState = CurrentState.FILL_POINTS;
            content.Content = "Interpolation";
            back3.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5B5B33"));

        }

        public void doCorrectionState()
        {
            myState = CurrentState.CORRECTION;
            content.Content = "Correction";
            back3.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1C1F1F"));
        }


        public void doManualState()
        {
            myState = CurrentState.MANUAL;
            content.Content = "Manual";
            back3.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5A2531"));
        }


        public CurrentState getState()
        {
            return myState;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (myState)
            {
                case CurrentState.CORRECTION:
                    doManualState();
                    break;
                case CurrentState.MANUAL:
                    doCorrectionState();
                    break;
                case CurrentState.FILL_POINTS:
                    doCorrectionState();
                    break;
            }
        }
    }
}
