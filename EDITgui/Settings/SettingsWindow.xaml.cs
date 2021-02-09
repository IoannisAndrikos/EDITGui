using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;

namespace EDITgui
{
    /// <summary>
    /// Interaction logic for Configurations.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        Settings settings;

        public MainWindow.Mode mode;

        public SettingsWindow()
        {
            InitializeComponent();
        }


        public SettingsWindow(Settings settings)
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.settings = settings;
            this.Visibility = Visibility.Visible;
        }

        public void setState(MainWindow.Mode state)
        {
            switch (state)
            {
                case MainWindow.Mode.AUTO:
                    mode = MainWindow.Mode.AUTO;
                    label_auto_manual.Content = "Auto";
                    Auto_manual.setCustomDotToLeft();
                    break;
                case MainWindow.Mode.ANOTATION:
                    mode = MainWindow.Mode.ANOTATION;
                    label_auto_manual.Content = "Annotation";
                    Auto_manual.setCustomDotToRight();
                    break;
            }
        }

        private void Auto_manual_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (mode)
            {
                case MainWindow.Mode.AUTO:
                    label_auto_manual.Content = "Annotation";
                    mode = MainWindow.Mode.ANOTATION;
                    break;
                case MainWindow.Mode.ANOTATION:
                    label_auto_manual.Content = "Auto";
                    mode = MainWindow.Mode.AUTO;
                    break;
            }
        }

   
        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            settings.doSave();
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            settings.onSettingsWindowClosing();
        }
    }
}
