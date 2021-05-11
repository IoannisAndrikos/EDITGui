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
    /// Interaction logic for TumorGroupItem.xaml
    /// </summary>
    public partial class TumorGroupItem : UserControl
    {

        TumorGroupsWindow TumorGroupsWindow;

        public TumorGroupItem(TumorGroupsWindow tumorGroupsWindow, string groupName)
        {
            InitializeComponent();
            this.TumorGroupsWindow = tumorGroupsWindow;
            this.groupName.Content = groupName.ToString();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            TumorGroupsWindow.removeItem(this.groupName.Content.ToString());
            TumorGroupsWindow.groupItems.Children.Remove(this);
        }
    }
}
