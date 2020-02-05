using System; // Convert.ToInt32
using System.Collections.Generic; // List
using System.IO; // Directory
using System.IO.Compression; // ZipFile
using System.Net; // IPEndPoint
using System.Net.Sockets; // UdpClient
using System.Text; // Encoding ASCII

namespace MSFT
{
    public class MyServer
    {

        // no need to be private
        public const int Port = MyUtils.UDPPort;

        public const string Name = "MSFT";

        // this needs to be private
        private UdpClient Client;

        private List<MyEndpoint> AvailablePeople; // using to store discovered people

        public MyServer()
        {
            this.AvailablePeople = new List<MyEndpoint>();
            this.Client = new UdpClient(MyServer.Port);
            this.Client.EnableBroadcast = true; // otherwise it cannot be discoverable
        }

        // networking methods
        // UDP discovery format is "MSFT@<port>@<name>"
        // MSFT has been added in order not to capture other traffic not related

        public MyEndpoint ClientDiscovery()
        {
            if (this.Client.Available > 0)
            {
                IPEndPoint NewClient = new IPEndPoint(0, 0);
                var result = Encoding.ASCII.GetString(this.Client.Receive(ref NewClient));
                if (result.Contains(MyServer.Name))
                {
                    string[] announcement = result.Split('@');
                    NewClient.Port = Convert.ToInt32(announcement[1]);
                    MyEndpoint NamedNewClient = new MyEndpoint(announcement[2], NewClient);
                    if (this.AvailablePeople.Contains(NamedNewClient))
                    {
                        this.AvailablePeople.Add(NamedNewClient);
                        return NamedNewClient;
                    }
                }
            }
            return null;
        }

        public SingleFileTransfer StartSending(string FilePath, MyEndpoint WantedEndpoint)
        {
            SingleFileTransfer sft = new SingleFileTransfer();
            sft.HostName = WantedEndpoint.Name;
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(WantedEndpoint.Endpoint); // Connecting to the client specified (throws an exception if not available)

            // checking if it's a directory: if so, then compressing
            if (Directory.Exists(FilePath))
            {
                string tempPath = Path.GetTempPath() + new DirectoryInfo(FilePath).Name + ".zip";
                if (File.Exists(tempPath)) // Check if the file already exists so that ZipFile doesn't throw an exception
                    File.Delete(tempPath);
                ZipFile.CreateFromDirectory(FilePath, tempPath);
                FilePath = tempPath;
            }
            // New Stream opening
            FileInfo fi = new FileInfo(FilePath); // Obtaining the infos of the specified file
            sft.Name = fi.Name;
            sft.FileLength = fi.Length;
            sft.CurrentNetworkStream = tcpClient.GetStream();
            sft.CurrentNetworkStream.WriteTimeout = 20000; // seems good enough 20 seconds
            // Sending UserName, FileName and FileLength
            byte[] hostNameLengthBuffer = BitConverter.GetBytes(Encoding.Unicode.GetByteCount(Environment.UserName));
            sft.CurrentNetworkStream.Write(hostNameLengthBuffer, 0, hostNameLengthBuffer.Length);
            // Sending 
            byte[] hostNameBuffer = Encoding.Unicode.GetBytes(Environment.UserName);
            sft.CurrentNetworkStream.Write(hostNameBuffer, 0, hostNameBuffer.Length);
            // Sending
            byte[] fileNameLengthBuffer = BitConverter.GetBytes(Encoding.Unicode.GetByteCount(sft.Name));
            sft.CurrentNetworkStream.Write(fileNameLengthBuffer, 0, fileNameLengthBuffer.Length);
            // Sending 
            byte[] fileNameBuffer = Encoding.Unicode.GetBytes(sft.Name);
            sft.CurrentNetworkStream.Write(fileNameBuffer, 0, fileNameBuffer.Length);
            // Sending 
            byte[] fileLengthBuffer = BitConverter.GetBytes(sft.FileLength);
            sft.CurrentNetworkStream.Write(fileLengthBuffer, 0, fileLengthBuffer.Length);
            // Opening FileStream
            sft.CurrentFileStream = File.OpenRead(FilePath);
            return sft;
        }

        public void Sending(SingleFileTransfer sft)
        {
            var buffer = new byte[256 * 1024]; // good enough
            int bytesRead = sft.CurrentFileStream.Read(buffer, 0, buffer.Length);
            if (bytesRead > 0 && sft.FileLength > 0)
            {
                sft.CurrentNetworkStream.Write(buffer, 0, bytesRead);
                sft.FileLength -= bytesRead;
            }
        }

        // stopping gracefully
        public void StopSending(SingleFileTransfer sft)
        {
            sft.CurrentNetworkStream.Flush();
            sft.CurrentNetworkStream.Dispose();
            sft.CurrentFileStream.Dispose();
        }

        // stopping gracefully
        public void CancelSending(SingleFileTransfer sft)
        {
            sft.CurrentNetworkStream.Dispose();
            sft.CurrentFileStream.Dispose();
        }

        // utils methods

        public void DisposeClient()
        {
            this.Client.Dispose();
        }

        public void CleanClients()
        {
            this.AvailablePeople.Clear();
        }

    }
}