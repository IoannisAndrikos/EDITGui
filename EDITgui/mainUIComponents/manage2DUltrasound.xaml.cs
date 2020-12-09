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
    /// Interaction logic for obectsDropdown.xaml
    /// </summary>
    public partial class manage2DUltrasound : UserControl
    {
        public delegate void addTumorItemHandler(tumorItem obj);
        public static event addTumorItemHandler tumorWasAdded = delegate { };

        public delegate void removeTumorItemHandler(tumorCheckbox obj);
        public static event removeTumorItemHandler tumorWasRemoved = delegate { };

        Context context;
        int selectedItem = -1; 

        static double tumorCheckbox_topMargin = 6.5;
        static int maxItemNumber = 14; //taking into account the UI size
        int leftMarginHasItems = 25;
        int leftMarginHasNoItems = 4;

        public List<List<tumorItem>> tumors = new List<List<tumorItem>>();
        public List<double> objectsPanelHeight = new List<double>();

        Thickness itemMargin = new Thickness(15, tumorCheckbox_topMargin, 0,0);

        Thickness hasItems = new Thickness(29, 156, 0, 0);

        public manage2DUltrasound()
        {
            InitializeComponent();
        }

        public manage2DUltrasound(Context context)
        {
            InitializeComponent();
            this.Visibility = Visibility.Collapsed;
            this.context = context;
            manage2DPhotoAcoustic.tumorWasAdded += onAddTumorItem;
            manage2DPhotoAcoustic.tumorWasRemoved += onRemoveTumorItem;
        }


        List<tumorItem> emptyList = new List<tumorItem>();
        public void initializeTumors()
        {
            tumors.Clear();
            objectsPanelHeight.Clear();
            checkboxItems.Children.Clear();
            for (int i=0; i<context.getUltrasoundPart().fileCount; i++)
            {
                objectsPanelHeight.Add(34);
                tumors.Add(emptyList.ToList());
            }
            objectsPanel.Height = 34;
            doHasNoItems();
            UltrasoundPart.sliderValueChanged += OnUltrasoundSliderValueChanged;
        }


        private void OpenButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (objectsPanel.Visibility == Visibility.Collapsed)
            {
                objectsPanel.Visibility = Visibility.Visible;
                openButton.Content = "\u2B9F";
            }
            else if (objectsPanel.Visibility == Visibility.Visible)
            {
                objectsPanel.Visibility = Visibility.Collapsed;
                openButton.Content = "\u2B9D";
            }
        }

        //--------------------EXTERNAL HANDLERS----------------------------
        private void OnUltrasoundSliderValueChanged(int obj)
        {
            objectsPanel.Height = objectsPanelHeight[(int)obj];
            checkboxItems.Children.Clear();
            if (!tumors[(int)obj].Any())
            {
                doHasNoItems();
            }
            else
            {
                foreach (tumorItem tm in tumors[(int)obj])
                {
                    checkboxItems.Children.Add(tm.checkbox);
                }
                doHasItems();
            }
        }

        private void onAddTumorItem(tumorItem item)
        {
            if (tumors[context.getUltrasoundPart().slider_value].Count <= maxItemNumber)
            {
                if (tumors[context.getUltrasoundPart().slider_value].Count >= 1)
                {
                    objectsPanelHeight[context.getUltrasoundPart().slider_value] += tumors[context.getUltrasoundPart().slider_value][0].checkbox.Height + tumorCheckbox_topMargin;
                    objectsPanel.Height = objectsPanelHeight[context.getUltrasoundPart().slider_value];
                }
                tumorCheckbox cb = new tumorCheckbox(this, false);
                cb.setIndex(item.checkbox.getIndex());//-----------------?
                cb.Margin = itemMargin;
                tumorItem tumor = new tumorItem() { index = tumors.Count, points = new List<Point>(), checkbox = cb };
                tumors[context.getUltrasoundPart().slider_value].Add(tumor);
                checkboxItems.Children.Add(tumor.checkbox);
            }
            doHasItems();
        }

        private void onRemoveTumorItem(tumorCheckbox item)
        {
            decreaseDropdownHeight();
            checkboxItems.Children.Remove(item);
            getCurrentFrameTumors().RemoveAt(item.getIndex());
            //Be careful here!
            int i = 0;
            foreach (tumorItem tm in getCurrentFrameTumors())
            {
                tm.index = i;
                tm.checkbox.setIndex(i++);
            }

            if (!getCurrentFrameTumors().Any())
            {
                doHasNoItems();
            }
        }
        //-----------------------------------------------------------


        private void AddButton_Click(object sender, RoutedEventArgs e)
        { 
            doHasItems();
            addItem();
        }

        public void addItem()
        {
            if (tumors[context.getUltrasoundPart().slider_value].Count <= maxItemNumber)
            {
                if (tumors[context.getUltrasoundPart().slider_value].Count >= 1)
                {
                    objectsPanelHeight[context.getUltrasoundPart().slider_value] += tumors[context.getUltrasoundPart().slider_value][0].checkbox.Height + tumorCheckbox_topMargin;
                    objectsPanel.Height = objectsPanelHeight[context.getUltrasoundPart().slider_value];
                }
                tumorCheckbox cb = new tumorCheckbox(this, true);
                cb.setIndex(tumors[context.getUltrasoundPart().slider_value].Count);//-----------------?
                cb.Margin = itemMargin;
                tumorItem tumor = new tumorItem() { index = tumors.Count, points = new List<Point>(), checkbox = cb };
                tumors[context.getUltrasoundPart().slider_value].Add(tumor);
                checkboxItems.Children.Add(tumor.checkbox);
                tumorWasAdded(tumor);
            }
        }

        public void removeItem(tumorCheckbox item)
        {
            decreaseDropdownHeight();
            checkboxItems.Children.Remove(item);
            getCurrentFrameTumors().RemoveAt(item.getIndex());
            //Be careful here!
            int i = 0;
            foreach (tumorItem tm in getCurrentFrameTumors())
            {
                tm.index = i;
                tm.checkbox.setIndex(i++);
            }

            if (!getCurrentFrameTumors().Any())
            {
                doHasNoItems();
            }
            tumorWasRemoved(item);
        }

        public void decreaseDropdownHeight()
        {
            if (tumors[context.getUltrasoundPart().slider_value].Count > 1)
            {
                 objectsPanelHeight[context.getUltrasoundPart().slider_value] -= (tumors[context.getUltrasoundPart().slider_value][0].checkbox.Height + tumorCheckbox_topMargin);
                 objectsPanel.Height = objectsPanelHeight[context.getUltrasoundPart().slider_value];
            }
        }
       
        public void doHasItems()
        {
            Thickness addButtonMargin = addButton.Margin;
            addButtonMargin.Left = leftMarginHasItems;
            addButton.Margin = addButtonMargin;
        }

        public void doHasNoItems()
        {
            Thickness addButtonMargin = addButton.Margin;
            addButtonMargin.Left = leftMarginHasNoItems;
            addButton.Margin = addButtonMargin;
            objectsPanel.Visibility = Visibility.Collapsed;
            openButton.Content = "\u2B9D";
        }

        public List<tumorItem> getCurrentFrameTumors()
        {
            return tumors[context.getUltrasoundPart().slider_value];
        }

        private int getSelectedItemIndex()
        {
            foreach (tumorCheckbox tcb in checkboxItems.Children)
            {
                if (tcb.check_box.IsChecked == true)
                {
                    return tcb.getIndex();
                    break;
                }
            }
            return -1;
        }


        public List<Point> getSelectedObject2D()
        {
            selectedItem = getSelectedItemIndex();
            if (selectedItem == -1)
            {
                return context.getPoints2D().getObjectPoints2D(Points2D.oject2DType.bladder, selectedItem);
            }
            else
            {
                return context.getPoints2D().getObjectPoints2D(Points2D.oject2DType.tumor, selectedItem);
            }
        }


    }
}
