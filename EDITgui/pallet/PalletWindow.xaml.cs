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
using System.Windows.Shapes;

namespace EDITgui
{
    /// <summary>
    /// Interaction logic for PalletWindow.xaml
    /// </summary>
    public partial class PalletWindow : Window
    {
        public enum selectionItem { bladder, outewall, layer, OXY, DeOXY, none}

        Pallet pallet;

        public selectionItem selected = selectionItem.none;
        List<Object3DViewAspects> currentView;
        List<Object3DViewAspects> newView;

        public PalletWindow(Pallet pallet)
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.pallet = pallet;
            this.Visibility = Visibility.Visible;
            this.currentView = pallet.getPalletColorsAndOpacities().ToList();
            this.newView = pallet.getPalletColorsAndOpacities().ToList();
            setInitialBladderClicked();
        }

        private void setCurrentSliderValuesFileds(double[] colorValues, double opacityValue)
        {
            red_slider.Value = colorValues[0] *255;
            green_slider.Value = colorValues[1] *255;
            blue_slider.Value = colorValues[2] *255;
            opacity_slider.Value = opacityValue *100;
        }

        private void Red_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            updateRecColor();
        }

        private void Green_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            updateRecColor();
        }

        private void Blue_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            updateRecColor();
        }

        private void Opacity_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            updateRecColor();
        }

        private string getSelectedStringName()
        {
            switch (selected)
            {
                case selectionItem.bladder:
                    return Messages.bladderGeometry;
                case selectionItem.outewall:
                    return Messages.outerWallGeometry;
                case selectionItem.layer:
                    return Messages.layerGeometry;
                case selectionItem.OXY:
                    return Messages.oxyGeometry;
                case selectionItem.DeOXY:
                    return Messages.deoxyGeometry;
                default:
                    return null;
            }
        }

        private void updateRecColor()
        {
            rec.Fill = new SolidColorBrush(Color.FromRgb((byte)red_slider.Value, (byte)green_slider.Value, (byte)blue_slider.Value));
            rec.Opacity = ((double)opacity_slider.Value)/ 100;
        }


        private void CheckBox_Bladder_Click(object sender, RoutedEventArgs e)
        {
            updateOnSelectCheckbox();
            this.checkBox_Bladder.IsChecked = true;
            selected = selectionItem.bladder;
            unselectRestItems(this.checkBox_Bladder);
            setCurrentSliderValuesFileds(newView.Find(x => x.objectName == getSelectedStringName()).colors, newView.Find(x => x.objectName == getSelectedStringName()).opacity);

        }


        private void setInitialBladderClicked()
        {
            this.checkBox_Bladder.IsChecked = true;
            selected = selectionItem.bladder;
            unselectRestItems(this.checkBox_Bladder);
            setCurrentSliderValuesFileds(newView.Find(x => x.objectName == getSelectedStringName()).colors, newView.Find(x => x.objectName ==getSelectedStringName()).opacity);

        }

        private void CheckBox_OuterWall_Click(object sender, RoutedEventArgs e)
        {
            updateOnSelectCheckbox();
            this.checkBox_OuterWall.IsChecked = true;
            selected = selectionItem.outewall;
            unselectRestItems(this.checkBox_OuterWall);
            setCurrentSliderValuesFileds(newView.Find(x => x.objectName == getSelectedStringName()).colors, newView.Find(x => x.objectName == getSelectedStringName()).opacity);
        }

        private void CheckBox_Layer_Click(object sender, RoutedEventArgs e)
        {
            updateOnSelectCheckbox();
            this.checkBox_Layer.IsChecked = true;
            selected = selectionItem.layer;
            unselectRestItems(this.checkBox_Layer);
            setCurrentSliderValuesFileds(newView.Find(x => x.objectName == getSelectedStringName()).colors, newView.Find(x => x.objectName == getSelectedStringName()).opacity);
        }

        private void CheckBox_OXY_Click(object sender, RoutedEventArgs e)
        {
            updateOnSelectCheckbox();
            this.checkBox_OXY.IsChecked = true;
            selected = selectionItem.OXY;
            unselectRestItems(this.checkBox_OXY);
            setCurrentSliderValuesFileds(newView.Find(x => x.objectName == getSelectedStringName()).colors, newView.Find(x => x.objectName == getSelectedStringName()).opacity);
        }

        private void CheckBox_DeOXY_Click(object sender, RoutedEventArgs e)
        {
            updateOnSelectCheckbox();
            this.checkBox_DeOXY.IsChecked = true;
            selected = selectionItem.DeOXY;
            unselectRestItems(this.checkBox_DeOXY);
            setCurrentSliderValuesFileds(newView.Find(x => x.objectName == getSelectedStringName()).colors, newView.Find(x => x.objectName == getSelectedStringName()).opacity);
        }


        public void unselectRestItems(CheckBox checkedItem)
        {
            foreach (CheckBox ch in checkBoxItems.Children)
            {
                if (ch.Content != checkedItem.Content)
                {
                    ch.IsChecked = false;
                }

            }
        }


        private void updateOnSelectCheckbox()
        {
            newView.Find(x => x.objectName == getSelectedStringName()).colors[0] = red_slider.Value / 255;
            newView.Find(x => x.objectName == getSelectedStringName()).colors[1] = green_slider.Value / 255;
            newView.Find(x => x.objectName == getSelectedStringName()).colors[2] = blue_slider.Value / 255;
            newView.Find(x => x.objectName == getSelectedStringName()).opacity = opacity_slider.Value / 100;
        }

       
        private void doSave(List<Object3DViewAspects> view)
        {
            updateOnSelectCheckbox();
            pallet.updatePalletColorsAndOpacities(view);
            pallet.updateRenderer();
        }

        private void Button_Apply_Click(object sender, RoutedEventArgs e)
        {
            doSave(newView);
        }


        private void Button_Reset_Click(object sender, RoutedEventArgs e)
        {
            newView = currentView;
            setCurrentSliderValuesFileds(newView.Find(x => x.objectName == getSelectedStringName()).colors, newView.Find(x => x.objectName == getSelectedStringName()).opacity);
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            doSave(currentView);
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            pallet.onPalletWindowClosing();
        }
    }
}
