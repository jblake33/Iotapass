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
    /// Interaction logic for UpdateCredForm.xaml
    /// </summary>
    public partial class UpdateCredForm : Window
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
        private CredentialModel _currentModel = new CredentialModel();
        internal CredentialModel CurrentModel
        {
            get { return _currentModel; }
            set { _currentModel = value; }
        }
        private bool _isPwdGenVisible;
        internal bool IsPwdGenVisible
        {
            get { return _isPwdGenVisible; }
            set { _isPwdGenVisible = value; }
        }
        public UpdateCredForm(CredentialModel model)
        {
            InitializeComponent();
            if (model == null) 
            {
                IotapassMessageBox box = new IotapassMessageBox();
                box.Owner = this;
                box.Left = box.Owner.Left + (box.Owner.Width / 2) - box.Width / 2;
                box.Top = box.Owner.Top + (box.Owner.Height / 2) - box.Height / 2;
                box.Show("Error: This credential is empty or no longer exists.", "Error", 1, 4);
                this.Close();
            }
            else
            {
                CurrentModel = model;
                websiteTextBox.Text = CurrentModel.website;
                usernameTextBox.Text = CurrentModel.username;
                passwordTextBox.Password = CurrentModel.passwd;
                notesTextBox.Text = CurrentModel.notes;
                IsPwdGenVisible = false;
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if fields are empty. Do not update if any required field is empty.
            if (String.IsNullOrWhiteSpace(websiteTextBox.Text) || String.IsNullOrWhiteSpace(usernameTextBox.Text) || String.IsNullOrWhiteSpace(passwordTextBox.Password))
            {
                IotapassMessageBox box = new IotapassMessageBox();
                box.Owner = this;
                box.Left = box.Owner.Left + (box.Owner.Width / 2) - box.Width / 2;
                box.Top = box.Owner.Top + (box.Owner.Height / 2) - box.Height / 2;
                box.Show("Error: Missing fields. The website, username, and password are required fields.", "Error", 1, 4);
            }
            // Otherwise (required fields all have text)
            else
            {
                CurrentModel.website = websiteTextBox.Text;
                CurrentModel.username = usernameTextBox.Text;
                bool passNotChangedFlag = false;
                // Only set the "isBreached" to 0 if the password was updated.
                if (CurrentModel.passwd != passwordTextBox.Password)
                {
                    CurrentModel.isBreached = 0;
                }
                // if isBreached is not 0 and the user didn't  update their password,
                else if (CurrentModel.isBreached != 0)
                { 
                    // this is for printing an extra message below
                    passNotChangedFlag = true;
                }
                if (passwordTextBox.Password != passwordTextBox.Password.Trim())
                {
                    IotapassMessageBox box = new IotapassMessageBox();
                    box.Owner = this;
                    box.Left = box.Owner.Left + (box.Owner.Width / 2) - box.Width / 2;
                    box.Top = box.Owner.Top + (box.Owner.Height / 2) - box.Height / 2;
                    box.Show("The password you entered contains spaces. Would you like to remove these spaces?", "Password contains spaces", 4, 1);
                    if (box.MessageBoxResult == 1)
                    {
                        CurrentModel.passwd = passwordTextBox.Password.Trim();
                    }
                    else
                    {
                        CurrentModel.passwd = passwordTextBox.Password;
                    }
                }
                else
                {
                    CurrentModel.passwd = passwordTextBox.Password;
                }  
                
                CurrentModel.notes = notesTextBox.Text;
                SqliteDataAccess.UpdateCredential(CurrentModel, false);
                var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
                if (d == null) return;
                d.ShowToastMsg("Password for " + CurrentModel.website + " successfully updated." + (passNotChangedFlag ? " This password should be changed." : ""));
                this.Close();
            }
            
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

        private void PasswordEye_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            passwordUnmask.Text = passwordTextBox.Password;
            passwordTextBox.Visibility = Visibility.Hidden;
            passwordUnmask.Visibility = Visibility.Visible;
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
    }
}
