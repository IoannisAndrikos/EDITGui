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

        //public tumorCheckbox(tumorObjectsUltrasound dropdownContainer)
        //{
        //    InitializeComponent();
        //    this.tumorsDropdownUltrasound = dropdownContainer;
        //    setJustCreatedCheckBoxToChecked();
        //}

        public tumorCheckbox(object dropdownContainer, bool setJustcreatedCheckbocChecked)
        {
            InitializeComponent(); 
            this.dropdownContainer = dropdownContainer;
            if (setJustcreatedCheckbocChecked)
            {
                foreach (tumorItem tm in getCurrentFrameTumors())
                {
                    tm.checkbox.check_box.IsChecked = false;
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
            }
            else //if((dropdownContainer is tumorObjectsUltrasound))
            {
               ((manage2DUltrasound)dropdownContainer).removeItem(this);
            }
           // tumorsDropdownUltrasound.removeItem(this);
        }



        private List<tumorItem> getCurrentFrameTumors()
        {
            if(dropdownContainer is manage2DPhotoAcoustic)
            {
                return ((manage2DPhotoAcoustic)dropdownContainer).getCurrentFrameTumors();
            }else //if((dropdownContainer is tumorObjectsUltrasound))
            {
                return ((manage2DUltrasound)dropdownContainer).getCurrentFrameTumors();
            }
        }

        private void Check_box_Click(object sender, RoutedEventArgs e)
        {
            if(this.check_box.IsChecked == true)
            {
                foreach (tumorItem tm in getCurrentFrameTumors())
                {
                    if (tm.index != this.index)
                    {
                        tm.checkbox.check_box.IsChecked = false;
                    }
                }
                this.check_box.IsChecked = true;
            }
            else
            {

                this.check_box.IsChecked = false;
            }
        }
    }

    public class tumorItem
    {
        public int index { get; set; }
        public List<Point> points { get; set; }
        public tumorCheckbox checkbox { get; set; }
    }
}
