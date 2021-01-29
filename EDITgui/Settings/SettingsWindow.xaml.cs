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

        public MainWindow.process currentProcess;

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

        public void setState(MainWindow.process state)
        {
            switch (state)
            {
                case MainWindow.process.AUTO:
                    currentProcess = MainWindow.process.AUTO;
                    label_auto_manual.Content = "Auto";
                    Auto_manual.setCustomDotToLeft();
                    break;
                case MainWindow.process.ANOTATION:
                    currentProcess = MainWindow.process.ANOTATION;
                    label_auto_manual.Content = "Annotation";
                    Auto_manual.setCustomDotToRight();
                    break;
            }
        }

        private void Auto_manual_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (currentProcess)
            {
                case MainWindow.process.AUTO:
                    label_auto_manual.Content = "Annotation";
                    currentProcess = MainWindow.process.ANOTATION;
                    break;
                case MainWindow.process.ANOTATION:
                    label_auto_manual.Content = "Auto";
                    currentProcess = MainWindow.process.AUTO;
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
