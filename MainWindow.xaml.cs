using MSFT.AllWindows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MSFT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MyClient client;
        private CancellationTokenSource cts;
        public System.Windows.Forms.NotifyIcon Ni { get; set; }
        private DiscoveryWindow cdw;


        public MainWindow()
        {
            CheckInstance();
            this.client = new MyClient();
            ConfigureTrayIcon();
            ActivateDiscoveryMode();
            InitializeComponent();
        }


        private void ConfigureTrayIcon()
        {
            Ni = new System.Windows.Forms.NotifyIcon();
            Ni.Icon = Properties.Resources.Icon;
            Ni.Visible = true;
            Ni.Click += ShowMSFTClick;
            Ni.ContextMenu = new System.Windows.Forms.ContextMenu();
            Ni.ContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Exit MSFT", ExitMSFTClick));
        }


        private void ExitMSFTClick(object sender, EventArgs e)
        {
            Ni.Dispose();
            Application.Current.Shutdown();
        }


        private void ShowMSFTClick(object sender, EventArgs e) => this.Show();


        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true; // setting cancel to true will cancel the close request so that the application is not closed
            this.Hide();
            base.OnClosing(e);
        }


        private void CheckInstance()
        {
            // checking if MSFT is already running
            if (Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
            {
                string[] args = Environment.GetCommandLineArgs();
                if (args.Count() > 1)
                {
                    // if launched from contextual menu
                    // sending data to the already running instance
                    // I don't want any ambiguous behavior
                    PipeClient.Client(args[1]);
                }
                else
                {
                    // if launched from .exe
                    MessageBox.Show("MSFT is already running.", "MSFT", MessageBoxButton.OK,
                        MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                // exiting the current process
                Application.Current.Shutdown();
            }
            else
            {
                // if there's no any running instance of MSFT
                string[] args = Environment.GetCommandLineArgs();
                if (args.Count() > 1) // if launched from contextual menu (that means that I will have some args passed to the .exe)
                {
                    InstantiateDiscoveryWindow(args[1]); // passing the path (2nd argument)
                }
                // listening for other instances of MSFT launched
                ListenInstancesAsync();
            }
        }


        private async void ListenInstancesAsync()
        {
            await Task.Run(async () => // async put so that the exception is thrown to the caller
            {
                do
                {
                    PipeServer ipcServer = new PipeServer();
                    string filePath = ipcServer.Server(); // The thread blocks here until an IPC Client connects to the server
                    // Operations pertaining the UI
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        InstantiateDiscoveryWindow(filePath);
                    }));

                } while (true);
            });

        }


        private void SendFileOrFolderButton(object sender, RoutedEventArgs e)
        {
            InstantiateDiscoveryWindow();
        }

        private void InstantiateDiscoveryWindow()
        {
            // first time cdw is null
            if ((cdw == null) || (cdw.IsLoaded == false))
            {
                cdw = new DiscoveryWindow();
                cdw.Show();
            }
            else
            {
                cdw.Activate();
            }
        }


        private void InstantiateDiscoveryWindow(string filePath)
        {
            // first time cdw is null
            // from second condition will not be evaluated
            if ((cdw == null) || (cdw.IsLoaded == false))
            {
                cdw = new DiscoveryWindow(filePath);
                cdw.Show();
            }
            else
            {
                // if entering here, then I'm trying to open 2 DiscoveryWindow (2 servers)
                MessageBox.Show("To start sending another file, close the open MSFT Windows.", "MSFT", MessageBoxButton.OK,
                    MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private async void ActivateDiscoveryMode()
        {
            this.cts = new CancellationTokenSource();
            var reportIndicator = new Progress<TcpClient>(ReportNewFile);
            try
            {
                await Task.WhenAll(AnnounceAsync(this.cts.Token), ListenRequestsAsync(reportIndicator));
            }
            catch (SocketException)
            {
                Debug.WriteLine("The AcceptTcpClient was effectively blocked");
            }
        }

        private async void DiscoverableModeCheckbox(object sender, RoutedEventArgs e)
        {
            ActivateDiscoveryMode();
        }


        private void HiddenModeChekbox(object sender, RoutedEventArgs e) => this.cts.Cancel();


        private void ReportNewFile(TcpClient tcpClient) => new ReceivingWindow(this, this.client, tcpClient);


        private async Task AnnounceAsync(CancellationToken token)
        {
            // async so that the exception is thrown to the caller
            await Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        this.client.Announce();
                    }
                    catch (SocketException)
                    {
                        // if entering here, then there is no network connection or something else
                        MessageBox.Show("Probably there is no network connection. MSFT will be closed.", "MSFT", MessageBoxButton.OK,
                                 MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                        await Application.Current.Dispatcher.BeginInvoke(new Action(() => // GUI thread
                        {
                            Application.Current.Shutdown();
                        }));
                    }
                    Thread.Sleep(1000);
                    // if private mode, then stop listening for connections
                    if (token.IsCancellationRequested)
                    {
                        this.client.StopListening();
                    }
                    token.ThrowIfCancellationRequested();
                }
            }, token);
        }


        private async Task ListenRequestsAsync(IProgress<TcpClient> reportIndicator)
        {
            await Task.Run(async () => // async put so that the exception is thrown to the caller
            {
                this.client.StartListening(); // now listening to traffic on the network...
                while (true)
                {
                    reportIndicator.Report(this.client.ListenRequests());
                }
            });
        }


        private void ButtonSettings(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(this);
            settingsWindow.Show();
            settingsWindow.Activate();
            this.Hide();
        }
    }
}

