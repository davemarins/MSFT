using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MSFT
{
    public class MyClient
    {

        // N.B. No multicast has been used, issues with modem-router :(
        // Choice: 2019 UDP 2020 TCP

        private Byte[] announcementBytes;
        private UdpClient udpClient;
        private TcpListener tcpListener;
        private IPEndPoint broadcastEndpoint;

        private const string tfString = "MSFT";

        public String Path { get; set; }

        public MyClient()
        {
            Path = Properties.Settings.Default.Path;
            // default path case encoding .config file as empty string (WTF?!)
            if (Path == "")
            {
                Path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads\\MSFT";
            }

            this.udpClient = new UdpClient();
            announcementBytes = Encoding.ASCII.GetBytes(tfString + "@" + Environment.UserName + "@" + MyUtils.TCPPort);

            this.udpClient.EnableBroadcast = true; // No multicast unfortunatelly
            IPAddress localAddress = MyUtils.GetLocalIPAddress();
            IPAddress localSubnetMask = MyUtils.GetSubnetMask(localAddress);

            broadcastEndpoint = new IPEndPoint(MyUtils.GetBroadcastAddress(localAddress, localSubnetMask), MyUtils.UDPPort);
            //multicastEndpoint = new IPEndPoint(IPAddress.Parse(multicastAddress), MyUtils.UDPPort); // No multicast unfortunatelly
        }


        public void Dispose()
        {
            this.udpClient.Dispose();
            this.tcpListener.Stop();
        }

        // Starting listening of connections from hosts that want to send a file
        public void StartListening()
        {
            if (this.tcpListener == null) // WARNING Singleton: MUST only be initialized once
            {
                this.tcpListener = new TcpListener(IPAddress.Any, MyUtils.TCPPort);
            }

            this.tcpListener.Start();
        }

        // Stopping the listening of connections.
        public void StopListening()
        {
            if (this.tcpListener != null)
            {
                this.tcpListener.Stop();
            }
        }

        // Sending a broadcast packet for discovery (must be put inside a loop for keeping the discovery active).
        public void Announce()
        {
            this.udpClient.Send(announcementBytes, announcementBytes.Length, broadcastEndpoint);
        }

        // Listening for file transfer requests
        public TcpClient ListenRequests()
        {
            TcpClient client = this.tcpListener.AcceptTcpClient(); // Blocking until a new request is received
            return client;
        }

        // Starting receiving the first metadata of the file (host name, file name and file size).
        public SingleFileTransfer StartReceiving(TcpClient client)
        {
            SingleFileTransfer sft = new SingleFileTransfer();
            NetworkStream netStream = client.GetStream();

            // Pattern of receiving as MyServer:
            // Receiving UserName, FileName and FileLength
            byte[] hostNameLengthBuffer = new byte[sizeof(int)];
            netStream.Read(hostNameLengthBuffer, 0, hostNameLengthBuffer.Length);
            int hostNameLength = BitConverter.ToInt32(hostNameLengthBuffer, 0);
            // Receiving
            byte[] hostNameBuffer = new byte[hostNameLength];
            netStream.Read(hostNameBuffer, 0, hostNameBuffer.Length);
            // Receiving
            byte[] fileNameLengthBuffer = new byte[sizeof(int)];
            netStream.Read(fileNameLengthBuffer, 0, fileNameLengthBuffer.Length);
            int fileNameLength = BitConverter.ToInt32(fileNameLengthBuffer, 0);
            // Receiving
            byte[] fileNameBuffer = new byte[fileNameLength];
            netStream.Read(fileNameBuffer, 0, fileNameBuffer.Length);
            // Receiving
            byte[] fileLengthBuffer = new byte[sizeof(long)];
            netStream.Read(fileLengthBuffer, 0, fileLengthBuffer.Length);

            string hostName = Encoding.Unicode.GetString(hostNameBuffer);
            string fileName = Encoding.Unicode.GetString(fileNameBuffer);
            long fileLength = BitConverter.ToInt64(fileLengthBuffer, 0);

            sft.HostName = hostName;
            sft.Name = fileName;
            sft.Path = Path + "//" + fileName;
            sft.FileLength = fileLength;
            sft.CurrentNetworkStream = netStream;
            sft.CurrentFileStream = null; // N.B. management is done on the GUI
            return sft;
        }

        // Receiving a chunk of data (to be in a while loop)
        public void Receive(SingleFileTransfer sft)
        {
            var buffer = new byte[256 * 1024]; // my default size of buffer to receive
            int bytesRead = sft.CurrentNetworkStream.Read(buffer, 0, buffer.Length);
            if (bytesRead > 0 && sft.FileLength > 0)
            {
                sft.CurrentFileStream.Write(buffer, 0, bytesRead);
                sft.FileLength -= bytesRead;
            }
            else
                throw new SocketException(1);
        }

        // Ending gracefully the file reception.
        public void EndReceiving(SingleFileTransfer sft)
        {
            sft.CurrentFileStream.Flush();
            sft.CurrentNetworkStream.Dispose();
            sft.CurrentFileStream.Dispose();
        }

        // Stopping the file reception and deletes the file stored. 
        public void CancelReceiving(SingleFileTransfer sft)
        {
            sft.CurrentNetworkStream.Dispose();
            sft.CurrentFileStream?.Dispose();
            File.Delete(sft.Path);
        }

    }
}
