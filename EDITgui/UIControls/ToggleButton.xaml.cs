﻿using System;
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
    /// Interaction logic for ToggleButton.xaml
    /// </summary>
    public partial class ToggleButton : UserControl
    {
        Thickness LeftSide = new Thickness(-39, 0, 0, 0);
        Thickness RightSide = new Thickness(0, 0, -39, 0);
        SolidColorBrush Off = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF333333"));
        SolidColorBrush On = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF333333"));
        private bool Toggled = false;

        public ToggleButton()
        {
            InitializeComponent();
            Back.Fill = Off;
            Toggled = false;
            Dot.Margin = LeftSide;
        }

        public bool Toggled1 { get => Toggled; set => Toggled = value; }

        private void Dot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Toggled)
            {
                Back.Fill = On;
                Toggled = true;
                Dot.Margin = RightSide;

            }
            else
            {

                Back.Fill = Off;
                Toggled = false;
                Dot.Margin = LeftSide;

            }
        }

        private void Back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Toggled)
            {
                Back.Fill = On;
                Toggled = true;
                Dot.Margin = RightSide;

            }
            else
            {

                Back.Fill = Off;
                Toggled = false;
                Dot.Margin = LeftSide;

            }

        }

        public void setCustomDotToRight()
        {
            Dot.Margin = RightSide;
            Toggled = true;
        }

        public void setCustomDotToLeft()
        {
            Dot.Margin = LeftSide;
            Toggled = false;
        }


        public void setCustomDotToRightAndRed()
        {
            Dot.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0A25D1"));
            Dot.Margin = RightSide;
            Toggled = true;
        }

        public void setCustomDotToLeftAndBlue()
        {
            Dot.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCB2525"));
            Dot.Margin = LeftSide;
            Toggled = false;
        }

        //public void setColor(String hexColor)
        //{
        //    Dot.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
        //}


        //public void setDotToRight()
        //{
        //    Dot.Margin = RightSide;
        //    Toggled = true;
        //}

        //public void setDotToLeft()
        //{
        //    Dot.Margin = LeftSide;
        //    Toggled = false;
        //}


    }
}
