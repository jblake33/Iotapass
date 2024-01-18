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
using Iotapass.Models;
using System.Text.RegularExpressions;
using Iotapass.Services;

namespace Iotapass
{
    /// <summary>
    /// Interaction logic for GeneratePasswords.xaml
    /// </summary>
    public partial class GeneratePasswords : UserControl
    {
        private PwdGenerator pwd;
        private Random random = new Random();
        private int size;
        // Colors to use for neutral/error
        private SolidColorBrush neutral = new SolidColorBrush(Color.FromRgb (171, 173, 179));
        private SolidColorBrush error = new SolidColorBrush(Color.FromRgb(240, 40, 40));
        public GeneratePasswords()
        {
            size = 0;
            pwd = new PwdGenerator();
            InitializeComponent();
        }

        private void MaxSizeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // If the text entered is an integer,
            if (int.TryParse(MaxSizeTextBox.Text, out int x)) {
                MaxSizeTextBox.BorderBrush = neutral;
                size = x;
            }
            // if the text entered is not an integer,
            else
            {
                MaxSizeTextBox.BorderBrush = error;
            }
        }

        private void MaxSizeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // If the text entered is an integer,
            if (int.TryParse(MaxSizeTextBox.Text, out int x))
            {
                MaxSizeTextBox.BorderBrush = neutral;
                size = x;
                NewGen();
            }
            // if the text entered is not an integer,
            else
            {
                MaxSizeTextBox.BorderBrush = error;
            }
        }

        private void SpecialCharsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            NewGen();
        }
        private void SpecialCharsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            NewGen();
        }
        private void CommonWordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            NewGen();
        }

        private void CommonWordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            NewGen();
        }

        private void ClipboardIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!String.IsNullOrEmpty((string?)GeneratedPassword.Content))
            {
                string s = (string)GeneratedPassword.Content;
                Iotapass.Services.Clipboard.Copy(s);
            }
        }

        private void RefreshIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NewGen();
        }

        // Generates a password based with the arguments set by the controls.
        private void NewGen()
        {
            if (ErrorMsg == null || GeneratedPassword == null)
            {
                return;
            }
            MaxSizeTextBox.BorderBrush = neutral;
            ErrorMsg.Visibility = Visibility.Hidden;
            bool specials = SpecialCharsCheckBox.IsChecked ?? false;
            bool words = CommonWordCheckBox.IsChecked ?? false;
            int size = 0;

            if (MaxSizeTextBox.Text == "")
            {
                size = 30;
                GeneratedPassword.Visibility = Visibility.Visible;
                // If we want to generate wordy passwords
                if (CommonWordCheckBox.IsChecked == true)
                {
                    GeneratedPassword.Content = PwdGenerator.GenerateWordyPassword(size, specials, random);
                }
                // If we don't want to generate wordy passwords
                else
                {
                    GeneratedPassword.Content = PwdGenerator.GeneratePassword(size, specials, random);
                }
            }
            else if (int.TryParse(MaxSizeTextBox.Text, out size))
            {
                if (size <= 4)
                {
                    MaxSizeTextBox.BorderBrush = error;
                    GeneratedPassword.Visibility = Visibility.Hidden;
                    ErrorMsg.Visibility = Visibility.Visible;
                    ErrorMsg.Text = "Please enter a vaild max size greater than 4, or nothing for no limit.";
                }
                else
                {
                    GeneratedPassword.Visibility = Visibility.Visible;
                    // If we want to generate wordy passwords
                    if (CommonWordCheckBox.IsChecked == true)
                    {
                        GeneratedPassword.Content = PwdGenerator.GenerateWordyPassword(size, specials, random);
                    }
                    // If we don't want to generate wordy passwords
                    else
                    {
                        GeneratedPassword.Content = PwdGenerator.GeneratePassword(size, specials, random);
                    }
                }
                
            }
            else
            {
                MaxSizeTextBox.BorderBrush = error;
                GeneratedPassword.Visibility = Visibility.Hidden;
                ErrorMsg.Visibility = Visibility.Visible;
                ErrorMsg.Text = "Please enter a vaild max size, or 0 for no limit.";
            }
        }
    }
}
