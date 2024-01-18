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
using Iotapass.Models;
using Iotapass.Services;

namespace Iotapass
{
    /// <summary>
    /// Interaction logic for CreateCredForm.xaml
    /// </summary>
    public partial class CreateCredForm : Window
    {
        private ProfileModel _currentUser;
        internal ProfileModel CurrentUser
        {
            get
            {
                return _currentUser;
            }
            set
            {
                _currentUser = value;
            }
        }
        private bool _isPwdGenVisible;
        internal bool IsPwdGenVisible
        {
            get { return _isPwdGenVisible; }
            set { _isPwdGenVisible = value; }
        }
        public CreateCredForm()
        {
            InitializeComponent();
            IsPwdGenVisible = false;
        }

        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            // If the required fields are not empty,
            if (websiteTextBox.Text != "" && usernameTextBox.Text != "" && passwordTextBox.Password != "")
            {
                CredentialModel cm = new CredentialModel()
                {
                    email = CurrentUser.email,
                    website = websiteTextBox.Text,
                    username = usernameTextBox.Text,
                    isBreached = 0
                };
                if (String.IsNullOrEmpty(notesTextBox.Text))
                {
                    cm.notes = "";
                }
                else { cm.notes = notesTextBox.Text; }
                // Pasting a password may leave a space at the end. This prompts the user if there's a space.
                if (passwordTextBox.Password != passwordTextBox.Password.Trim())
                {
                    IotapassMessageBox box = new IotapassMessageBox();
                    box.Owner = this;
                    box.Left = box.Owner.Left + (box.Owner.Width / 2) - box.Width / 2;
                    box.Top = box.Owner.Top + (box.Owner.Height / 2) - box.Height / 2;
                    box.Show("The password you entered contains spaces. Would you like to remove these spaces?", "Password contains spaces", 4, 1);
                    if (box.MessageBoxResult == 1)
                    {
                        cm.passwd = passwordTextBox.Password.Trim();
                    }
                    else
                    {
                        cm.passwd = passwordTextBox.Password;
                    }
                }
                else
                {
                    cm.passwd = passwordTextBox.Password;
                }
                SqliteDataAccess.CreateCredential(cm);
                var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
                if (d == null) return;
                d.ShowToastMsg("New password for " + cm.website + " successfully created.");
                this.Close();
            }
            // otherwise, some fields are missing. Display an error message stating what fields are missing.
            else
            {
                string msgTxt = "";
                if (websiteTextBox.Text == "")
                {
                    msgTxt += " website";
                }
                if (usernameTextBox.Text == "")
                {
                    if (msgTxt != "")
                    {
                        msgTxt += ",";
                    }
                    msgTxt += " username";
                }
                if (passwordTextBox.Password == "")
                {
                    if (msgTxt != "")
                    {
                        msgTxt += ",";
                    }
                    msgTxt += " password";
                }
                IotapassMessageBox box = new IotapassMessageBox();
                box.Owner = this;
                box.Left = this.Left + (this.Width / 2) - box.Width / 2;
                box.Top = this.Top + (this.Height / 2) - box.Height / 2;
                box.Show("You must enter:" + msgTxt + ".", "Missing fields", 1, 4);
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PasswordLightBulb_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsPwdGenVisible)
            {
                IsPwdGenVisible = true;
                PasswordGenPopup.IsOpen = true;
            }
            else if (IsPwdGenVisible)
            {
                IsPwdGenVisible = false;
                PasswordGenPopup.IsOpen = false;
            }
        }

        private void PasswordEye_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            passwordTextBox.Visibility = Visibility.Visible;
            passwordUnmask.Visibility = Visibility.Hidden;
        }

        private void PasswordEye_MouseLeave(object sender, MouseEventArgs e)
        {
            passwordTextBox.Visibility = Visibility.Visible;
            passwordUnmask.Visibility = Visibility.Hidden;
        }

        private void PasswordEye_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            passwordUnmask.Text = passwordTextBox.Password;
            passwordTextBox.Visibility = Visibility.Hidden;
            passwordUnmask.Visibility = Visibility.Visible;
        }
    }
}
