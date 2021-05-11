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
    /// Interaction logic for CreateTumorGroup.xaml
    /// </summary>
    public partial class TumorGroupsWindow : Window
    {
        manageTumorGroups manageGroups;

        public TumorGroupsWindow()
        {
            InitializeComponent();
        }

        public TumorGroupsWindow(manageTumorGroups tumorGroups)
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.Visibility = Visibility.Visible;
            this.manageGroups = tumorGroups;
        }
        public void removeItem(string group)
        {
            manageGroups.removeGroup(group);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            //check should be implemented here!!!!!!
            string title = groupTitle_textBox.Text;

            string message = manageGroups.checkIfGroupAlreadyExists(title);
            if (message != null)
            {
                CustomMessageBox.Show(message, Messages.warningConst, MessageBoxButton.OK);
                return;
            }

            //title = title.Replace(' ', '-');

            TumorGroupItem tumorGroupItem = new TumorGroupItem(this, title);
            manageGroups.addGroup(title);
            groupItems.Children.Add(tumorGroupItem);
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            manageGroups.onTumorGrousWindowClosing();
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            manageGroups.onTumorGrousWindowClosing();
            this.Close();
        }
    }
}
