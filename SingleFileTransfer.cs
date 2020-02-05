using System.IO; // FileStream
using System.Net.Sockets; // NetworkStream

namespace MSFT
{
    public class SingleFileTransfer
    {
        public string HostName { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public long FileLength { get; set; }

        public NetworkStream CurrentNetworkStream { get; set; }

        public FileStream CurrentFileStream { get; set; }
    }
}