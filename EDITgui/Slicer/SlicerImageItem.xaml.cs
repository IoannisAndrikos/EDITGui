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
    /// Interaction logic for SlicerImageItem.xaml
    /// </summary>
    public partial class SlicerImageItem : UserControl
    {
        Slicer3D slicer;

        public bool isSelected = false;

        public string imagesPath;
        Border imageBorder;
        Image image;
       

        public SlicerImageItem(Slicer3D slicer, string imagesPath, double width, double height)
        {
            InitializeComponent();

            this.Height =  height;
            this.Width = width;
            this.slicer = slicer;
            this.imagesPath = imagesPath;

            imageBorder = new Border();
            imageBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            imageBorder.VerticalAlignment = VerticalAlignment.Stretch;
            imageBorder.Margin = new Thickness(0, 0, 0, 0);
            imageBorder.BorderThickness = new Thickness(3, 3, 3, 3);
            imageBorder.BorderBrush = null;

            this.itemGrid.Children.Add(imageBorder);

            this.itemGrid.MouseLeftButtonDown += ItemGrid_MouseLeftButtonDown;

            image = new Image();
            image.Stretch = Stretch.Fill;
            image.HorizontalAlignment = HorizontalAlignment.Stretch;
            image.VerticalAlignment = VerticalAlignment.Stretch;
            image.Margin = new Thickness(0, 0, 0, 0);
            imageBorder.Child = image;
        }

        private void ItemGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            slicer.itemWasPressed(this);
        }

        public void setImage(int frameIndex)
        {
            image.Source = new BitmapImage();
            ((BitmapImage)image.Source).BeginInit();
            ((BitmapImage)image.Source).CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            ((BitmapImage)image.Source).CacheOption = BitmapCacheOption.OnLoad;
            ((BitmapImage)image.Source).UriSource = new Uri(imagesPath + Path.DirectorySeparatorChar + frameIndex.ToString() + ".bmp");
            ((BitmapImage)image.Source).EndInit();
        }


        public void setSelected()
        { 
          imageBorder.BorderBrush = ViewAspects.selectSlicerImageGreen;
          this.isSelected = true;
        }


        public void setNonSelected()
        {
            imageBorder.BorderBrush = null;
            this.isSelected = false;
        }

    }
}
