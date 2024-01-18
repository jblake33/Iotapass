using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using Iotapass.Models;
using Iotapass.Properties;
using Iotapass.Services;

namespace Iotapass
{ 
    /// <summary>
    /// Interaction logic for SignInWindow.xaml
    /// </summary>
    public partial class SignInWindow : Window, INotifyPropertyChanged
    {
        #region Constants
        SolidColorBrush red = new SolidColorBrush(Colors.Red);
        SolidColorBrush neutral = new SolidColorBrush(Colors.DarkGray);
        private const int emailLengthRequirement = 5;
        private const int passwordLengthRequirement = 12;
        private const string bugReportURL = "https://forms.gle/9yqkY3rKsKZXMBMH6";
        private const string aboutPageURL = "https://github.com/jblake33/Iotapass";
        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;
        internal void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
        // Is the sign up form visible? (By default false)
        private bool _signUpVisible = false;
        internal bool SignUpVisible
        {
            get { return _signUpVisible; }
            set { _signUpVisible = value; }
        }
        public SignInWindow()
        {
            
            InitializeComponent();
            if (!String.IsNullOrEmpty(Properties.Settings.Default.LastSignedInEmail))
            {
                SignInEmailField.Text = Properties.Settings.Default.LastSignedInEmail;
            }
            VersionText.Text = "Version: " + GetVersionNumber();
        }

        // Shut down the application when the sign in window is closed
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
        /// <summary>
        /// Get version number of the application, using System.Reflection
        /// </summary>
        /// <returns></returns>
        private string GetVersionNumber()
        {
            string ver = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            if (string.IsNullOrEmpty(ver)) { return "unknown"; }
            return ver;
        }
        private void ShowSignUpBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SignUpVisible)
            {
                ToggleSignUpFields(false);
                SignUpVisible = false;
            }
            else
            {
                ToggleSignUpFields(true);
                SignUpVisible = true;
            }
        }
        // Shows or hides visibility of sign up fields/ UI elements. If b=true then sign up is becoming visible.
        private void ToggleSignUpFields(bool b)
        {
            Visibility v = new Visibility();
            if (b) { v = Visibility.Visible; }
            else { v = Visibility.Hidden; }
            List<UIElement> list = new List<UIElement>
            {
                SignUpEmailField, SignUpEmailLabel, SignUpPasswordField, SignUpPasswordLabel, SignUpEye, ConfirmPasswordField,SignUpConfirmLabel, SignUpBtn
            };
            foreach (UIElement element in list)
            {
                element.Visibility = v;
            }
            SignUpPasswordUnmask.Visibility = Visibility.Hidden;
            SignUpConfirmUnmask.Visibility = Visibility.Hidden;
            // Sets the default "on enter key pressed" button between sign in and sign up.
            if (b)
            {
                SignInBtn.IsDefault = false;
                SignUpBtn.IsDefault = true;
            }
            else
            {
                SignInBtn.IsDefault = true;
                SignUpBtn.IsDefault = false;
            }
        }
        private void ForgotPasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            SignInEmailField.BorderThickness = new Thickness(1);
            SignInEmailField.BorderBrush = neutral;
            SignInErrorMsg.Content = "";
            SignInErrorMsg.Foreground = red;
            PasswordHintTextBlock.Text = "";
            if (String.IsNullOrEmpty(SignInEmailField.Text))
            {
                SignInEmailField.BorderBrush = red;
                SignInEmailField.BorderThickness = new Thickness(4);
                SignInErrorMsg.Content = "Enter your email address.";
            }
            else
            {
                string myHint = SqliteDataAccess.ReadProfileHint(SignInEmailField.Text);
                if (myHint == "")
                {
                    SignInErrorMsg.Content = "Email address does not exist, please try a different email.";
                }
                else
                {
                    if (myHint == "No hint provided.")
                    {
                        PasswordHintTextBlock.Text = "No hint was provided for the account with the email " + SignInEmailField.Text + ".";
                    }
                    else
                    {
                        PasswordHintTextBlock.Text = "Password hint: " + myHint;
                    }
                }
            }
        }

        private void SignInBtn_Click(object sender, RoutedEventArgs e)
        {
            SignInPasswordField.BorderThickness = new Thickness(1);
            SignInPasswordField.BorderBrush = neutral;
            SignInEmailField.BorderThickness = new Thickness(1);
            SignInEmailField.BorderBrush = neutral;
            SignInErrorMsg.Content = "";
            SignInErrorMsg.Foreground = red;
            bool flag1 = true;
            if (String.IsNullOrEmpty(SignInEmailField.Text))
            {
                SignInEmailField.BorderBrush = red;
                SignInEmailField.BorderThickness = new Thickness(4);
                flag1 = false;
            }
            if (String.IsNullOrEmpty(SignInPasswordField.Password))
            {
                SignInPasswordField.BorderBrush = red;
                SignInPasswordField.BorderThickness = new Thickness(4);
                flag1 = false;
            }
            if (flag1)
            {
                ProfileModel tmp = new ProfileModel();
                tmp.hint = "";
                tmp.email = SignInEmailField.Text;
                tmp.passwd = SignInPasswordField.Password;
                tmp = SqliteDataAccess.ProfileMatch(tmp);
                if (tmp.email == "")
                {
                    SignInErrorMsg.Content = "Incorrect email or password.";
                }
                else
                {
                    SignInErrorMsg.Foreground = neutral;
                    if (RememberMeCheckBox.IsChecked == true)
                    {
                        Properties.Settings.Default.LastSignedInEmail = tmp.email;
                    }
                    else
                    {
                        Properties.Settings.Default.LastSignedInEmail = "";
                    }
                    SignInErrorMsg.Content = "Profile found. Signing in...";
                    // pass the user's profile to App
                    ((App)Application.Current).OnSignIn(tmp, "");
                }
            }
            else
            {
                SignInErrorMsg.Content = "Missing fields.";
            }
        }
        private void SignUpBtn_Click(object sender, RoutedEventArgs e)
        {
            SignUpEmailField.BorderThickness = new Thickness(1);
            SignUpEmailField.BorderBrush = neutral;
            SignUpPasswordField.BorderThickness = new Thickness(1);
            SignUpPasswordField.BorderBrush = neutral;
            ConfirmPasswordField.BorderThickness = new Thickness(1);
            ConfirmPasswordField.BorderBrush= neutral;
            SignUpErrorMsg.Content = "";
            bool flag1 = true;
            // Check if any fields are empty, if so highlight them
            if (String.IsNullOrEmpty(SignUpEmailField.Text)) {
                SignUpEmailField.BorderBrush = red;
                SignUpEmailField.BorderThickness = new Thickness(2);
                flag1 = false;
            }
            if (String.IsNullOrEmpty(SignUpPasswordField.Password))
            {
                SignUpPasswordField.BorderBrush = red;
                SignUpPasswordField.BorderThickness = new Thickness(2);
                flag1 = false;
            }
            if (String.IsNullOrEmpty(ConfirmPasswordField.Password))
            {
                ConfirmPasswordField.BorderBrush = red;
                ConfirmPasswordField.BorderThickness = new Thickness(2);
                flag1 = false;
            }
            // If the fields are not empty,
            if (flag1)
            {
                // If email length is less than 5 or doesn't contain @,
                if (!SignUpEmailField.Text.Contains("@") || SignUpEmailField.Text.Length < emailLengthRequirement)
                {
                    SignUpEmailField.BorderBrush = red;
                    SignUpEmailField.BorderThickness = new Thickness(2);
                    SignUpErrorMsg.Content = "Invalid email.";
                }
                // If passwords do not match,
                else if (SignUpPasswordField.Password != ConfirmPasswordField.Password)
                {
                    SignUpPasswordField.BorderBrush = red;
                    SignUpPasswordField.BorderThickness = new Thickness(2);
                    ConfirmPasswordField.BorderBrush = red;
                    ConfirmPasswordField.BorderThickness = new Thickness(2);
                    SignUpErrorMsg.Content = "Passwords do not match.";
                }
                // Or if password length is too short,
                else if (SignUpPasswordField.Password.Length < passwordLengthRequirement)
                {
                    SignUpPasswordField.BorderBrush = red;
                    SignUpPasswordField.BorderThickness = new Thickness(2);
                    ConfirmPasswordField.BorderBrush = red;
                    ConfirmPasswordField.BorderThickness = new Thickness(2);
                    SignUpErrorMsg.Content = "Password must be at least 12 characters.";
                }
                // Otherwise (email is valid, passwords match, and password length requirement is met),
                else
                {
                    ProfileModel tmp = new ProfileModel();
                    tmp.email = SignUpEmailField.Text;
                    if (SqliteDataAccess.DoesProfileExist(tmp) == 0)
                    {
                        tmp.passwd = SignUpPasswordField.Password;
                        SqliteDataAccess.CreateProfile(tmp);
                        // The default preferences below.
                        SqliteDataAccess.CreatePreferences(new PreferencesModel { email = tmp.email, fontsize = 2, resolution = "1024x768", theme = 2 });
                        SignUpErrorMsg.Content = "Sign up success.";
                        // pass the user's profile to App
                        ((App)Application.Current).OnSignIn(tmp, "Welcome to Iotapass. Please go to Settings > Change account details... to set up a hint for your master password.");
                    }
                    else
                    {
                        SignUpErrorMsg.Content = "The email is currently in use.";
                    }
                }
            }
            else
            {
                SignUpErrorMsg.Content = "Missing fields.";
            }
        }

        private void SignInEye_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowSignInPassword();
        }

        private void SignInEye_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            HideSignInPassword();
        }

        private void SignInEye_MouseLeave(object sender, MouseEventArgs e)
        {
            HideSignInPassword();
        }
        private void ShowSignInPassword()
        {
            SignInPasswordUnmask.Visibility = Visibility.Visible;
            SignInPasswordField.Visibility = Visibility.Hidden;
            SignInPasswordUnmask.Text = SignInPasswordField.Password;
        }

        private void HideSignInPassword()
        {
            SignInPasswordUnmask.Visibility = Visibility.Hidden;
            SignInPasswordField.Visibility = Visibility.Visible;
        }

        private void SignUpEye_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowSignUpPassword();
        }

        private void SignUpEye_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            HideSignUpPassword();
        }

        private void SignUpEye_MouseLeave(object sender, MouseEventArgs e)
        {
            HideSignUpPassword();
        }
        private void ShowSignUpPassword()
        {
            SignUpPasswordField.Visibility = Visibility.Hidden;
            ConfirmPasswordField.Visibility = Visibility.Hidden;
            SignUpPasswordUnmask.Visibility = Visibility.Visible;
            SignUpConfirmUnmask.Visibility = Visibility.Visible;
            
            SignUpPasswordUnmask.Text = SignUpPasswordField.Password;
            SignUpConfirmUnmask.Text = ConfirmPasswordField.Password;
        }
        private void HideSignUpPassword()
        {
            SignUpPasswordUnmask.Visibility = Visibility.Hidden;
            SignUpConfirmUnmask.Visibility = Visibility.Hidden;

            SignUpPasswordField.Visibility = Visibility.Visible;
            ConfirmPasswordField.Visibility = Visibility.Visible;
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer", aboutPageURL);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while attempting to access the link: \r\n" + aboutPageURL + "\r\n\r\nError: " + ex.Message);
            }
        }

        private void ReportBugButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer", bugReportURL);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while attempting to access the link: \r\n" + bugReportURL + "\r\n\r\nError: " + ex.Message);
            }
        }
    }
}
