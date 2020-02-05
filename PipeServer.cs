using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;

namespace MSFT
{
    class PipeServer
    {
        private NamedPipeServerStream server;

        public PipeServer()
        {
            PipeSecurity pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.ReadWrite, AccessControlType.Allow));
            server = new NamedPipeServerStream("MSFTContextualMenuHandler", PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Message, PipeOptions.None, 512, 512, pipeSecurity);
        }


        // N.B. First instance of MSFT executes this function, otherwise no one will wait for connections
        public string Server()
        {
            server.WaitForConnection();
            StreamReader reader = new StreamReader(server);
            string received = reader.ReadLine(); // file path is the first thing received
            server.Close();
            return received;
        }
    }
}
