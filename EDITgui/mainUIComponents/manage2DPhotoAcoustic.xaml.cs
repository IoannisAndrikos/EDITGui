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
    public partial class manage2DPhotoAcoustic : UserControl
    {
        static double tumorCheckbox_topMargin = 6.5;
        Thickness itemMargin = new Thickness(15, tumorCheckbox_topMargin, 0, 0);

        public delegate void addTumorItemHandler(int index);
        public static event addTumorItemHandler tumorWasAdded = delegate { };

        public delegate void removeTumorItemHandler(int index);
        public static event removeTumorItemHandler tumorWasRemoved = delegate { };

        Context context;

        int slider_value = 0;

        public int selectedItem = -1;

        double checkBoxMinHeight = 19.97;
        static int maxItemNumber = 14; //taking into account the UI size
        int leftMarginHasItems = 25;
        int leftMarginHasNoItems = 4;

        private List<double> objectsPanelHeight = new List<double>();


        public manage2DPhotoAcoustic()
        {
            InitializeComponent();
        }

        public manage2DPhotoAcoustic(Context context)
        {
            InitializeComponent();
            this.Visibility = Visibility.Collapsed;
            this.context = context;
            manage2DUltrasound.tumorWasAdded += onAddTumorCheckbox;
            manage2DUltrasound.tumorWasRemoved += onRemoveTumorCheckbox;
        }

        public void initializeTumors()
        {
            setThicknessAsSelected();
            objectsPanelHeight.Clear();
            checkboxItems.Children.Clear();
            for (int i = 0; i < context.getUltrasoundPart().fileCount; i++)
            {
                objectsPanelHeight.Add(34);
            }
            doHasNoItems();
            objectsPanel.Height = 34;
            UltrasoundPart.sliderValueChanged += OnUltrasoundSliderValueChanged;
        }

        public void setThicknessAsSelected()
        {
            this.selectedItem = -1;
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
            setThicknessAsSelected();
            objectsPanel.Height = objectsPanelHeight[(int)obj];
            checkboxItems.Children.Clear();
            if (context.getImages().getTumorItemsCount() == 0)
            {
                doHasNoItems();
            }
            else
            {
                objectsPanelHeight[slider_value] = 34;
                objectsPanel.Height = 34;
                for (int i = 0; i < context.getImages().getTumorItemsCount(); i++)
                {
                    increaseDropdownHeight();
                    tumorCheckbox checkbox = new tumorCheckbox(this, i, false);
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
                tumorCheckbox newCheckbox = new tumorCheckbox(this, index, false);
                increaseDropdownHeight();
                newCheckbox.Margin = itemMargin;
                checkboxItems.Children.Add(newCheckbox);
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
            if (theRemovedCheckboxIsChecked) setThicknessAsSelected(); //if it was selected set as selected the bladder

            if (checkboxItems.Children.Count == 0) doHasNoItems();
            updateCanvas();

        }
        //-----------------------------------------------------------

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (context.getPhotoAcousticPart().areTherePoints())
            {
                doHasItems();
                addItem();
            }
            else
            {
                CustomMessageBox.Show(context.getMessages().addTumorWithoutThicknessAnnotation, context.getMessages().warning, MessageBoxButton.OK);
            }
        }

        public void addItem()
        {
            if (checkboxItems.Children.Count <= maxItemNumber)
            {
                increaseDropdownHeight();
                int index = context.getImages().addTumor();
                tumorCheckbox checkbox = new tumorCheckbox(this, index);
                checkbox.Margin = itemMargin;
                checkboxItems.Children.Add(checkbox);
                tumorWasAdded(checkbox.getIndex());
            }
        }

        public void removeItem(tumorCheckbox checkbox)
        {
            if (checkbox.check_box.IsChecked == true) setThicknessAsSelected();
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
            if (this.checkboxItems.Children.Count >= 1)
            {
                objectsPanelHeight[slider_value] += checkBoxMinHeight + tumorCheckbox_topMargin;
                objectsPanel.Height = objectsPanelHeight[slider_value];
            }
        }

        public void decreaseDropdownHeight()
        {
            if (objectsPanel.Height > 2 * checkBoxMinHeight)
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
        }

        public void updateCanvas()
        {
            context.getPhotoAcousticPart().doCorrection();
            context.getPhotoAcousticPart().updateCanvas();
        }

        public void doHasNoItems()
        {
            Thickness addButtonMargin = addButton.Margin;
            addButtonMargin.Left = leftMarginHasNoItems;
            addButton.Margin = addButtonMargin;
            objectsPanel.Visibility = Visibility.Collapsed;
            openButton.Content = "\u2B9D";
        }


        Selected2DObject selectedObject = new Selected2DObject() { points = null, metrics = null, polylineColor = null };
        public Selected2DObject getSelectedObject2D()
        {
            try
            {
                if (selectedItem >= 0)
                {
                    selectedObject.points = context.getImages().getTumorPoints(selectedItem);
                    selectedObject.polylineColor = ViewAspects.magenta;
                    selectedObject.metrics = getTumorMetricsString();
                }
                else
                {
                    selectedObject.points = context.getImages().getThicknessPoints();
                    selectedObject.polylineColor = ViewAspects.cyan;
                    selectedObject.metrics = getThicknessMetricsString();
                }

                return selectedObject;
            }
            catch (Exception ex)
            {
                setThicknessAsSelected();
                selectedObject.points = context.getImages().getBladderPoints();
                selectedObject.polylineColor = ViewAspects.cyan;
                selectedObject.metrics = getThicknessMetricsString();
                return selectedObject;
            }
        }


        List<UIElement> polylines = new List<UIElement>();
        List<Point> tempPoints = new List<Point>();
        public List<UIElement> getNoSelectedObject2DPolylines()
        {
            polylines.Clear();
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
        private string getThicknessMetricsString()
        {
            string metricsString;

            metrics = context.getImages().getThicknessMetrics();
            metricsString = context.getMessages().perimeter + " = " + Math.Round(metrics.perimeter, 2) + " " + context.getMessages().mm + Environment.NewLine +
                                                         context.getMessages().area + " = " + Math.Round(metrics.area, 2) + " " + context.getMessages().mmB2;

            if (context.getMainWindow().currentProcess == MainWindow.process.AUTO)
            {
                metricsString += Environment.NewLine + context.getMessages().meanThickness + " = " + Math.Round(metrics.meanThickness, 2) + " " + context.getMessages().mm;
            }
            return metricsString;
        }

        private string getTumorMetricsString()
        {

            metrics = context.getImages().getTumorMetrics(selectedItem);
            return context.getMessages().perimeter + " = " + Math.Round(metrics.perimeter, 2) + " " + context.getMessages().mm + Environment.NewLine +
                                                         context.getMessages().area + " = " + Math.Round(metrics.area, 2) + " " + context.getMessages().mmB2;
        }


        public void updateSelectedObjectMetrics()
        {
            if (selectedItem >= 0)
            {
                context.getImages().recalculateTumorMetrics(selectedItem);
            }
            else
            {
                context.getImages().recalculateThicknessMetrics();
            }
        }
    }
    public class Selected2DObject
    {
        public List<Point> points { set; get; }
        public string metrics { set; get; }
        public SolidColorBrush polylineColor { set; get; }
    }

}
