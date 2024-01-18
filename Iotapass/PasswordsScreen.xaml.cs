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
using Iotapass.Models;
using Iotapass.Services;

namespace Iotapass
{
    /// <summary>
    /// Interaction logic for PasswordsScreen.xaml
    /// </summary>
    public partial class PasswordsScreen : UserControl
    {
        // Used for showing the confirmation message when deleting credentials.
        private bool _showDeleteConfirm = true;
        internal bool ShowDeleteConfirm
        {
            get { return _showDeleteConfirm; }
            set { _showDeleteConfirm = value; }
        }
        // Used for the search field, "default color" to revert back to after a search error.
        private Brush _searchBorderColor;
        internal Brush SearchBorderColor
        {
            get { return _searchBorderColor; }
            set { _searchBorderColor = value; }
        }
        SolidColorBrush red = new SolidColorBrush(Colors.Red);
        public PasswordsScreen()
        {
            InitializeComponent();
            SearchBorderColor = SearchBox.BorderBrush.Clone();
        }


        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // FOR BINDING FIX 10/18
            var cred = ((Button)sender).Tag as CredentialModel;
            if (cred == null)
            {
                IotapassMessageBox box = new IotapassMessageBox();
                box.Owner = Window.GetWindow(App.Current.MainWindow);
                box.Left = box.Owner.Left + (box.Owner.Width / 2) - box.Width / 2;
                box.Top = box.Owner.Top + (box.Owner.Height / 2) - box.Height / 2;
                box.Show("This credential no longer exists, or it is invalid.", "Delete failed", 1, 4);
                return;
            }
            if (ShowDeleteConfirm)
            {
                IotapassMessageBox box = new IotapassMessageBox();
                box.Owner = Window.GetWindow(App.Current.MainWindow);
                box.Left = box.Owner.Left + (box.Owner.Width / 2) - box.Width / 2;
                box.Top = box.Owner.Top + (box.Owner.Height / 2) - box.Height / 2;
                int result = box.Show("Are you sure you want to delete your password for " + cred.website + "?", "Delete password for " + cred.username, 3, 2);
                box.Close();
                if (result == 1 || result == 2)
                {
                    if (result == 2)
                    {
                        ShowDeleteConfirm = false;
                    }
                    var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
                    d.DeleteCredential(cred);
                    d.ShowToastMsg("Password deleted successfully.");
                }
            }
            else if (!ShowDeleteConfirm)
            {
                var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
                d.DeleteCredential(cred);
                d.ShowToastMsg("Password deleted successfully.");
            }
        }
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // FOR BINDING FIX 10/18
            var cred = ((Button)sender).Tag as CredentialModel;
            var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
            d.UpdateCredential(cred);
        }
        private void NewCredentialButton_Click(object sender, RoutedEventArgs e)
        {
            // FOR BINDING FIX 10/18
            var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
            d.NewCredential();
        }
        // Refresh the list when the refresh button gets clicked.
        private void RefreshCredentialButton_Click(object sender, RoutedEventArgs e)
        {
            // FOR BINDING FIX 10/18
            var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
            d.RefreshCredentialList(true);
        }
        //Enable mouse scrolling for listbox.
        private void ListViewScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - (e.Delta/10));
            e.Handled = true;
        }
        private void PasswordEye_PreviewMouseDown(object sender, RoutedEventArgs e)
        {
            var item = ((Viewbox)sender).Tag as StackPanel;
            if (item == null) return;
            // password dots
            item.Children[0].Visibility = Visibility.Collapsed;
            // password unmask
            item.Children[1].Visibility = Visibility.Visible;
        }
        private void PasswordEye_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var item = ((Viewbox)sender).Tag as StackPanel;
            if (item == null) return;
            // password dots
            item.Children[0].Visibility = Visibility.Visible;
            // password unmask
            item.Children[1].Visibility = Visibility.Collapsed;
        }

        private void PasswordEye_MouseLeave(object sender, MouseEventArgs e)
        {
            /*
            var item = ((Viewbox)sender).Tag as StackPanel;
            if (item == null) return;
            // password dots
            item.Children[0].Visibility = Visibility.Collapsed;
            // password unmask
            item.Children[1].Visibility = Visibility.Visible;
            */
        }

        /// <summary>
        /// Copies password to clipboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClipboardIcon_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var cred = ((Viewbox)sender).Tag as CredentialModel;
            if (cred == null) return;

            Iotapass.Services.Clipboard.Copy(cred.passwd);
            var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
            if (d == null) return;
            d.ShowToastMsg("Password for " + cred.username + " copied.");
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBorderColor = SearchBox.BorderBrush.Clone();
                SearchBox.BorderBrush = red;
            }
            else
            {
                // Set color back to what it was if it was initially red
                if (SearchBox.BorderBrush == red)
                {
                    SearchBox.BorderBrush = SearchBorderColor;
                }
                var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
                if (d == null) return;
                if (d.PerformSearch(SearchBox.Text))
                {
                    XClearSearch.Visibility = Visibility.Visible;
                    XClearSearch.IsHitTestVisible = true;
                }
            }
        }

        private void XClearSearch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchBox.Text = "";
            XClearSearch.Visibility = Visibility.Hidden;
            XClearSearch.IsHitTestVisible = false;
            var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
            if (d == null) return;
            d.ClearSearch();
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (String.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    SearchBorderColor = SearchBox.BorderBrush.Clone();
                    SearchBox.BorderBrush = red;
                }
                else
                {
                    // Set color back to what it was if it was initially red
                    if (SearchBox.BorderBrush == red)
                    {
                        SearchBox.BorderBrush = SearchBorderColor;
                    }
                    var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
                    if (d == null) return;
                    if (d.PerformSearch(SearchBox.Text))
                    {
                        XClearSearch.Visibility = Visibility.Visible;
                        XClearSearch.IsHitTestVisible = true;
                    }
                }
            }
        }

        /// <summary>
        /// Click to open the Help window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow help = new HelpWindow();
            help.Show();
        }
    }
}
