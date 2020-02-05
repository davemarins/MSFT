using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace MSFT.AllWindows
{
    /// <summary>
    /// Logica di interazione per ReceivingWindow.xaml
    /// </summary>
    public partial class ReceivingWindow : Window
    {
        private MyClient client;
        private TcpClient tcpClient;
        private SingleFileTransfer sft;
        private long originalLength;
        private long timestamp;
        private int updateEstimation;
        private double oldValue;

        private CancellationTokenSource cts;

        // For hiding the close button
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);


        public ReceivingWindow(MainWindow mainWindow, MyClient client, TcpClient tcpClient)
        {
            this.client = client;
            this.tcpClient = tcpClient;
            this.sft = client.StartReceiving(tcpClient);
            InitializeComponent();

            // if auto-accepting files is enabled
            if (Properties.Settings.Default.AutoAccept)
            {
                mainWindow.Ni.BalloonTipTitle = "MSFT";
                mainWindow.Ni.BalloonTipText = "Receiving file " + this.sft.Name + " from " + this.sft.HostName;
                mainWindow.Ni.ShowBalloonTip(3000);
                this.Show();
                this.Activate();
                Yes_Button_Click(null, null);
            }
            else
            {
                fileInfo.Text = "Do you want to receive file " + this.sft.Name + " from " + this.sft.HostName + '?';
                this.Show();
                this.Activate();
            }
        }


        private void ReceivingFileWindowLoaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }


        private async void Yes_Button_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Visibility = Visibility.Visible;
            cancelButton.Visibility = Visibility.Visible;
            yesButton.Visibility = Visibility.Hidden;
            noButton.Visibility = Visibility.Hidden;
            fileInfo.Text = "Receiving file " + this.sft.Name + " from " + this.sft.HostName + "...";

            if (!Properties.Settings.Default.SetPath) // if the path must be chosen each time the user receives a file
            {
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dialog.Description = "Please choose the folder in which save the file.";
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    if (dialog.SelectedPath != "")
                    {
                        // start transfering
                        client.Path = dialog.SelectedPath;
                        this.sft.Path = dialog.SelectedPath + "//" + this.sft.Name;
                    }
                    else
                    {
                        // cancel the transfer
                        this.Close();
                        return;
                    }
                    Debug.WriteLine(dialog.SelectedPath + " chosen for receiving file.");
                }
            }

            // adding cast from bool? to bool, hoping it doesn't break
            if (File.Exists(this.sft.Path) && (bool)!Properties.Settings.Default.AutoReplace) // if autoreplace is disabled
            {
                if (MessageBox.Show(this.sft.Name + " already exists. Do you want to replace it?", "MSFT",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                { // no substitution
                    string appendedTimestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string oldName = this.sft.Name;
                    string oldNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(oldName);
                    this.sft.Name = oldName.Replace(oldNameWithoutExtension, oldNameWithoutExtension + appendedTimestamp);
                    this.sft.Path = this.sft.Path.Replace(oldName, this.sft.Name);

                    Debug.WriteLine("Name: " + this.sft.Name + " Path: " + this.sft.Path);
                }
            }

            this.sft.CurrentFileStream = File.Create(this.sft.Path);
            originalLength = this.sft.FileLength;

            cts = new CancellationTokenSource();
            var progressIndicator = new Progress<double>(ReportProgress);

            try
            {
                await ReceiveFileAsync(progressIndicator, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Cancellation requested!");
            }

            this.Close();
        }


        private void No_Button_Click(object sender, RoutedEventArgs e)
        {
            client.CancelReceiving(this.sft);
            tcpClient.Dispose();
            this.Close();
        }


        private void Cancel_Button_Click(object sender, RoutedEventArgs e) => cts.Cancel();


        private void ReportProgress(double value)
        {
            if (updateEstimation == 0)
            {
                double diffValue = value - oldValue;
                oldValue = value;

                long nowTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); // if i took diff seconds for transferring the diffValue, then I can perform an estimation of the remaining time
                long diffTime = nowTime - timestamp;
                timestamp = nowTime;

                TimeSpan remainingTime = TimeSpan.FromMilliseconds(Math.Ceiling((diffTime * (100 - value)) / diffValue));
                if (remainingTime.TotalSeconds > 0)
                    remainingTimeBlock.Text = "Remaining time: " + remainingTime.Minutes + " minutes and " + remainingTime.Seconds + " seconds";
            }
            updateEstimation = (updateEstimation + 1) % 15; // this is done for preventing inconsistent updates of the estimated time

            progressBar.Value = value;
        }

        private async Task ReceiveFileAsync(IProgress<double> progressIndicator, CancellationToken token)
        {
            await Task.Run(async () => // async put so that the exception is thrown to the caller
            {
                try
                {
                    token.ThrowIfCancellationRequested();

                    updateEstimation = 1;
                    oldValue = 0;
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); // initializing the timestamp at the beginning of the transfer

                    while (this.sft.FileLength > 0)
                    {
                        client.Receive(this.sft);
                        token.ThrowIfCancellationRequested();

                        //Thread.Sleep(500); // HACK: Waiting for testing purposes

                        progressIndicator.Report(100 - ((float)this.sft.FileLength / originalLength * 100));
                    }

                    client.EndReceiving(this.sft);
                }
                catch (OperationCanceledException)
                {
                    client.CancelReceiving(this.sft);
                    throw;
                }
                catch (Exception e)
                {
                    if (e is SocketException || e is IOException)
                    {
                        client.CancelReceiving(this.sft);
                        MessageBox.Show("There was an error in receiving the file.", "Mthis.sft", MessageBoxButton.OK,
                            MessageBoxImage.Stop, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                        throw new OperationCanceledException();
                    }

                    throw;
                }
            }, token);
        }
    }
}
