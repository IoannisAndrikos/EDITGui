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
    public partial class obectsDropdown : UserControl
    {
        public obectsDropdown()
        {
            InitializeComponent();
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
    }
}
