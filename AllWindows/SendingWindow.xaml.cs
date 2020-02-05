using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace MSFT.AllWindows
{
    /// <summary>
    /// Logica di interazione per SendingWindow.xaml
    /// </summary>
    public partial class SendingWindow : Window
    {
        private MyServer server;
        private MyEndpoint SelectedClient;
        private string FilePath;
        private long originalLength;
        private SingleFileTransfer sft;

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


        public SendingWindow(MyServer server, MyEndpoint SelectedClient, string FilePath)
        {
            this.FilePath = FilePath;
            this.server = server;
            this.SelectedClient = SelectedClient;
            InitializeComponent();
            StartSending();
        }


        private void SendingFileWindowLoaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
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

        private async void StartSending()
        {
            cts = new CancellationTokenSource();
            var progressIndicator = new Progress<double>(ReportProgress);
            try
            {
                this.sft = this.server.StartSending(this.FilePath, this.SelectedClient);
            }
            catch (SocketException)
            {
                MessageBox.Show("The selected host is unavailable.", "Transfar", MessageBoxButton.OK,
                    MessageBoxImage.Stop, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                this.Close();
                return;
            }
            this.originalLength = this.sft.FileLength;

            // If there are no issues with the transfer I can show the window
            fileInfo.Text = "Sending file " + this.sft.Name + " to " + this.sft.HostName + "...";
            this.Show();
            this.Activate();
            try
            {
                await SendFileAsync(progressIndicator, cts.Token);
            }
            catch (Exception)
            {
                Console.WriteLine("Cancellation requested!");
            }
            this.Close();
        }


        private async Task SendFileAsync(IProgress<double> progressIndicator, CancellationToken token)
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
                        try
                        {
                            this.server.Sending(this.sft);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("The selected host is unavailable.", "Transfar", MessageBoxButton.OK,
                                MessageBoxImage.Stop, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                            this.server.CancelSending(this.sft);
                            return;
                        }
                        token.ThrowIfCancellationRequested();
                        //HACK: waiting for testing purposes //Thread.Sleep(100);
                        progressIndicator.Report(100 - ((float)this.sft.FileLength / this.originalLength * 100));
                    }
                    this.server.StopSending(this.sft);
                }
                catch (OperationCanceledException)
                {
                    this.server.CancelSending(this.sft);
                    throw;
                }
            }, token);
        }
    }
}
