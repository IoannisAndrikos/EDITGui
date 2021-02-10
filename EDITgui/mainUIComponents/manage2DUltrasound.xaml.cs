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
        static double tumorCheckbox_topMargin = 4;
        Thickness itemMargin = new Thickness(5, tumorCheckbox_topMargin, 0, 0);

        public delegate void addTumorItemHandler(int index);
        public static event addTumorItemHandler tumorWasAdded = delegate { };

        public delegate void removeTumorItemHandler(int index);
        public static event removeTumorItemHandler tumorWasRemoved = delegate { };

        Context context;

        int slider_value = 0;

        public int selectedItem = -2;

        double checkBoxMinHeight = 19.97;
        static int maxItemNumber = 10; //taking into account the UI size
        int leftMarginHasItems = 25;
        int leftMarginHasNoItems = 4;

        private List<double> objectsPanelHeight = new List<double>();

        public manage2DUltrasound()
        {
            InitializeComponent();
        }

        public manage2DUltrasound(Context context)
        {
            InitializeComponent();
            this.Visibility = Visibility.Collapsed;
            this.context = context;
            this.objectsPanel.Visibility = Visibility.Collapsed;
            manage2DPhotoAcoustic.tumorWasAdded += onAddTumorCheckbox;
            manage2DPhotoAcoustic.tumorWasRemoved += onRemoveTumorCheckbox;
        }

        public void initializeTumors()
        {
            setBladderAsSelected();
            objectsPanelHeight.Clear();
            checkboxItems.Children.Clear();
            for (int i = 0; i < context.getUltrasoundPart().fileCount; i++)
            {
                objectsPanelHeight.Add(120);
            }
            doHasNoItems();
            objectsPanel.Height = 120;
        }



        private void Checkbox_OuterWall_Click(object sender, RoutedEventArgs e)
        {
            if(this.checkbox_OuterWall.IsChecked == true)
            {
                this.checkbox_Bladder.IsChecked = false;
                foreach (tumorCheckbox tc in checkboxItems.Children)
                {
                    tc.check_box.IsChecked = false;
                }
                this.checkbox_OuterWall.IsChecked = true;
                this.selectedItem = -1;
                updateCanvas();
            }
            else
            {
                this.checkbox_OuterWall.IsChecked = false;
                setBladderAsSelected();
                updateCanvas();
            }
        }

        private void Checkbox_Bladder_Click(object sender, RoutedEventArgs e)
        {
            this.checkbox_OuterWall.IsChecked = false;
            foreach (tumorCheckbox tc in checkboxItems.Children)
            {
                tc.check_box.IsChecked = false;
            }
            setBladderAsSelected();
            updateCanvas();
        }


        public void setSelectedTumor(int index)
        {
            this.checkbox_OuterWall.IsChecked = false;
            this.checkbox_Bladder.IsChecked = false;
            this.selectedItem = index;
            updateCanvas();
        }


        public void setBladderAsSelected()
        {
            selectedItem = -2;
            this.checkbox_Bladder.IsChecked = true;
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
        public void updatePanel (int value)
        {
            slider_value = value;

            setBladderAsSelected();
            objectsPanel.Height = objectsPanelHeight[value];
            checkboxItems.Children.Clear();
            if (context.getImages().getTumorItemsCount() == 0)
            {
                doHasNoItems();
            }
            else
            {
                objectsPanelHeight[slider_value] = 120;
                objectsPanel.Height = 120;
                for (int i = 0; i < context.getImages().getTumorItemsCount(); i++)
                {
                    increaseDropdownHeight();
                    tumorCheckbox checkbox = new tumorCheckbox(this, i, false);
                    checkbox.FontSize = 11;
                    checkbox.Margin = itemMargin;
                    checkboxItems.Children.Add(checkbox);
                }
                doHasItems();
            }
        }

        private void onAddTumorCheckbox(int index)
        {
            if (checkboxItems.Children.Count <= maxItemNumber)
            {
                increaseDropdownHeight();
                tumorCheckbox checkbox = new tumorCheckbox(this, index, false);
                checkbox.FontSize = 11;
                checkbox.Margin = itemMargin;
                checkboxItems.Children.Add(checkbox);
            }
            doHasItems();
        }

        private void onRemoveTumorCheckbox(int index)
        {
            decreaseDropdownHeight();
            checkboxItems.Children.RemoveAt(index);
            bool theRemovedCheckboxIsChecked = true; //to mange the selected item
            int i = 0;
            foreach (tumorCheckbox ch in checkboxItems.Children)
            {
                ch.setIndex(i++);
                if (ch.check_box.IsChecked == true) theRemovedCheckboxIsChecked = false;
            }
            if (theRemovedCheckboxIsChecked) setBladderAsSelected(); //if it was selected set as selected the bladder

            if (checkboxItems.Children.Count == 0) doHasNoItems();
            updateCanvas();
        }
        //-----------------------------------------------------------

        public void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (context.getUltrasoundPart().areTherePoints())
            {
                doHasItems();
                addItem();
            }
            else
            {
                CustomMessageBox.Show(context.getMessages().addTumorWithoutBladderAnnotation, context.getMessages().warning, MessageBoxButton.OK);
            }
        }

        public void addItem()
        {
            if (checkboxItems.Children.Count <= maxItemNumber)
            {
                increaseDropdownHeight();
                int index = context.getImages().addTumor();
                tumorCheckbox checkbox = new tumorCheckbox(this, index);
                checkbox.FontSize = 11;
                checkbox.Margin = itemMargin;
                checkboxItems.Children.Add(checkbox);
                tumorWasAdded(checkbox.getIndex());
            }
        }


        public void removeItem(tumorCheckbox checkbox)
        {
            if (checkbox.check_box.IsChecked == true) setBladderAsSelected();
            decreaseDropdownHeight();
            context.getImages().removeTumor(checkbox.getIndex());
            checkboxItems.Children.Remove(checkbox);
            int i = 0;
            foreach (tumorCheckbox ch in checkboxItems.Children)
            {
                ch.setIndex(i++);
            }
            if (checkboxItems.Children.Count == 0) doHasNoItems();
            tumorWasRemoved(checkbox.getIndex());
            updateCanvas();
        }

        private void increaseDropdownHeight()
        {
            if (checkboxItems.Children.Count >= 1)
            {
                objectsPanelHeight[slider_value] += checkBoxMinHeight + tumorCheckbox_topMargin;
                objectsPanel.Height = objectsPanelHeight[slider_value];
            }
        }

        public void decreaseDropdownHeight()
        {
            if (objectsPanel.Height > 130)
            {
                objectsPanelHeight[slider_value] -= (checkBoxMinHeight + tumorCheckbox_topMargin);
                objectsPanel.Height = objectsPanelHeight[slider_value];
            }
        }

        public void doHasItems()
        {
            Thickness addButtonMargin = addButton.Margin;
            addButtonMargin.Left = leftMarginHasItems;
            addButton.Margin = addButtonMargin;
            openButton.Visibility = Visibility.Visible;
        }

        public void doHasNoItems()
        {
           // Thickness addButtonMargin = addButton.Margin;
            //addButtonMargin.Left = leftMarginHasNoItems;
           // addButton.Margin = addButtonMargin;
            //objectsPanel.Visibility = Visibility.Collapsed;
            //openButton.Visibility = Visibility.Collapsed;
            openButton.Content = "\u2B9D";
        }

        public void updateCanvas()
        {
            if(context.getUltrasoundPart().contourSeg == UltrasoundPart.ContourSegmentation.CORRECTION)
            {
                context.getUltrasoundPart().doCorrection();
            }
           
            context.getUltrasoundPart().updateCanvas();
        }


        //-----------------GETTERS---------------------
        //---1--GET POINTS AND POLYLINES
        Selected2DObject selectedObject = new Selected2DObject() { points = null, metrics = null, polylineColor = null };
        public Selected2DObject getSelectedObject2D()
        {
            try
            {
                if(selectedItem == -2)
                {
                    selectedObject.points = context.getImages().getBladderPoints();
                    selectedObject.polylineColor = ViewAspects.cyan;
                    selectedObject.metrics = getBladderMetricsString();
                }else if(selectedItem == -1)
                {
                    selectedObject.points = context.getImages().getThicknessPoints();
                    selectedObject.polylineColor = ViewAspects.cyan;
                    selectedObject.metrics = context.getPhotoAcousticPoints2D().getThicknessMetricsString();
                }
                else
                {
                    selectedObject.points = context.getImages().getTumorPoints(selectedItem);
                    selectedObject.polylineColor = ViewAspects.magenta;
                    selectedObject.metrics = getTumorMetricsString();
                }


                //if (selectedItem >= 0)
                //{
                //    selectedObject.points = context.getImages().getTumorPoints(selectedItem);
                //    selectedObject.polylineColor = ViewAspects.magenta;
                //    selectedObject.metrics = getTumorMetricsString();
                //}
                //else
                //{
                //    selectedObject.points = context.getImages().getBladderPoints();
                //    selectedObject.polylineColor = ViewAspects.cyan;
                //    selectedObject.metrics = getBladderMetricsString();
                //}

                return selectedObject;
            }
            catch (Exception ex)
            {
                setBladderAsSelected();
                selectedObject.points = context.getImages().getBladderPoints();
                selectedObject.polylineColor = ViewAspects.cyan;
                selectedObject.metrics = getBladderMetricsString();
                return selectedObject;
            }
        }

        List<UIElement> polylines = new List<UIElement>();
        List<Point> tempPoints = new List<Point>();
        public List<UIElement> getNoSelectedObject2DPolylines()
        {
            polylines.Clear();
            if (selectedItem != -2)
            {
                tempPoints = context.getImages().getBladderPoints().ToList();
                if (tempPoints.Any())
                {
                    tempPoints.Add(tempPoints[0]);
                    for (int i = 0; i < tempPoints.Count - 1; i++)
                    {
                        Polyline pl = new Polyline();
                        pl.FillRule = FillRule.EvenOdd;
                        pl.StrokeThickness = 0.5;
                        pl.Points.Add(tempPoints.ElementAt(i));
                        pl.Points.Add(tempPoints.ElementAt(i + 1));
                        pl.Stroke = ViewAspects.cyan;
                        pl.StrokeStartLineCap = PenLineCap.Round;
                        pl.StrokeEndLineCap = PenLineCap.Round;
                        polylines.Add(pl);
                    }
                }
                tempPoints.Clear();
            }
            if (selectedItem != -1)
            {
                tempPoints = context.getImages().getThicknessPoints().ToList();
                if (tempPoints.Any())
                {
                    tempPoints.Add(tempPoints[0]);
                    for (int i = 0; i < tempPoints.Count - 1; i++)
                    {
                        Polyline pl = new Polyline();
                        pl.FillRule = FillRule.EvenOdd;
                        pl.StrokeThickness = 0.5;
                        pl.Points.Add(tempPoints.ElementAt(i));
                        pl.Points.Add(tempPoints.ElementAt(i + 1));
                        pl.Stroke = ViewAspects.cyan;
                        pl.StrokeStartLineCap = PenLineCap.Round;
                        pl.StrokeEndLineCap = PenLineCap.Round;
                        polylines.Add(pl);
                    }
                }
                tempPoints.Clear();
            }
            for (int i = 0; i < context.getImages().getTumorItemsCount(); i++)
            {
                if (i != selectedItem)
                {
                    tempPoints = context.getImages().getTumorPoints(i).ToList();
                    if (tempPoints.Any())
                    {
                        tempPoints.Add(tempPoints[0]);
                        for (int j = 0; j < tempPoints.Count - 1; j++)
                        {
                            Polyline pl = new Polyline();
                            pl.FillRule = FillRule.EvenOdd;
                            pl.StrokeThickness = 0.5;
                            pl.Points.Add(tempPoints.ElementAt(j));
                            pl.Points.Add(tempPoints.ElementAt(j + 1));
                            pl.Stroke = ViewAspects.magenta;
                            pl.StrokeStartLineCap = PenLineCap.Round;
                            pl.StrokeEndLineCap = PenLineCap.Round;
                            polylines.Add(pl);
                        }
                        tempPoints.Clear();
                    }
                }
            }
            return polylines;
        }

        FrameMetrics metrics;
        public string getBladderMetricsString()
        {
            metrics = context.getImages().getBladderMetrics();
            if (metrics.area == 0) return "";

            return context.getMessages().perimeter + " = " + Math.Round(metrics.perimeter, 2) + " " + context.getMessages().mm + Environment.NewLine +
                                                         context.getMessages().area + " = " + Math.Round(metrics.area, 2) + " " + context.getMessages().mmB2;
        }

        private string getTumorMetricsString()
        {
            metrics = context.getImages().getTumorMetrics(selectedItem);
            if (metrics.area == 0) return "";
            return context.getMessages().perimeter + " = " + Math.Round(metrics.perimeter, 2) + " " + context.getMessages().mm + Environment.NewLine +
                                                         context.getMessages().area + " = " + Math.Round(metrics.area, 2) + " " + context.getMessages().mmB2;
        }



        public void updateSelectedObjectMetrics()
        {
            if (selectedItem == -2)
            {
                context.getImages().recalculateBladderMetrics();
            }
            else if(selectedItem == -1)
            {
                context.getImages().recalculateThicknessMetrics();
            }
            else
            {
                context.getImages().recalculateTumorMetrics(selectedItem);
            }
        }

        
    }
}
