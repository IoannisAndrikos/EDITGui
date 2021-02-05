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
    /// Interaction logic for Loggin.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        Context context;

        MainWindow mainWindow;
        Messages messages;

        public bool isAuthenticated = false;

        public Login()
        {
            InitializeComponent();
            
        }

        public Login(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            this.messages = new Messages();
            //isAuthenticated = true; //-----------------2
            //mainWindow.doAfterUserAuthentication(); //-------------2
        }

        string username;
        string password;

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            isAuthenticated = false;

            username = textBox_username.Text;
            password = password_box.Password;

            if (username =="lime" && password == "lime")
            {
                isAuthenticated = true;
                mainWindow.doAfterUserAuthentication();
            }
            else
            {
               CustomMessageBox.Show(messages.notCorrectUserPass, messages.warning, MessageBoxButton.OK);
            }
        }

        private void CheckBox_showPassword_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox_showPassword.IsChecked == true)
            {
                reveal_password.Text = password_box.Password;
                reveal_password.Visibility = Visibility.Visible;

            }
            else
            {
                password_box.Password = reveal_password.Text;
                reveal_password.Visibility = Visibility.Collapsed;
            }
        }
    }
}
