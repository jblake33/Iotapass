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
using System.Diagnostics; // for opening URL
using Iotapass.Models;
using Iotapass.Services;

namespace Iotapass
{
    /// <summary>
    /// Interaction logic for SettingsScreen.xaml
    /// </summary>
    public partial class SettingsScreen : UserControl
    {
        private string bugReportURL = "https://forms.gle/9yqkY3rKsKZXMBMH6";
        private string aboutPageURL = "https://github.com/jblake33/Iotapass";
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
        public SettingsScreen()
        {
            CurrentUser = ((App)Application.Current).CurrentUser;
            InitializeComponent();
            string currRes = (Math.Round(Application.Current.MainWindow.ActualWidth)) + "x" + (Math.Round(Application.Current.MainWindow.ActualHeight));
            Properties.Settings.Default.Resolution = currRes;
            RefreshResComboBox();
            InitButtonBind();
            VersionText.Text = "Version: " + GetVersionNumber();
        }

        /// <summary>
        /// Refresh the items in the Resolution drop down.
        /// </summary>
        private void RefreshResComboBox()
        {
            string currRes = (Math.Round(Application.Current.MainWindow.ActualWidth)) + "x" + (Math.Round(Application.Current.MainWindow.ActualHeight));
            Properties.Settings.Default.Resolution = currRes;
            ResolutionDropDown.Items.Clear();
            ResolutionDropDown.Items.Add(currRes);
            ResolutionDropDown.SelectedValue = currRes;
            
            // Below are sample resolutions: Format is "600x480" for a 600 width, 480 height window.
            ResolutionDropDown.Items.Add("640x480");
            ResolutionDropDown.Items.Add("800x600");
            ResolutionDropDown.Items.Add("1024x768");
            ResolutionDropDown.Items.Add("1280x800");
            ResolutionDropDown.Items.Add("1440x900");
            ResolutionDropDown.Items.Add("1680x1050");
            ResolutionDropDown.Items.Add("1920x1200");
        }

        /// <summary>
        /// Sets the initially selected buttons in the font and color groups to match the current user preferences.
        /// </summary>
        private void InitButtonBind()
        {
            // Color mode buttons
            if (int.TryParse(Properties.Settings.Default.ColorMode.Trim(), out int colormode))
            {
                if (colormode == 1) { LightMode.IsChecked = true; }
                else if (colormode == 2) { NeutralMode.IsChecked = true; }
                else if (colormode == 3) { DarkMode.IsChecked = true; }
                else { NeutralMode.IsChecked = true; }
            }
            // Font size buttons
            if (int.TryParse(Properties.Settings.Default.FontSize.Trim(), out int fontsize))
            {
                if (fontsize == 1) { SmallFont.IsChecked = true; }
                else if (fontsize == 2) { MedFont.IsChecked = true; }
                else if (fontsize == 3) { LargeFont.IsChecked = true; }
                else { MedFont.IsChecked = true; }
            }
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
        private void LightMode_Checked(object sender, RoutedEventArgs e)
        {
            var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
            if (d == null) return;
            d.CurrentUserPreferences.theme = 1;
            Properties.Settings.Default.ColorMode = "1";
            Properties.Settings.Default.Save();
        }

        private void NeutralMode_Checked(object sender, RoutedEventArgs e)
        {
            var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
            if (d == null) return;
            d.CurrentUserPreferences.theme = 2;
            Properties.Settings.Default.ColorMode = "2";
            Properties.Settings.Default.Save();
        }

        private void DarkMode_Checked(object sender, RoutedEventArgs e)
        {
            var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
            if (d == null) return;
            d.CurrentUserPreferences.theme = 3;
            Properties.Settings.Default.ColorMode = "3";
            Properties.Settings.Default.Save();
        }

        private void SmallFont_Checked(object sender, RoutedEventArgs e)
        {
            var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
            if (d == null) return;
            d.CurrentUserPreferences.fontsize = 1;
            Properties.Settings.Default.FontSize = "1";
            Properties.Settings.Default.Save();
        }

        private void MedFont_Checked(object sender, RoutedEventArgs e)
        {
            var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
            if (d == null) return;
            d.CurrentUserPreferences.fontsize = 2;
            Properties.Settings.Default.FontSize = "2";
            Properties.Settings.Default.Save();
        }

        private void LargeFont_Checked(object sender, RoutedEventArgs e)
        {
            var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
            if (d == null) return;
            d.CurrentUserPreferences.fontsize = 3;
            Properties.Settings.Default.FontSize = "3";
            Properties.Settings.Default.Save();
        }

        private void ResolutionDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tmp = ResolutionDropDown.SelectedItem;
            if (tmp == null) return;
            var res = tmp.ToString();
            if (res.Contains('x') && res != Properties.Settings.Default.Resolution)
            {
                try
                {
                    int w = int.Parse(res.Substring(0, res.IndexOf('x')));
                    int h = int.Parse(res.Substring(res.IndexOf('x') + 1));
                    Application.Current.MainWindow.Width = (double)w;
                    Application.Current.MainWindow.Height = (double)h;
                    var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
                    if (d == null) throw new Exception("MainWindow null exception");
                    d.CurrentUserPreferences.resolution = res;
                    Properties.Settings.Default.Resolution = res;
                    Properties.Settings.Default.Save();
                }
                // if an error occurs trying to parse the resolution, do nothing
                catch (Exception ex) 
                { 
                    IotapassMessageBox box = new IotapassMessageBox();
                    box.Owner = Window.GetWindow (App.Current.MainWindow);
                    box.Left = box.Owner.Left + (box.Owner.Width / 2) - box.Width / 2;
                    box.Top = box.Owner.Top + (box.Owner.Height / 2) - box.Height / 2;
                    box.Show("Unexpected error: " + ex.Message, "Error", 1, 4);
                }
            }
        }

        /// <summary>
        /// Deletes the current user's passwords.
        /// </summary>
        private void DeleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser != null)
            {
                IotapassMessageBox box = new IotapassMessageBox();
                box.Owner = Window.GetWindow(App.Current.MainWindow);
                box.Left = box.Owner.Left + (box.Owner.Width / 2) - box.Width / 2;
                box.Top = box.Owner.Top + (box.Owner.Height / 2) - box.Height / 2;
                int x = box.Show("Are you sure you want to delete all of your passwords? You can not reverse this decision.", "Delete all passwords", 2, 3);
                if (x == 1)
                {
                    SqliteDataAccess.DeleteAllCredentials(CurrentUser);
                    var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
                    if (d == null)
                    {
                        box = new IotapassMessageBox();
                        box.Left = box.Owner.Left + (box.Owner.Width / 2) - box.Width / 2;
                        box.Top = box.Owner.Top + (box.Owner.Height / 2) - box.Height / 2;
                        box.Show("MainWindow nullreference error", "Error", 1, 4);
                        return;
                    }
                    d.RefreshCredentialList(false);
                }
            }
        }

        /// <summary>
        /// Start a scan of the current user's passwords. Also toggles visibility of the breach scan user control.
        /// </summary>
        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            if (MyBreachScanControl.Visibility == Visibility.Collapsed)
            {
                // If breach scan control is not visible, then make it visible and start the scan
                var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
                MyBreachScanControl.Visibility = Visibility.Visible;
                d.PasswordScanStart();
            }
            else
            {
                // Otherwise, make the breach scan control collapsed
                MyBreachScanControl.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Opens the form to change account details.
        /// </summary>
        private void ChangeAccount_Click(object sender, RoutedEventArgs e)
        {
            var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
            if (d == null)
            {
                IotapassMessageBox box = new IotapassMessageBox();
                box.Owner = Window.GetWindow(App.Current.MainWindow);
                box.Left = box.Owner.Left + (box.Owner.Width / 2) - box.Width / 2;
                box.Top = box.Owner.Top + (box.Owner.Height / 2) - box.Height / 2;
                box.Show("MainWindow nullreference error", "Error", 1, 4);
                return;
            }
            d.ChangeAccountDetails();
        }
        
        /// <summary>
        /// Deletes the current user's passwords, profile, preferences.
        /// </summary>
        private void DeleteAccButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser != null)
            {
                IotapassMessageBox box = new IotapassMessageBox();
                box.Owner = Window.GetWindow(App.Current.MainWindow);
                box.Left = box.Owner.Left + (box.Owner.Width / 2) - box.Width / 2;
                box.Top = box.Owner.Top + (box.Owner.Height / 2) - box.Height / 2;
                int x = box.Show("Are you sure you want to delete your account? You can not reverse this decision.", "Delete account", 2, 3);
                if (x == 1)
                {
                    // Delete user credentials, preferences, and their profile
                    SqliteDataAccess.DeleteAllCredentials(CurrentUser);
                    SqliteDataAccess.DeletePrefernces(CurrentUser.email);
                    SqliteDataAccess.DeleteProfile(CurrentUser);
                    // Sign out
                    var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
                    if (d != null) 
                        d.Hide();
                    ((App)Application.Current).OnSignOut();
                }
            }
            else
            {
                IotapassMessageBox box = new IotapassMessageBox();
                box.Owner = Window.GetWindow(App.Current.MainWindow);
                box.Left = box.Owner.Left + (box.Owner.Width / 2) - box.Width / 2;
                box.Top = box.Owner.Top + (box.Owner.Height / 2) - box.Height / 2;
                box.Show("Null user error", "Error", 1, 4);
            }
        }

        private void RefreshIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RefreshResComboBox();
            var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
            d.ShowToastMsg("Resolutions list refreshed.");
        }

        #region External links
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer", aboutPageURL);
            }
            catch (Exception ex)
            {
                var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
                if (d != null)
                    d.ShowToastMsg("Error attempting to open the link: " + aboutPageURL + "");
            }
        }

        private void BugReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer", bugReportURL);
            }
            catch (Exception ex)
            {
                var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
                if (d != null)
                    d.ShowToastMsg("Error attempting to open the link: " + bugReportURL + "");
            }

        }
        #endregion
        /// <summary>
        /// Enables scrolling for the settings page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - (e.Delta / 10));
            e.Handled = true;
        }
    }
}
