using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Threading;
using Iotapass.Models;
using Iotapass.Services;

namespace Iotapass
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Implement INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
        // This is the list of credentials for the password screen (the DP for binding)
        private ObservableCollection<CredentialModel> _credentials = new ObservableCollection<CredentialModel>();
        internal ObservableCollection<CredentialModel> Credentials
        {
            get { return _credentials; }
            set
            {
                _credentials = value;
                OnPropertyChanged(nameof(Credentials));
            }
        }
        // This is the filtered list of credentials for the password screen, used to store search results.
        private ObservableCollection<CredentialModel> _credentialsSearch = new ObservableCollection<CredentialModel>();
        internal ObservableCollection<CredentialModel> CredentialsSearch
        {
            get { return _credentialsSearch; }
            set
            {
                _credentialsSearch = value;
                OnPropertyChanged(nameof(CredentialsSearch));
            }
        }
        // This is the user preferences for the settings screen (the DP for binding)
        public PreferencesModel CurrentUserPreferences
        {
            get { return (PreferencesModel)GetValue(CurrentUserPreferencesProperty); }
            set { SetValue(CurrentUserPreferencesProperty, value); }
        }
        // Using a DependencyProperty as the backing store for CurrentUserPreferences.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentUserPreferencesProperty =
            DependencyProperty.Register("CurrentUserPreferences", typeof(PreferencesModel), typeof(MainWindow));


        // Runs on starting the MainWindow.
        public MainWindow()
        {
            CurrentUser = ((App)Application.Current).CurrentUser;
            CurrentUserPreferences = ((App)Application.Current).CurrentUserPref;
            InitializeComponent();
            List<CredentialModel> credList = SqliteDataAccess.ReadCredentials(CurrentUser.email);
            foreach (CredentialModel cred in credList)
            {
                Credentials.Add(cred);
            }
            PasswordsScreenView.DataContext = Credentials;
            SettingsScreenView.DataContext = CurrentUserPreferences;
            
        }

        // When the MainWindow is closed, the "currently signed in" user should be empty
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Set signed in user to an empty profile
            ProfileModel profile = new ProfileModel();
            profile.email = ""; profile.passwd = ""; profile.hint = "";
            ((App)Application.Current).CurrentUser = profile;
            // Update user preferences
            SqliteDataAccess.UpdatePrefernces(CurrentUserPreferences);
            // base closing behavior
            base.OnClosing(e);
        }
        // Shut down the application when the main window is closed
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Environment.Exit(0);
        }

        private void SidebarTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If tab selected is to log out,
            if (SidebarTabs.SelectedItem == Logout)
            {
                SqliteDataAccess.UpdatePrefernces(CurrentUserPreferences);
                this.CurrentUser = new ProfileModel { email = "", passwd = "", hint = "" };
                this.Hide();
                ((App)Application.Current).OnSignOut();
            }
        }
        #region PASSWORD SCREEN METHODS
        internal void NewCredential()
        {
            CreateCredForm form = new CreateCredForm();
            form.Width = this.Width - 80;
            form.Height = this.Height - 80;
            form.Left = this.Left + 40;
            form.Top = this.Top + 40;
            form.CurrentUser = CurrentUser;
            form.Closed += new EventHandler(RefreshCredentialOnClosedForm);
            form.ShowDialog();
        }
        internal void UpdateCredential(CredentialModel cm)
        {
            UpdateCredForm form = new UpdateCredForm(cm);
            form.Width = this.Width - 80;
            form.Height = this.Height - 80;
            form.Left = this.Left + 40;
            form.Top = this.Top + 40;
            form.CurrentUser = CurrentUser;
            form.Closed += new EventHandler(RefreshCredentialOnClosedForm);
            form.ShowDialog();
        }
        internal void DeleteCredential(CredentialModel cm)
        {
            SqliteDataAccess.DeleteCredential(cm);
            RefreshCredentialList(false);
        }
        /// <summary>
        /// Refreshes the credential list in the passwords tab. showMsgBox if set to true will show a toast notification of how many credentials are found upon a refresh.
        /// </summary>
        internal void RefreshCredentialList(bool showMsgBox)
        {
            PasswordsScreenView.DataContext = Credentials;
            List<CredentialModel> credList = SqliteDataAccess.ReadCredentials(CurrentUser.email);
            if (showMsgBox)
            {
                ShowToastMsg("" + credList.Count + " passwords found.");
            }
            Credentials.Clear();
            foreach (CredentialModel cred in credList)
            {
                Credentials.Add(cred);
            }
        }
        // Refresh the list when the forms for updating or creating credentials are closed.
        private void RefreshCredentialOnClosedForm(object? sender, EventArgs? e)
        {
            RefreshCredentialList(false);
        }
        /// <summary>
        /// Searches the current credential list to see if criteria is contained in the website or username of a credential.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        internal bool PerformSearch(string criteria)
        {
            CredentialsSearch.Clear();
            foreach (CredentialModel cred in Credentials)
            {
                if (cred != null)
                {
                    if (cred.website.ToLower().Contains(criteria.ToLower()) || cred.username.ToLower().Contains(criteria.ToLower()))
                    {
                        CredentialsSearch.Add(cred);
                    }
                }
            }
            // If there's any results: DataContext of PasswordsScreenView gets set to the filtered list of credentials
            if (CredentialsSearch.Count > 0)
            {
                PasswordsScreenView.DataContext = CredentialsSearch;
                return true;
            }
            // Otherwise (search query got no results)
            else
            {
                IotapassMessageBox box = new IotapassMessageBox();
                box.Owner = this;
                box.Left = box.Owner.Left + (box.Owner.Width / 2) - box.Width / 2;
                box.Top = box.Owner.Top + (box.Owner.Height / 2) - box.Height / 2;
                box.Show("The search criteria you entered produced 0 results. Check if there was a typo in your search.", "No search results", 1, 1);
                PasswordsScreenView.DataContext = Credentials;
                return false;
            }
        }
        internal void ClearSearch()
        {
            PasswordsScreenView.DataContext = Credentials;
        }
        #endregion
        #region SETTINGS SCREEN METHODS
        internal void ChangeAccountDetails()
        {
            UpdateProfileForm form = new UpdateProfileForm();
            form.Width = this.Width - 80;
            form.Height = this.Height - 80;
            form.Left = this.Left + 40;
            form.Top = this.Top + 40;
            form.CurrentUser = CurrentUser;
            //form.Closed += new EventHandler(RefreshCredentialOnClosedForm);
            form.ShowDialog();
        }
        /// <summary>
        /// Updates the email of the current user when it is changed from the "UpdateProfileForm".
        /// </summary>
        /// <param name="newEmail"></param>
        internal void OnEmailUpdate(string newEmail)
        {
            CurrentUser.email = newEmail;
        }

        /// <summary>
        /// Passes the user's list of credentials to the breach controller, performs a password scan, gets result back as the new credential list. 
        /// </summary>
        internal void PasswordScanStart()
        {
            // Pass the current list of credentials to the breach scan user control inside the settings view.
            this.SettingsScreenView.MyBreachScanControl.CredentialsForScan = Credentials.ToList();
            // Start the scan
            this.SettingsScreenView.MyBreachScanControl.PerformScan();

            // Get the updated list back from the breach scan control, and add the contents in place of the existing credential list.
            List<CredentialModel> credList = this.SettingsScreenView.MyBreachScanControl.CredentialsForScan;

            // This is only done because, by encrypting the password in UpdateCredential, the value change also affects the parameter passed from here (pass by ref). A "copy" is created to resolve this.
            List<CredentialModel> tmpCredList = new List<CredentialModel>();
            int index = 0;

            Credentials.Clear();
            foreach (CredentialModel cred in credList)
            {
                tmpCredList.Add(new CredentialModel { email = cred.email, Id = cred.Id, isBreached = cred.isBreached, notes = cred.notes, passwd = cred.passwd, username = cred.username, website = cred.website });
                Credentials.Add(cred);
                
                // This is fine, since the only field that should change is the isBreached field.
                SqliteDataAccess.UpdateCredential(tmpCredList[index], false);
                index++;
            }
        }
        #endregion
        #region TOAST NOTIFICATION METHODS
        /// <summary>
        /// Close the popup "toast notification" when the user clicks the X
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToastCloseX_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ToastNotifyBox.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// Shows a popup "toast notification" with a message.
        /// </summary>
        /// <param name="msg"></param>
        internal void ShowToastMsg(string msg)
        {
            ToastNotifyBoxMsg.Text = msg;
            ToastNotifyBox.Visibility = Visibility.Visible;
            StartToastCloseTimer();
        }
        private double ToastTimeDuration = 8d;
        /// <summary>
        /// Timer for closing the toast notification automatically. Time is set to 8 seconds
        /// </summary>
        private void StartToastCloseTimer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(ToastTimeDuration);
            timer.Tick += TimerTick;
            timer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            timer.Stop();
            timer.Tick -= TimerTick;
            ToastNotifyBox.Visibility = Visibility.Collapsed;
        }
        #endregion
    }
}