using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using Path = System.IO.Path;

namespace EDITgui
{
    /// <summary>
    /// Interaction logic for multipleCheckboxes.xaml
    /// </summary>
    public partial class managePhotoacousticImaging : UserControl
    {
        Context context;

        public string OXY_ImagesDir = null;
        public string DeOXY_ImagesDir = null;
        public string GNR_ImagesDir = null;

        public string OXYDicomFile = null;
        public string DeOXYDicomFile = null;
        public string GNRDicomFile = null;

        public string OXYStudyname = null;
        public string DeOXYStudyname = null;
        public string GNRStudyname = null;


        List<double> pixelSpacing = new List<double>(); //x=pixelSpacing[0] y=pixelSpacing[1]
        List<double> imageSize = new List<double>();


        public managePhotoacousticImaging(Context context)
        {
            this.context = context;
            InitializeComponent();
            OXY_Checkbox.IsEnabled = false;
            DeOXY_Checkbox.IsEnabled = false;
            GNR_Checkbox.IsEnabled = false;
        }

        private async void LoadΟΧΥDicom(object sender, RoutedEventArgs e)
        {
            if (context.getUltrasoundPart().ultrasoundDicomFile == null)
            {
                CustomMessageBox.Show(context.getMessages().loadOXYWithoutUltraasound, context.getMessages().warning, MessageBoxButton.OK);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = context.getMessages().selectDicom;
            if (openFileDialog.ShowDialog() == true)
            {
                context.getPhotoAcousticPart().startSpinner();
                context.getCore().repeatSegmentation();
                OXYDicomFile = null;

                await Task.Run(() =>
                {
                    string dicomPath = context.getStudyFile().copyFileToWorkspace(context.getStudyFile().getWorkspaceDicomPath(), openFileDialog.FileName, StudyFile.FileType.OXYDicomFile);
                    OXY_ImagesDir = context.getCore().exportOXYImages(dicomPath, true); //enablelogging = true
                    pixelSpacing = context.getCore().pixelSpacing;
                    imageSize = context.getCore().imageSize;
                });
                if (OXY_ImagesDir != null)
                {
                    context.getMetrics().setPixelSpacing(pixelSpacing);
                    context.getPhotoAcousticPart().fitUIAccordingToDicomImageSize(imageSize[1], imageSize[0]);
                    OXYDicomFile = openFileDialog.FileName;

                    context.getPhotoAcousticPart().makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Visible);
                    context.getPhotoAcousticPart().fileCount = Directory.GetFiles(OXY_ImagesDir, "*.bmp", SearchOption.AllDirectories).Length;
                    context.getPhotoAcousticPart().BitmapFromPath(OXY_ImagesDir + Path.DirectorySeparatorChar + context.getPhotoAcousticPart().slider_value + ".bmp");
                    //context.getPhotoAcousticPart().OXY_studyname_label.Content = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    OXYStudyname = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    context.getPhotoAcousticPart().frame_num_label.Content = context.getMessages().frame + ": " + context.getPhotoAcousticPart().slider_value;
                    context.getPhotoAcousticPart().doRepeatProcess();
                }
                this.OXY_Checkbox.IsEnabled = true;
                this.OXY_Checkbox.IsChecked = true;
                context.getPhotoAcousticPart().doOXYState();
                context.getPhotoAcousticPart().stopSpinner();
            }
        }

        private async void LoadDeOXYDicom(object sender, RoutedEventArgs e)
        {
            if (context.getUltrasoundPart().ultrasoundDicomFile == null)
            {
                CustomMessageBox.Show(context.getMessages().loadDeOXYWithoutUltrasound, context.getMessages().warning, MessageBoxButton.OK);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = context.getMessages().selectDicom;
            if (openFileDialog.ShowDialog() == true)
            {
                context.getPhotoAcousticPart().startSpinner();
                //clear_canvas();
                context.getCore().repeatSegmentation();
                DeOXYDicomFile = null;

                await Task.Run(() =>
                {
                    string dicomPath = context.getStudyFile().copyFileToWorkspace(context.getStudyFile().getWorkspaceDicomPath(), openFileDialog.FileName, StudyFile.FileType.DeOXYDicomFile);
                    DeOXY_ImagesDir = context.getCore().exportDeOXYImages(dicomPath, true); //enablelogging = true
                    pixelSpacing = context.getCore().pixelSpacing;
                    imageSize = context.getCore().imageSize;
                });
                if (DeOXY_ImagesDir != null)
                {
                    context.getMetrics().setPixelSpacing(pixelSpacing);
                    context.getPhotoAcousticPart().fitUIAccordingToDicomImageSize(imageSize[1], imageSize[0]);
                    DeOXYDicomFile = openFileDialog.FileName;

                    context.getPhotoAcousticPart().makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Visible);
                    context.getPhotoAcousticPart().fileCount = Directory.GetFiles(DeOXY_ImagesDir, "*.bmp", SearchOption.AllDirectories).Length;
                    context.getPhotoAcousticPart().BitmapFromPath(DeOXY_ImagesDir + Path.DirectorySeparatorChar + context.getPhotoAcousticPart().slider_value + ".bmp"); //imagesDir + "/0.bmp"
                    //context.getPhotoAcousticPart().DeOXY_studyname_label.Content = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    DeOXYStudyname = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    context.getPhotoAcousticPart().frame_num_label.Content = context.getMessages().frame + ": " + context.getPhotoAcousticPart().slider_value;
                    
                }
                this.DeOXY_Checkbox.IsEnabled = true;
                this.DeOXY_Checkbox.IsChecked = true;
                context.getPhotoAcousticPart().doDeOXYState();
                context.getPhotoAcousticPart().stopSpinner();
            }
        }

        private async void LoadGNRDicom(object sender, RoutedEventArgs e)
        {
            if (context.getUltrasoundPart().ultrasoundDicomFile == null)
            {
                CustomMessageBox.Show(context.getMessages().loadGNRWithoutUltrasound, context.getMessages().warning, MessageBoxButton.OK);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = context.getMessages().selectDicom;
            if (openFileDialog.ShowDialog() == true)
            {
                context.getPhotoAcousticPart().startSpinner();
                //clear_canvas();
                context.getCore().repeatSegmentation();
                GNRDicomFile = null;

                await Task.Run(() =>
                {
                    string dicomPath = context.getStudyFile().copyFileToWorkspace(context.getStudyFile().getWorkspaceDicomPath(), openFileDialog.FileName, StudyFile.FileType.GNRDicomFile);
                    GNR_ImagesDir = context.getCore().exportGNRImages(dicomPath, true); //enablelogging = true
                    pixelSpacing = context.getCore().pixelSpacing;
                    imageSize = context.getCore().imageSize;
                });
                if (GNR_ImagesDir != null)
                {
                    context.getMetrics().setPixelSpacing(pixelSpacing);
                    context.getPhotoAcousticPart().fitUIAccordingToDicomImageSize(imageSize[1], imageSize[0]);
                    GNRDicomFile = openFileDialog.FileName;

                    context.getPhotoAcousticPart().makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Visible);
                    context.getPhotoAcousticPart().fileCount = Directory.GetFiles(GNR_ImagesDir, "*.bmp", SearchOption.AllDirectories).Length;
                    context.getPhotoAcousticPart().BitmapFromPath(GNR_ImagesDir + Path.DirectorySeparatorChar + context.getPhotoAcousticPart().slider_value + ".bmp"); //imagesDir + "/0.bmp"
                    GNRStudyname = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    context.getPhotoAcousticPart().frame_num_label.Content = context.getMessages().frame + ": " + context.getPhotoAcousticPart().slider_value;
                }
                this.GNR_Checkbox.IsEnabled = true;
                this.GNR_Checkbox.IsChecked = true;
                //context.getPhotoAcousticPart().doDeOXYState();
                context.getPhotoAcousticPart().stopSpinner();
            }
        }

        public void LoadOXYDicomOnStudyloading(string studyName, string filename, string imagesDir, List<double> pixelSpacing, List<double> imageSize)
        {
            this.OXYDicomFile = filename;
            this.OXY_ImagesDir = imagesDir;
            this.pixelSpacing = pixelSpacing;
            this.imageSize = imageSize;
            context.getMetrics().setPixelSpacing(pixelSpacing);
            context.getPhotoAcousticPart().fitUIAccordingToDicomImageSize(imageSize[1], imageSize[0]);
            context.getPhotoAcousticPart().makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Visible);
            context.getPhotoAcousticPart().doOXYState();
            context.getPhotoAcousticPart().fileCount = Directory.GetFiles(OXY_ImagesDir, "*.bmp", SearchOption.AllDirectories).Length;
            context.getPhotoAcousticPart().BitmapFromPath(OXY_ImagesDir + Path.DirectorySeparatorChar + context.getPhotoAcousticPart().slider_value + ".bmp");
            //context.getPhotoAcousticPart().OXY_studyname_label.Content = studyName + " " + context.getMessages().oxy;
            OXYStudyname = studyName + " " + context.getMessages().oxy;
            context.getPhotoAcousticPart().frame_num_label.Content = context.getMessages().frame + ": " + context.getPhotoAcousticPart().slider_value;
            context.getPhotoAcousticPart().doRepeatProcess();
            selectOXY();

        }

        public void LoadDeOXYDicomOnStudyloading(string studyName, string filename, string imagesDir, List<double> pixelSpacing, List<double> imageSize)
        {
            this.DeOXYDicomFile = filename;
            this.DeOXY_ImagesDir = imagesDir;
            this.pixelSpacing = pixelSpacing;
            this.imageSize = imageSize;
            context.getMetrics().setPixelSpacing(pixelSpacing);
            context.getPhotoAcousticPart().fitUIAccordingToDicomImageSize(this.imageSize[1], this.imageSize[0]);
            context.getPhotoAcousticPart().makeVisibeOrUnvisibleSliderLeftTickBar(Visibility.Visible);
            context.getPhotoAcousticPart().doDeOXYState();
            context.getPhotoAcousticPart().fileCount = Directory.GetFiles(DeOXY_ImagesDir, "*.bmp", SearchOption.AllDirectories).Length;
            context.getPhotoAcousticPart().BitmapFromPath(DeOXY_ImagesDir + Path.DirectorySeparatorChar + context.getPhotoAcousticPart().slider_value + ".bmp"); //imagesDir + "/0.bmp"
            //context.getPhotoAcousticPart().DeOXY_studyname_label.Content = studyName + " " + context.getMessages().deoxy;
            DeOXYStudyname = studyName + " " + context.getMessages().deoxy;
            context.getPhotoAcousticPart().frame_num_label.Content = context.getMessages().frame + ": " + context.getPhotoAcousticPart().slider_value;
            selectDeOXY();
        }



        public void selectOXY()
        {
            this.OXY_Checkbox.IsEnabled = true;
            this.OXY_Checkbox.IsChecked = true;
            
        }

        public void selectDeOXY()
        {
            this.DeOXY_Checkbox.IsEnabled = true;
            this.DeOXY_Checkbox.IsChecked = true;
        }

        public void selectGNR()
        {
            this.GNR_Checkbox.IsEnabled = true;
            this.GNR_Checkbox.IsChecked = true;
           
        }

        private void OXY_Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            uncheckRestCheckboxes(OXY_Checkbox);
            context.getPhotoAcousticPart().doOXYState();
        }

        private void DeOXY_Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            uncheckRestCheckboxes(DeOXY_Checkbox);
            context.getPhotoAcousticPart().doDeOXYState();
        }


        private void GNR_Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            uncheckRestCheckboxes(GNR_Checkbox);
            context.getPhotoAcousticPart().doGNRState();
        }

        public void uncheckRestCheckboxes(CheckBox cb = null)
        {
            foreach (CheckBox checkbox in checkboxItems.Children)
            {
                if (cb != checkbox || cb == null)
                {
                    checkbox.IsChecked = false;
                }
            }
        }

        private void OXY_Checkbox_Click(object sender, RoutedEventArgs e)
        {
            if(OXY_Checkbox.IsChecked == false)
            {
                setOXYCheckboxUnchecked();
            }
        }

        private void DeOXY_Checkbox_Click(object sender, RoutedEventArgs e)
        {
            if (DeOXY_Checkbox.IsChecked == false)
            {
                setOXYCheckboxUnchecked();
            }
        }

        private void GNR_Checkbox_Click(object sender, RoutedEventArgs e)
        {
            if (GNR_Checkbox.IsChecked == false)
            {
                setOXYCheckboxUnchecked();
            }
        }

        private void setOXYCheckboxUnchecked()
        {
            OXY_Checkbox.IsChecked = true;
            context.getPhotoAcousticPart().doOXYState();
        }

    }
}
