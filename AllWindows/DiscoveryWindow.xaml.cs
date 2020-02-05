using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MSFT.AllWindows
{
    /// <summary>
    /// Logica di interazione per DiscoveryWindow.xaml
    /// </summary>
    public partial class DiscoveryWindow : Window
    {
        private MyServer server;
        private CancellationTokenSource cts;
        private string filePath;

        public DiscoveryWindow()
        {
            this.server = new MyServer();
            InitializeComponent();
            // not necessary to press the start button
            startButton_Click(null, null);
        }

        public DiscoveryWindow(string filePath) // Constructor when you don't need to open the file picker
        {
            this.filePath = filePath;
            this.server = new MyServer();
            InitializeComponent();
            infoLabel.Content = "Please select an host to which send the selected file:";
            sendButton.Content = "Send file";
            sendButton.Click -= sendButton_Click;
            sendButton.Click += sendButtonContextual_Click;
            startButton_Click(null, null); // HACK: Added so that it's not necessary to press the start button
        }

        private async void startButton_Click(object sender, RoutedEventArgs e)
        {
            startButton.IsEnabled = false;
            stopButton.IsEnabled = true;
            stopButton.IsDefault = true;
            this.cts = new CancellationTokenSource();
            var progressIndicator = new Progress<int>(ReportProgress);
            var reportIndicator = new Progress<MyEndpoint>(ReportAddition);
            clientsListView.Items.Clear();

            try
            {
                await ClientDiscoveryAsync(reportIndicator, progressIndicator, cts.Token);
            }
            catch (OperationCanceledException)
            {
                this.server.CleanClients(); // HACK: Resets the list of the available clients in the server object
            }
            stopButton.IsDefault = false;
            stopButton.IsEnabled = false;
            startButton.IsEnabled = true;
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            stopButton.IsDefault = false;
            stopButton.IsEnabled = false;
            startButton.IsEnabled = true;
            cts.Cancel();
            progressBar.Value = 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (stopButton.IsEnabled) // Check if there is something to interrupt
                stopButton_Click(this, new RoutedEventArgs()); // HACK: to dispose the processes that are executing
            this.server.DisposeClient();
        }

        private void ReportProgress(int value)
        {
            progressBar.Value = value;
        }

        private void ReportAddition(MyEndpoint client)
        {
            if (client != null && !clientsListView.Items.Contains(client))
                clientsListView.Items.Add(client);
        }

        private async Task ClientDiscoveryAsync(IProgress<MyEndpoint> reportIndicator, IProgress<int> progressIndicator, CancellationToken token)
        {
            // async put so that the exception is thrown to the caller
            await Task.Run(async () =>
            {
                for (int i = 0; i < 100; i++)
                {
                    reportIndicator.Report(this.server.ClientDiscovery());
                    Thread.Sleep(500);
                    token.ThrowIfCancellationRequested();
                    progressIndicator.Report(i + 1);
                }
            }, token);
        }

        private void clientsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (clientsListView.SelectedItem != null)
            {
                sendButton.IsEnabled = true;
            }
            else
            {
                sendButton.IsEnabled = false;
            }
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowser fb = new FolderBrowser();
            fb.Description = "Please select a file or a folder below:";
            fb.IncludeFiles = true;
            fb.NewStyle = false;
            //fb.InitialDirectory = @"C:\";
            if (fb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = fb.SelectedPath;
                for (int i = 0; i < clientsListView.SelectedItems.Count; i++)
                {
                    SendingWindow sendingFileWindow = new SendingWindow(this.server, (MyEndpoint)clientsListView.SelectedItems[i], filePath);
                }
                this.Close();
            }
        }

        private void sendButtonContextual_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < clientsListView.SelectedItems.Count; i++)
            {
                SendingWindow sendingFileWindow = new SendingWindow(this.server, (MyEndpoint)clientsListView.SelectedItems[i], filePath);
            }
            this.Close();
        }
    }
}
