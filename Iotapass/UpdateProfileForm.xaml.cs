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
    /// Interaction logic for UpdateProfileForm.xaml
    /// </summary>
    public partial class UpdateProfileForm : Window
    {
        private const int emailLengthRequirement = 5;
        private const int passwordLengthRequirement = 12;
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
        public UpdateProfileForm()
        {
            InitializeComponent();
        }

        private void PasswordEye_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ToggleShowPassword(false, false);
        }
        
        private void PasswordEye_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ToggleShowPassword(true, false);
        }

        private void PasswordEye_MouseLeave(object sender, MouseEventArgs e)
        {
            ToggleShowPassword(true, false);
        }

        private void ReenterEye_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ToggleShowPassword(false, true);
        }

        private void ReenterEye_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ToggleShowPassword(true, true);
        }

        private void ReenterEye_MouseLeave(object sender, MouseEventArgs e)
        {
            ToggleShowPassword(true, true);
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMsg.Content = "";
            ErrorMsg.Visibility = Visibility.Visible;
            
            // If current user is not set, or email is not set, update cannot be performed. Error message appears and the form gets closed.
            if (CurrentUser == null)
            {
                IotapassMessageBox box = new IotapassMessageBox();
                box.Owner = this;
                box.Left = box.Owner.Left + (box.Owner.Width / 2) - box.Width / 2;
                box.Top = box.Owner.Top + (box.Owner.Height / 2) - box.Height / 2;
                box.Show("Error: This profile is empty or no longer exists.", "Error", 1, 4);
                this.Close();
            }
            else if (String.IsNullOrEmpty(CurrentUser.email))
            {
                IotapassMessageBox box = new IotapassMessageBox();
                box.Owner = this;
                box.Left = box.Owner.Left + (box.Owner.Width / 2) - box.Width / 2;
                box.Top = box.Owner.Top + (box.Owner.Height / 2) - box.Height / 2;
                box.Show("Error: This profile is empty or no longer exists.", "Error", 1, 4);
                this.Close();
            }
            // If reenter box is empty,
            else if (String.IsNullOrEmpty(ReenterPasswordBox.Password))
            {
                ErrorMsg.Content = "Enter your current master password.";
            }
            // Otherwise,
            else
            {
                // If the password in reenter box doesn't match with current user password,
                ProfileModel currentProfile = new ProfileModel();
                currentProfile.hint = "";
                currentProfile.email = CurrentUser.email;
                currentProfile.passwd = ReenterPasswordBox.Password;
                currentProfile = SqliteDataAccess.ProfileMatch(currentProfile);
                if (currentProfile.email == "")
                {
                    ErrorMsg.Content = "Incorrect password was re-entered.";
                }
                else
                {
                    bool canUpdateBePerformed = true;
                    // Set to true if a field was changed, for showing a toast when profile is updated
                    bool noFieldsChanged = true;
                    // The new profile is initially set to the profile of the current user (obtained from a query above), start making checks to see if update can be performed
                    ProfileModel newUserProfile = new ProfileModel() { email = currentProfile.email, passwd = currentProfile.passwd, hint = currentProfile.hint };
                    // If email is going to be modified, Check for email validity
                    if (!String.IsNullOrEmpty(EmailTextBox.Text))
                    { 
                        // Is the email invalid?
                        if (!EmailTextBox.Text.Contains("@") || EmailTextBox.Text.Length < emailLengthRequirement)
                        {
                            ErrorMsg.Content = "Invalid email. ";
                            canUpdateBePerformed = false;
                        }
                        else
                        {
                            // Is the email currently in use?
                            if (SqliteDataAccess.DoesProfileExist(new ProfileModel() { email = EmailTextBox.Text }) != 0)
                            {
                                ErrorMsg.Content = "Email is currently in use by another user. ";
                                canUpdateBePerformed = false;
                            }
                            else
                            {
                                newUserProfile.email = EmailTextBox.Text;
                                noFieldsChanged = false;
                            }
                        }
                    
                    }
                    // If the password is going to be modified, check for password validity
                    if (!String.IsNullOrEmpty(MasterPasswordTextBox.Password)) 
                    {
                        // Is the password not long enough?
                        if (MasterPasswordTextBox.Password.Length < passwordLengthRequirement)
                        {
                            ErrorMsg.Content += "Password must be at least 12 characters. ";
                            canUpdateBePerformed=false;
                        }
                        else
                        {
                            newUserProfile.passwd = MasterPasswordTextBox.Password;
                            noFieldsChanged = false;
                        }
                    }
                    // if the hint is going to be modified, (no validity requirements)
                    if (!String.IsNullOrEmpty(HintTextBox.Text)) 
                    { 
                        newUserProfile.hint = HintTextBox.Text;
                        noFieldsChanged = false;
                    }
                    // If the update can be performed (no errors),
                    if (canUpdateBePerformed)
                    {
                        var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
                        // If the email has been changed, the profile, preferences, and credentials all need to be updated.
                        if (newUserProfile.email != currentProfile.email)
                        {
                            // Update preferences
                            SqliteDataAccess.UpdatePreferencesEmail(currentProfile.email, newUserProfile.email);
                            // If password was not modified, set new profile password to "" for use in UpdateProfile
                            if (newUserProfile.passwd == currentProfile.passwd)
                            {
                                newUserProfile.passwd = "";
                            }
                            // Update profile
                            SqliteDataAccess.UpdateProfile(newUserProfile, currentProfile.email);
                            // Update user saved credentials
                            SqliteDataAccess.ReEncryptCredentials(currentProfile.email, newUserProfile.email);
                            // Set the current user email to be the new email
                            
                            if (d != null) { d.OnEmailUpdate(newUserProfile.email); }
                        }
                        // If the email was not changed, just the profile needs to be updated.
                        else
                        {
                            if (newUserProfile.passwd == currentProfile.passwd)
                            {
                                newUserProfile.passwd = "";
                            }
                            SqliteDataAccess.UpdateProfile(newUserProfile, currentProfile.email);
                        }
                        if (d != null)
                        {
                            if (noFieldsChanged)
                            {
                                d.ShowToastMsg("You updated your hint to be empty. If you did not mean to, please change your account details.");
                            }
                            else
                            {
                                d.ShowToastMsg("Your profile has been updated.");
                            }
                        }
                        this.Close();
                    }
                }
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// Show/hide password toggle for the new password and reenter fields.
        /// </summary>
        /// <param name="isReenter"></param>
        private void ToggleShowPassword(bool IsHide, bool IsReenterField)
        {
            // New password
            if (!IsReenterField)
            {
                // Hide
                if (IsHide)
                {
                    MasterPasswordTextBox.Visibility = Visibility.Visible;
                    MasterPasswordUnmask.Visibility = Visibility.Hidden;
                }
                // Show
                else
                {
                    MasterPasswordTextBox.Visibility = Visibility.Hidden;
                    MasterPasswordUnmask.Text = MasterPasswordTextBox.Password;
                    MasterPasswordUnmask.Visibility = Visibility.Visible;
                }
            }
            // Reenter password
            else
            {
                // Hide
                if (IsHide)
                {
                    ReenterPasswordBox.Visibility = Visibility.Visible;
                    ReenterPasswordUnmask.Visibility = Visibility.Hidden;
                }
                // Show
                else
                {
                    ReenterPasswordBox.Visibility = Visibility.Hidden;
                    ReenterPasswordUnmask.Text = ReenterPasswordBox.Password;
                    ReenterPasswordUnmask.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
