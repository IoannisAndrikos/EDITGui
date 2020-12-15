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
    /// Interaction logic for contourItem.xaml
    /// </summary>
    public partial class tumorCheckbox : UserControl
    {
        int index;
        object dropdownContainer;

        public tumorCheckbox()
        {
            InitializeComponent();
        }

        public tumorCheckbox(object dropdownContainer, int index, bool setChecked = true)
        {
            InitializeComponent(); 
            this.dropdownContainer = dropdownContainer;
            this.index = index;
            check_box.Content = index.ToString();
            if (setChecked)
            {
                foreach (tumorCheckbox tm in getItemPanel().Children)
                {
                    if (tm.index != this.index)
                    {
                        tm.check_box.IsChecked = false;
                    }
                }
                this.check_box.IsChecked = true;
            }
        }

        public void setIndex(int index)
        {
            check_box.Content = index.ToString();
            this.index = index;
        }

        public int getIndex()
        {
            return this.index;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (dropdownContainer is manage2DPhotoAcoustic)
            {
                 ((manage2DPhotoAcoustic)dropdownContainer).removeItem(this);
                 ((manage2DPhotoAcoustic)dropdownContainer).updateCanvas();
            }
            else if((dropdownContainer is manage2DUltrasound))
            {
               ((manage2DUltrasound)dropdownContainer).removeItem(this);
                ((manage2DUltrasound)dropdownContainer).updateCanvas();
            }
        }

        private StackPanel getItemPanel()
        {
            if(dropdownContainer is manage2DPhotoAcoustic)
            {
                return ((manage2DPhotoAcoustic)dropdownContainer).checkboxItems;
            }else if((dropdownContainer is manage2DUltrasound))
            {
                return ((manage2DUltrasound)dropdownContainer).checkboxItems;
            }

            return null;
        }

        private void Check_box_Click(object sender, RoutedEventArgs e)
        {
            if (this.check_box.IsChecked == false)
            {
                if (dropdownContainer is manage2DPhotoAcoustic)
                {
                    ((manage2DPhotoAcoustic)dropdownContainer).setThicknessAsSelected();
                    ((manage2DPhotoAcoustic)dropdownContainer).updateCanvas();
                }
                else if ((dropdownContainer is manage2DUltrasound))
                {
                    ((manage2DUltrasound)dropdownContainer).setBladderAsSelected();
                    ((manage2DUltrasound)dropdownContainer).updateCanvas();
                }
            }
        }

        private void Check_box_Checked(object sender, RoutedEventArgs e)
        {
            if (dropdownContainer is manage2DPhotoAcoustic)
            {
                ((manage2DPhotoAcoustic)dropdownContainer).selectedItem = this.index;
                ((manage2DPhotoAcoustic)dropdownContainer).updateCanvas();
            }
            else if ((dropdownContainer is manage2DUltrasound))
            {
                ((manage2DUltrasound)dropdownContainer).selectedItem = this.index;
                ((manage2DUltrasound)dropdownContainer).updateCanvas();
            }

            foreach (tumorCheckbox tm in getItemPanel().Children)
            {
                if (tm.index != this.index)
                {
                    tm.check_box.IsChecked = false;
                }
            }
        }
    }
}
