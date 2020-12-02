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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EDITgui
{
    /// <summary>
    /// Interaction logic for settings.xaml
    /// </summary>
    public partial class settings : UserControl
    {
        MainWindow mainWindow;
        UltrasoundPart ultrasound;
        PhotoAcousticPart photoAcoustic;
        coreFunctionality core;
        Messages messages;

        double xspace = 0;
        double yspace = 0;
        double distaceBetweenFrames;

        public settings()
        {
            InitializeComponent();
        }

        public settings(MainWindow mainWindow, coreFunctionality coreFunctionality, UltrasoundPart ultrasound, PhotoAcousticPart photoAcoustic)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            this.ultrasound = ultrasound;
            this.photoAcoustic = photoAcoustic;
            this.core = coreFunctionality;
            this.messages = new Messages();
        }


        private void Auto_manual_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (label_auto_manual.Content)
            {
                case "Auto":
                    label_auto_manual.Content = "Annotation";
                    doManual();
                    break;
                case "Annotation":
                    label_auto_manual.Content = "Auto";
                    doAuto();
                    break;
            }
        }

        private void Gear_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (settingsPanel.Visibility == Visibility.Collapsed)
            {
                settingsPanel.Visibility = Visibility.Visible;
                gear.setActive();
            }
            else if (settingsPanel.Visibility == Visibility.Visible)
            {
                settingsPanel.Visibility = Visibility.Collapsed;
                gear.setNonActive();
                doSave();
            }
        }

        private void doManual()
        {
            mainWindow.currentProcess = MainWindow.process.ANOTATION;
            ultrasound.extract_bladder.IsEnabled = false;
            photoAcoustic.extract_thickness.IsEnabled = false;
            photoAcoustic.recalculate.IsEnabled = false;
            ultrasound.doCorrection();
            photoAcoustic.doCorrection();
        }


        private void doAuto()
        {
            mainWindow.currentProcess = MainWindow.process.AUTO;
            ultrasound.extract_bladder.IsEnabled = true;
            photoAcoustic.extract_thickness.IsEnabled = true;
            photoAcoustic.recalculate.IsEnabled = true;
            ultrasound.doRepeatProcess();
        }

        public void setPixelSpacing(List<double> pixelsSpacing){
            textbox_pixelSpacingX.Text = pixelsSpacing[0].ToString();
            textbox_pixelSpacingY.Text = pixelsSpacing[1].ToString();
        }

       
        private void doSave()
        {  
            try
            {
                distaceBetweenFrames = double.Parse(textbox_distanceBetweenFrames.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                xspace = double.Parse(textbox_pixelSpacingX.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                yspace = double.Parse(textbox_pixelSpacingY.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                core.setStudySettings(distaceBetweenFrames, xspace, yspace);
            }
            catch(Exception ex)
            {
                CustomMessageBox.Show(messages.correctDoubleFormat, messages.warning, MessageBoxButton.OK);
            }

            
        }

    }
}
