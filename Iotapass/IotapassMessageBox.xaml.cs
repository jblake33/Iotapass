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
using Iotapass.Properties;

namespace Iotapass
{
    /// <summary>
    /// Interaction logic for IotapassMessageBox.xaml
    /// </summary>
    public partial class IotapassMessageBox : Window
    {
        private int _messageBoxResult = 0;
        /// <summary>
        /// 0 is cancel, 1 is confirm. 2 is "confirm and don't show again"
        /// </summary>
        internal int MessageBoxResult
        {
            get { return _messageBoxResult; }
            set { _messageBoxResult = value; }
        }
        /// <summary>
        /// Default constructor. Please 1) create object, 2) set this object's Owner to be the window you call it from, 3) call the show method, which returns a int for the dialog result.
        /// </summary>
        public IotapassMessageBox()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Displays a default message box with this message. Only option is OK.
        /// </summary>
        /// <returns>0 is cancel, 1 is confirm. 2 is "confirm and don't show again"</returns>
        internal int Show(string message)
        {
            Message.Text = message;
            this.ShowDialog();
            return MessageBoxResult;
        }
        /// <summary>
        /// Displays a message box, also with a caption (title of window).
        /// </summary>
        /// <param name="message"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        internal int Show(string message, string caption)
        {
            Message.Text = message;
            Caption.Content = caption;
            this.Title = caption;
            this.ShowDialog();
            return MessageBoxResult;
        }
        /// <summary>
        /// Displays a message box, also with set buttons. 1 = OK, 2 = ConfirmCancel, 3 = ConfirmCancel&DontShowAgain, 4 = YesNo
        /// </summary>
        /// <param name="message"></param>
        /// <param name="caption"></param>
        /// <param name="buttonsGroupID"></param>
        /// <returns></returns>
        internal int Show(string message, string caption, int buttonsGroupID)
        {
            Message.Text = message;
            Caption.Content = caption;
            SetBtnVis(buttonsGroupID);
            this.Title = caption;
            this.ShowDialog();
            return MessageBoxResult;
        }
        /// <summary>
        /// Displays a message box, also with set icon. 1 = Info, 2 = QuestionMark, 3 = WarningTriangle, 4 = ErrorX
        /// </summary>
        /// <param name="message"></param>
        /// <param name="caption"></param>
        /// <param name="buttonsGroupID"></param>
        /// <param name="iconID"></param>
        /// <returns></returns>
        internal int Show(string message, string caption, int buttonsGroupID, int iconID)
        {
            Message.Text = message;
            Caption.Content = caption;
            SetBtnVis(buttonsGroupID);
            SetIconVis(iconID);
            this.Title = caption;
            this.ShowDialog();
            return MessageBoxResult;
        }
        /// <summary>
        /// 1 = OK, 2 = Confirm/Cancel, 3 = Confirm/Cancel/Dontshowagain
        /// </summary>
        /// <param name="buttonsGroupID"></param>
        private void SetBtnVis(int buttonsGroupID)
        {
            switch (buttonsGroupID)
            {
                case 1:
                    break;
                case 2:
                    OKbtn.Visibility = Visibility.Collapsed;
                    Cancelbtn.Visibility = Visibility.Visible;
                    Confirmbtn.Visibility = Visibility.Visible;
                    break;
                case 3:
                    OKbtn.Visibility = Visibility.Collapsed;
                    Cancelbtn.Visibility = Visibility.Visible;
                    Confirmbtn.Visibility = Visibility.Visible;
                    DontShowAgainCheckBox.Visibility = Visibility.Visible;
                    break;
                case 4:
                    OKbtn.Visibility = Visibility.Collapsed;
                    Cancelbtn.Visibility = Visibility.Visible;
                    Cancelbtn.Content = "No";
                    Confirmbtn.Visibility = Visibility.Visible;
                    Confirmbtn.Content = "Yes";
                    break;
            }
        }
        /// <summary>
        ///  1 = Info, 2 = QuestionMark, 3 = WarningTriangle, 4 = ErrorX
        /// </summary>
        /// <param name="iconsID"></param>
        private void SetIconVis(int iconsID)
        {
            switch (iconsID)
            {
                case 1:
                    InfoIcon.Visibility = Visibility.Visible;
                    break;
                case 2:
                    QuestionIcon.Visibility = Visibility.Visible;
                    break;
                case 3:
                    WarningIcon.Visibility = Visibility.Visible;
                    break;
                case 4:
                    ErrorIcon.Visibility = Visibility.Visible;
                    break;
            }
        }
        /// <summary>
        /// When the message box is closed by pressing the X.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XClose_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult = 0;
            this.Close();
        }

        private void OKbtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = 0;
            this.Close();
        }

        private void Cancelbtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = 0;
            this.Close();
        }

        private void Confirmbtn_Click(object sender, RoutedEventArgs e)
        {
            if (DontShowAgainCheckBox.IsChecked == true)
            {
                MessageBoxResult = 2;
            }
            else
            {
                MessageBoxResult = 1;
            }
            this.Close(); 
        }
    }
}
