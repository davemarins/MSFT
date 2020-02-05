using System.IO;
using System.IO.Pipes;

namespace MSFT
{
    class PipeClient
    {
        public static void Client(string arg)
        {
            var client = new NamedPipeClientStream("MSFTContextualMenuHandler");
            client.Connect();
            StreamWriter writer = new StreamWriter(client);
            writer.WriteLine(arg); // Every string is sent individually
            writer.Flush();
        }
    }
}
