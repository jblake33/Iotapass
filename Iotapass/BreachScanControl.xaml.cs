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
using System.IO;
using Iotapass.Models;
using Iotapass.Services;
using System.ComponentModel;
using System.Globalization;
using System.Net;

namespace Iotapass
{
    /// <summary>
    /// Interaction logic for BreachScanControl.xaml
    /// </summary>
    public partial class BreachScanControl : UserControl
    {
        private List<CredentialModel> _credentialsForScan;
        internal List<CredentialModel> CredentialsForScan
        {
            get { return _credentialsForScan; }
            set { _credentialsForScan = value; }
        }
        private List<string> _resultMsgsToSave;
        internal List<string> ResultMsgsToSave
        {
            get { return _resultMsgsToSave; }
            set { _resultMsgsToSave = value; }
        }
        // using System.ComponentModel; this is to fill the progressbar as scan is performed
        private BackgroundWorker worker = new BackgroundWorker();
        // The count of how many passwords the user has have been found in breaches.
        int breachedCount;
        // Did the user lose internet connection? Prevent scan attempt with no internet:
        bool wasConnectionLostInScan;

        public BreachScanControl()
        {
            InitializeComponent();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            // events that get added to the background worker (for UI progress bar update)
            worker.ProgressChanged += ProgressChanged;
            worker.DoWork += DoWork;
            worker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            worker.CancelAsync();
            StatusMsg.Content = "Cancelling...";
        }

        private void RetryScanBtn_Click(object sender, RoutedEventArgs e)
        {
            var d = Window.GetWindow(App.Current.MainWindow) as MainWindow;
            if (d != null)
                d.PasswordScanStart();
        }
        /// <summary>
        /// Starts a scan of the user's credentials.
        /// </summary>
        internal void PerformScan()
        {
            StatusMsg.Content = "Scanning...";
            CancelBtn.Visibility = Visibility.Visible;
            SaveReportBtn.Visibility = Visibility.Collapsed;
            RetryScanBtn.Visibility = Visibility.Collapsed;
            PercentageMsg.Content = "0 %";
            breachedCount = 0;
            wasConnectionLostInScan = false;
            ResultMsgsToSave = new List<string>();
            try
            {
                worker.RunWorkerAsync();
            }
            catch (InvalidOperationException e)
            {
                IotapassMessageBox box = new IotapassMessageBox();
                box.Owner = Window.GetWindow(App.Current.MainWindow);
                box.Left = box.Owner.Left + (box.Owner.Width / 2) - box.Width / 2;
                box.Top = box.Owner.Top + (box.Owner.Height / 2) - box.Height / 2;
                box.Show("A scan is currently in progress. If your scan is stuck and you can not cancel it, please restart Iotapass.", "Scan attempt failed", 1, 4);
            }
        }
        /// <summary>
        /// Background worker's "DoWork" (scanning). Do not interact with the UI here. It should be on a separate thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < CredentialsForScan.Count; i++)
            {
                // Check internet connection
                if (CheckForInternetConnection())
                {
                    // pull result from API
                    int result = BreachScanController.FindOccurrences(IotaCryptService.GetSHA1Hash(CredentialsForScan[i].passwd));
                    // if result is greater than 0, set isBreached to 1 (true)
                    if (result > 0)
                    {
                        CredentialsForScan[i].isBreached = 1;
                        breachedCount++;
                        ResultMsgsToSave.Add("The password for your account \"" + CredentialsForScan[i].username + "\" on " + CredentialsForScan[i].website + " has been breached " + result + " time" + (result == 1 ? "" : "s") + "." + (wasConnectionLostInScan ? " The connection was lost sometime before this scan." : ""));
                    }
                }
                else { wasConnectionLostInScan = true; }
                worker.ReportProgress(i * (100/CredentialsForScan.Count));
            }
        }
        /// <summary>
        /// Update the progress bar. This is called on the UI thread when ReportProgress method is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ScanProgressBar.Value = e.ProgressPercentage;
        }
        /// <summary>
        /// This is called on the UI thread when the DoWork method completes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StatusMsg.Content = "Scan complete.";
            // What number of passwords need to be changed? Don't say what the passwords are.
            PercentageMsg.Content = "100 %\t " + breachedCount + " password" + (breachedCount == 1 ? " needs " : "s need ") + "to be changed." + (wasConnectionLostInScan ? " Internet connection was lost during scan." : "");
            ScanProgressBar.Value = 100;
            CancelBtn.Visibility = Visibility.Collapsed;
            SaveReportBtn.Visibility = Visibility.Visible;
            RetryScanBtn.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// Basic check to test internet connection before performing a search.
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool CheckForInternetConnection(int timeoutMs = 10000, string url = null)
        {
            try
            {
                // using System.Globalization
                url ??= CultureInfo.InstalledUICulture switch
                {
                    { Name: var n } when n.StartsWith("fa") => // Iran
                        "http://www.aparat.com",
                    { Name: var n } when n.StartsWith("zh") => // China
                        "http://www.baidu.com",
                    _ =>
                        "http://www.gstatic.com/generate_204",
                };
                // using System.Net
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.Timeout = timeoutMs;
                using (var response = (HttpWebResponse)request.GetResponse())
                    return true;
            }
            catch
            {
                return false;
            }
        }

        Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
        private void SaveReportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ResultMsgsToSave != null)
            {
                if (ResultMsgsToSave.Count > 0)
                {
                    OpenFileDialogAndSave();
                }
                else
                {
                    IotapassMessageBox box = new IotapassMessageBox();
                    box.Owner = Window.GetWindow(App.Current.MainWindow);
                    box.Left = box.Owner.Left + (box.Owner.Width / 2) - box.Width / 2;
                    box.Top = box.Owner.Top + (box.Owner.Height / 2) - box.Height / 2;
                    box.Show("None of your passwords were found in breaches. If you would like to create a report anyway, click Confirm.", "No issues to report", 2, 1);
                    if (box.MessageBoxResult == 1)
                    {
                        OpenFileDialogAndSave();
                    }
                }
            }
        }
        private void OpenFileDialogAndSave()
        {
            // SaveFileDialog is how the user selects where to save.
            saveFileDialog.FileName = "iotapass_scan_" + DateTime.Now.ToString("yyyyMMddss"); // Default file name
            saveFileDialog.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension
            saveFileDialog.DefaultExt = ".txt"; // Default file extension
            saveFileDialog.ValidateNames = true; // Only accept valid file names
            saveFileDialog.OverwritePrompt = true; // Ask to overwrite file if the file name already exists
            saveFileDialog.Title = "Save scan results"; // Title of the save file dialog
            Nullable<bool> result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                // note that FileName is the full file path and file name.
                WriteToFile(saveFileDialog.FileName);
            }
        }
        /// <summary>
        /// Asynchronously writes the BreachScanControl's list of result messages (from the scan) to the file with the file path parameter.
        /// </summary>
        /// <param name="filePath"></param>
        private async void WriteToFile(string filePath)
        {
            string message = "Iotapass - Scan for Breaches, performed " + DateTime.Now.ToString() + "\r\n____________________________________________________________\r\n";
            if (ResultMsgsToSave.Count == 0)
            {
                message += "None of your passwords were found in breaches.";
            }
            else
            {
                foreach (string msg in ResultMsgsToSave)
                {
                    message += msg + "\r\n";
                } 
            }
            
            //using System.IO;  We want to dispose of sw when done
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                await sw.WriteAsync(message);
            }
        }
    }
}
