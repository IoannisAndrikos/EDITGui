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
using static EDITgui.tumorItem;

namespace EDITgui
{
    /// <summary>
    /// Interaction logic for contourItem.xaml
    /// </summary>
    public partial class tumorCheckbox : UserControl
    {
        int index;
        object dropdownContainer;
        Context context;
        tumorItem tumorItem;
        
        public tumorCheckbox()
        {
            InitializeComponent();
        }

        public tumorCheckbox(object dropdownContainer, Context context, int index, bool setChecked = true)
        {
            InitializeComponent();
            this.dropdownContainer = dropdownContainer;
            this.context = context;
            this.tumorItem = context.getImages().getTumor(index);
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
            updateGroupItemsOptions();
        }

        public void updateGroupItemsOptions()
        {
            groupOptionsDropdown.Items.Clear();

            foreach (tumorGroup groupItem in context.getImages().getTumorGroups())
            {
                ComboBoxItem ci = new ComboBoxItem();
                ci.Style = (Style)FindResource("ComboBoxItemStyle1");
                ci.Content = groupItem.groupName;
               // ci.Selected += new RoutedEventHandler(OnSelectedTumor);
                ci.Foreground = Brushes.White;
                groupOptionsDropdown.Items.Add(ci);
       
                if (groupItem.groupName == this.tumorItem.group)
                {
                    groupOptionsDropdown.SelectedValue = ci;
                }
            }
            ComboBoxItem newGroup = new ComboBoxItem();
            newGroup.Style = (Style)FindResource("ComboBoxItemStyle1");
            newGroup.Content = "Manage...";
           // newGroup.Selected += new RoutedEventHandler(OnSelectedTumor);
            newGroup.Foreground = Brushes.White;
            groupOptionsDropdown.Items.Add(newGroup);

            //groupOptionsDropdown.SelectionChanged += new SelectionChangedEventHandler(OnSelectedTumor);
        }


        public void updateAfterCreatingNewGroup()
        {
            //update the currect tumorCheckbox
            //updateGroupItemsOptions();
            if (dropdownContainer is photoAcousticDataParser)
            {
                ((photoAcousticDataParser)dropdownContainer).updateAfterCreatingTumorGroup();
            }
            else if ((dropdownContainer is ultrasoundDataParser))
            {
                ((ultrasoundDataParser)dropdownContainer).updateAfterCreatingTumorGroup();
            }
        }


        private void GroupOptionsDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (groupOptionsDropdown.IsDropDownOpen)
            {
                ComboBoxItem selectedItem = groupOptionsDropdown.SelectedItem as ComboBoxItem;

                if (selectedItem.Content.ToString() == "Manage...")
                {
                    context.getTumorGroups().CreateTumorGroupsWindow(this);
                }
                else
                {
                    tumorItem.group = selectedItem.Content.ToString();

                    if (dropdownContainer is photoAcousticDataParser)
                    {
                        ((photoAcousticDataParser)dropdownContainer).updateUltrasoundSideAfterSelectingTumorGroup(index, tumorItem.group);
                    }
                    else if ((dropdownContainer is ultrasoundDataParser))
                    {
                        ((ultrasoundDataParser)dropdownContainer).updatePhotoacousticSideAfterSelectingTumorGroup(index, tumorItem.group);
                    }
                }
                // groupOptionsDropdown.IsDropDownOpen = false; //better performance
            }
        }

        public void OnSelectedTumor(object sender, RoutedEventArgs e)
        {
            
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
            if (dropdownContainer is photoAcousticDataParser)
            {
                 ((photoAcousticDataParser)dropdownContainer).removeItem(this);
            }
            else if((dropdownContainer is ultrasoundDataParser))
            {
               ((ultrasoundDataParser)dropdownContainer).removeItem(this);
            }
        }

        private StackPanel getItemPanel()
        {
            if(dropdownContainer is photoAcousticDataParser)
            {
                return ((photoAcousticDataParser)dropdownContainer).checkboxItems;
            }else if((dropdownContainer is ultrasoundDataParser))
            {
                return ((ultrasoundDataParser)dropdownContainer).checkboxItems;
            }

            return null;
        }

        private void Check_box_Click(object sender, RoutedEventArgs e)
        {
            if (this.check_box.IsChecked == false)
            {
                if (dropdownContainer is photoAcousticDataParser)
                {
                    ((photoAcousticDataParser)dropdownContainer).setThicknessAsSelected();
                    ((photoAcousticDataParser)dropdownContainer).updateCanvas();
                }
                else if ((dropdownContainer is ultrasoundDataParser))
                {
                    ((ultrasoundDataParser)dropdownContainer).setBladderAsSelected();
                    ((ultrasoundDataParser)dropdownContainer).updateCanvas();
                }
            }
        }

        private void Check_box_Checked(object sender, RoutedEventArgs e)
        {
            if (dropdownContainer is photoAcousticDataParser)
            {
                ((photoAcousticDataParser)dropdownContainer).setSelectedTumor(this.index);
            }
            else if ((dropdownContainer is ultrasoundDataParser))
            {
                ((ultrasoundDataParser)dropdownContainer).setSelectedTumor(this.index);
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
