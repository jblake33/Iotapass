using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Iotapass.Models;
using Iotapass.Services;

namespace Iotapass
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //internal int mainWidth = 600, mainHeight = 480;
        private ProfileModel? _currentUser;
        internal ProfileModel? CurrentUser
        {
            get => _currentUser;
            set => _currentUser = value;
        }
        private PreferencesModel? _currentUserPref;
        internal PreferencesModel? CurrentUserPref
        {
            get => _currentUserPref;
            set => _currentUserPref = value;
        }
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);
            
            ProfileModel profile = new ProfileModel();
            profile.email = "%%"; profile.passwd = ""; profile.hint = "";
            CurrentUser = profile;
            App.Current.MainWindow = new SignInWindow();
            App.Current.MainWindow.ShowDialog();
            // If after the dialog is closed the current user is still empty, then shut down the application
            if (CurrentUser.email == "%%")
            {
                this.Shutdown();
            }
        }
        // Called by SignInWindow, when user successfully signs in or signs up. Special message in toast notification can be displayed, or left empty for no message.
        internal void OnSignIn(ProfileModel p, string specialMsg)
        {
            App.Current.MainWindow.Hide();
            CurrentUser = p;
            if (SqliteDataAccess.ReadPreferences(p.email).Count == 0)
            {
                SqliteDataAccess.CreatePreferences(new PreferencesModel { email = p.email, fontsize = 2, resolution = "1024x768", theme = 2 });
            }
            CurrentUserPref = SqliteDataAccess.ReadPreferences(p.email)[0];
            OnSignIn_LoadPreferences();
            App.Current.MainWindow = new MainWindow();
            int w = 800;
            int h = 600;
            try
            {
                // Attempt to parse resolution and set the values to be the width/height of window
                w = int.Parse(CurrentUserPref.resolution.Substring(0, CurrentUserPref.resolution.IndexOf('x')));
                h = int.Parse(CurrentUserPref.resolution.Substring(CurrentUserPref.resolution.IndexOf('x') + 1));
            }
            catch (Exception ex) { }
            App.Current.MainWindow.Width = w;
            App.Current.MainWindow.Height = h;
            // If the special message is not null or empty, display it in a toast
            if (!string.IsNullOrEmpty(specialMsg))
            {
                var d = (App.Current.MainWindow as MainWindow);
                if (d != null) d.ShowToastMsg(specialMsg);
            }
            App.Current.MainWindow.ShowDialog();

        }
        // Load user preferences on sign in.
        private void OnSignIn_LoadPreferences()
        {
            if (CurrentUserPref == null) return;
            Iotapass.Properties.Settings.Default.Resolution = CurrentUserPref.resolution;
            Iotapass.Properties.Settings.Default.ColorMode = "" + CurrentUserPref.theme;
            Iotapass.Properties.Settings.Default.FontSize = "" + CurrentUserPref.fontsize;
        }
        // Called by MainWindow, when user logs out.
        internal void OnSignOut()
        {
            ProfileModel profile = new ProfileModel();
            profile.email = ""; profile.passwd = ""; profile.hint = "";
            CurrentUser = profile;
            App.Current.MainWindow = new SignInWindow();
            App.Current.MainWindow.Show();
        }
    }
}
