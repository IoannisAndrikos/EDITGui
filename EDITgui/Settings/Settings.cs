using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EDITgui
{
    public class Settings
    {
        Context context;
        SettingsWindow settingsWindow;

        public double xspace = 0;
        public double yspace = 0;

        public double xdim = 0;
        public double ydim = 0; 

        public double distaceBetweenFrames = 0.203;


        public Settings(Context context)
        {
            this.context = context;
        }

        public void CreateSettingsWindow()
        {
            if (settingsWindow == null)
            {
                this.settingsWindow = new SettingsWindow(this);
                settingsWindow.textbox_distanceBetweenFrames.Text = this.distaceBetweenFrames.ToString();
                settingsWindow.textbox_pixelSpacingX.Text = this.xspace.ToString();
                settingsWindow.textbox_pixelSpacingY.Text = this.yspace.ToString();
                settingsWindow.setState(context.getMainWindow().currentMode);
                settingsWindow.Visibility = Visibility.Visible;

            }
        }


        private void doAutoOrManual(MainWindow.Mode process)
        {
            if(process == MainWindow.Mode.AUTO)
            {
                context.getMainWindow().currentMode = MainWindow.Mode.AUTO;
                context.getUltrasoundPart().extract_bladder.IsEnabled = true;
                context.getPhotoAcousticPart().extract_thickness.IsEnabled = true;
                context.getPhotoAcousticPart().recalculate.IsEnabled = true;
                context.getUltrasoundPart().doRepeatProcess();
            }
            else if(process == MainWindow.Mode.ANOTATION)
            {
                context.getMainWindow().currentMode = MainWindow.Mode.ANOTATION;
                context.getUltrasoundPart().extract_bladder.IsEnabled = false;
                context.getPhotoAcousticPart().extract_thickness.IsEnabled = false;
                context.getPhotoAcousticPart().recalculate.IsEnabled = false;
                context.getUltrasoundPart().doRepeatProcess();
                //context.getUltrasoundPart().doCorrection();
                //context.getPhotoAcousticPart().doCorrection();
            }
           
        }

        public void setImageDimsAndPixelSpacing(List<double> pixelsSpacing, List<double> dims)
        {
            this.xspace = pixelsSpacing[0];
            this.yspace = pixelsSpacing[1];

            this.xdim = dims[0];
            this.ydim = dims[1];
        }


        public void doSave()
        {
            try
            {

                if (settingsWindow.mode != context.getMainWindow().currentMode)
                {
                    MessageBoxResult result = CustomMessageBox.Show(context.getMessages().getMessageAfterChangingSystemMode(settingsWindow.mode, context.getMainWindow().currentMode), context.getMessages().warning, MessageBoxButton.YesNoCancel);
                    if (result == MessageBoxResult.Yes)
                    {
                        doAutoOrManual(settingsWindow.mode);
                        this.distaceBetweenFrames = double.Parse(this.settingsWindow.textbox_distanceBetweenFrames.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                        this.xspace = double.Parse(this.settingsWindow.textbox_pixelSpacingX.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                        this.yspace = double.Parse(this.settingsWindow.textbox_pixelSpacingY.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                        context.getCore().setStudySettings(distaceBetweenFrames, xspace, yspace);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(context.getMessages().correctDoubleFormat, context.getMessages().warning, MessageBoxButton.OK);
            }
            onSettingsWindowClosing();
        }


        public void onSettingsWindowClosing()
        {
            settingsWindow = null;
        }

    }
}
