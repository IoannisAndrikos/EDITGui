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
using Path = System.IO.Path;

namespace EDITgui
{
    /// <summary>
    /// Interaction logic for Visualize2DUsing3DSlicer.xaml
    /// </summary>
    public partial class Slicer3D : UserControl
    {

        Context context;


        int startingFrame;
        int endingFrame;
        int frameIndex;

        string ultrasoundImagesDir = null;
        string OXYImagesDir = null;
        string DeOXYmagesDir = null;


   

        public Slicer3D()
        {
            InitializeComponent();
        }

        public Slicer3D(Context context)
        {
            InitializeComponent();
            this.context = context;
            InitializeView();
        }

        public void InitializeView()
        {
            this.imagesPanel.Visibility = Visibility.Collapsed;
            this.frame_num_label.Visibility = Visibility.Collapsed;
        }

        public void setUltrasoundImagesDir(string dir)
        {
            this.ultrasoundImagesDir = dir;
        }

        public void setOXYImagesDir(string dir)
        {
            this.OXYImagesDir = dir;
        }

        public void setDeOXYImagesDir(string dir)
        {
            this.DeOXYmagesDir = dir;
        }

        private void updateFrameIndex(int index)
        {
            this.frameIndex = index + startingFrame;
        }

     
        public void updateImages(int index)
        {
            updateFrameIndex(index);

            if (ultrasoundImagesDir != null)
            {
                ultrasoundGrid.Visibility = Visibility.Visible;
                ultrasoundImage.Source = new BitmapImage();
                ((BitmapImage)ultrasoundImage.Source).BeginInit();
                ((BitmapImage)ultrasoundImage.Source).CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                ((BitmapImage)ultrasoundImage.Source).CacheOption = BitmapCacheOption.OnLoad;
                // ((BitmapImage)ultrasoundImage.Source).UriSource = new Uri("C:/Users/Legion Y540/Desktop/images for AI/20.bmp");
                ((BitmapImage)ultrasoundImage.Source).UriSource = new Uri(ultrasoundImagesDir +Path.DirectorySeparatorChar + (this.frameIndex).ToString() + ".bmp");
                ((BitmapImage)ultrasoundImage.Source).EndInit();
            }
            else
            {
                ultrasoundImage.Source = null;
                ultrasoundGrid.Visibility = Visibility.Collapsed;
            }


            if (OXYImagesDir != null)
            {
                OXYGrid.Visibility = Visibility.Visible;
                OXYImage.Source = new BitmapImage();
                ((BitmapImage)OXYImage.Source).BeginInit();
                ((BitmapImage)OXYImage.Source).CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                ((BitmapImage)OXYImage.Source).CacheOption = BitmapCacheOption.OnLoad;
                ((BitmapImage)OXYImage.Source).UriSource = new Uri(OXYImagesDir + Path.DirectorySeparatorChar + (this.frameIndex).ToString() + ".bmp");
                ((BitmapImage)OXYImage.Source).EndInit();
            }
            else
            {
                OXYImage.Source = null;
                OXYGrid.Visibility = Visibility.Collapsed;
            }

            if (DeOXYmagesDir != null)
            {
                DeOXYGrid.Visibility = Visibility.Visible;
                DeOXYImage.Source = new BitmapImage();
                ((BitmapImage)DeOXYImage.Source).BeginInit();
                ((BitmapImage)DeOXYImage.Source).CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                ((BitmapImage)DeOXYImage.Source).CacheOption = BitmapCacheOption.OnLoad;
                ((BitmapImage)DeOXYImage.Source).UriSource = new Uri(DeOXYmagesDir + Path.DirectorySeparatorChar + (this.frameIndex).ToString() + ".bmp");
                ((BitmapImage)DeOXYImage.Source).EndInit();
            }
            else
            {
                DeOXYImage.Source = null;
                DeOXYGrid.Visibility = Visibility.Collapsed;
            }

            updateFrameLabel();
        }

        private void updateFrameLabel()
        {
            frame_num_label.Content = "Frame: " + this.frameIndex.ToString();
        }


        private void Overlay_ultrasound_button_Click(object sender, RoutedEventArgs e)
        {
            context.getMainWindow().overlayImage(this.ultrasoundImagesDir + Path.DirectorySeparatorChar + this.frameIndex.ToString() + ".bmp");
        }

        private void Overlay_OXY_button_Click(object sender, RoutedEventArgs e)
        {
            context.getMainWindow().overlayImage(this.OXYImagesDir + Path.DirectorySeparatorChar + this.frameIndex.ToString() + ".bmp");
        }

        private void Overlay_DeOXY_button_Click(object sender, RoutedEventArgs e)
        {
            context.getMainWindow().overlayImage(this.DeOXYmagesDir + Path.DirectorySeparatorChar + this.frameIndex.ToString() + ".bmp");
        }

        private void Slicer_Click(object sender, RoutedEventArgs e)
        {
            string message = context.getCheck().getMessage(checkBeforeExecute.executionType.slicer);
            if (message != null)
            {
                CustomMessageBox.Show(message, context.getMessages().warning, MessageBoxButton.OK);
                return;
            }

            context.getMainWindow().applySlicer();
            Console.WriteLine(context.getUltrasoundPart().startingFrame.ToString());
            Console.WriteLine(context.getUltrasoundPart().endingFrame.ToString());

            this.startingFrame = context.getUltrasoundPart().startingFrame;
            this.endingFrame = context.getUltrasoundPart().endingFrame;

            setUltrasoundImagesDir(context.getUltrasoundPart().imagesDir);
            setOXYImagesDir(context.getPhotoAcousticPart().OXYimagesDir);
            setDeOXYImagesDir(context.getPhotoAcousticPart().deOXYimagesDir);
            updateImages(0);

            this.imagesPanel.Visibility = Visibility.Visible;
            this.frame_num_label.Visibility = Visibility.Visible;
        }
    }
}
