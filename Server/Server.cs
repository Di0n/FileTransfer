using Newtonsoft.Json;
using Shared;
using Shared.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Update = System.Timers.Timer;
namespace Server
{
    class Server
    {
        static void Main(string[] args)
        {
            new Server().Start();
        }
        /// <summary>
        /// De server socket.
        /// </summary>
        private Socket listener;
        /// <summary>
        /// List met verbonden clients en de laatste interactie tijd.
        /// </summary>
        private readonly List<Client> connectedClients = new List<Client>();
        /// <summary>
        /// Update voor server taken.
        /// </summary>
        private readonly Update updater = new Update(5000);
        /// <summary>
        /// Reset event voor client connecties aannemen.
        /// </summary>
        private readonly ManualResetEvent resetEvent = new ManualResetEvent(false);
        private Server()
        {
            updater.AutoReset = true;
            updater.Elapsed += UpdateElapsed;
        }

        
        /// <summary>
        /// Start de server.
        /// </summary>
        private void Start()
        {
            using (listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
#if USE_LOCAL
                listener.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 33000));
#else
                listener.Bind(new IPEndPoint(IPAddress.Any, 33000));
#endif

                listener.Listen(25);
                Console.WriteLine($"=========================\n" +
                    $"Server started: {listener.LocalEndPoint}\n{DateTime.Now}" +
                    $"\n=========================");

                updater.Start();

                while (true)
                {
                    resetEvent.Reset();
                    listener.BeginAccept(ConnectCallback, listener);
                    resetEvent.WaitOne();
                }
            }
        }

        /// <summary>
        /// Callback wordt aangeroepen als een client probeert te verbinden.
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;

            Client client = listener.EndAccept(ar);

#if DEBUG
            Console.WriteLine($"Client {client.Socket.RemoteEndPoint} connected to the server.");
#endif
            StateObject state = new StateObject()
            {
                Client = client
            };

            client.Socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
        }

        /// <summary>
        /// Wordt aangeroepen wanneer er data binnenkomt.
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;

            Socket handler = state.Client.Socket;

            int read = handler.EndReceive(ar);
            state.Data = state.Data.Combine(state.Buffer, read);

            while (state.Data.Length >= sizeof(Int32))
            {
                //state.Data = state.Data.Combine(state.Buffer, read);

                int msgSize = BitConverter.ToInt32(state.Data, 0);

                int totalPacketSize = msgSize + sizeof(Int32);
                if (state.Data.Length >= totalPacketSize)
                {
                    string data = Encoding.UTF8.GetString(state.Data, sizeof(Int32), msgSize);
#if DEBUG
                    Console.WriteLine($"Received {data} from {handler.RemoteEndPoint}");
#endif
                    dynamic jsonData = null;
                    try
                    {
                        jsonData = JsonConvert.DeserializeObject(data);
                        HandlePacket(jsonData, handler);
                    }
                    catch (JsonReaderException)
                    {
                        Console.WriteLine("Got a malformed packet {0}", data);
                    }

                    byte[] bytes = new byte[state.Data.Length - totalPacketSize];
                    Array.Copy(state.Data, totalPacketSize, bytes, 0, state.Data.Length - totalPacketSize);

                    state.Data = bytes;
                }
                else break;
            }
            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
        }

        private void ReceiveFileCallback()
        {
            
        }

        /// <summary>
        /// Bekijkt wat voor soort packet er is gestuurd.
        /// </summary>
        /// <param name="jsonData"></param>
        /// <param name="socket"></param>
        private void HandlePacket(dynamic jsonData, Socket socket)
        {
            string packetType = (string)jsonData.packetType;
            switch (packetType)
            {
                case nameof(FileInfoRequest):
                    HandleFileInfoRequest(FileInfoRequest.ToClass(jsonData));
                    break;
                case nameof(FileDownloadRequest):
                    HandleFileDownloadRequest(FileDownloadRequest.ToClass(jsonData));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handelt een bestand info aanvraag af.
        /// </summary>
        /// <param name="request"></param>
        private void HandleFileInfoRequest(FileInfoRequest request)
        {

        }

        /// <summary>
        /// Handelt een bestand download af.
        /// </summary>
        /// <param name="request"></param>
        private void HandleFileDownloadRequest(FileDownloadRequest request)
        {

        }

        /// <summary>
        /// Update voor server taken.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateElapsed(object sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private class StateObject
        {
            public Client Client { get; set; }
            public static int BufferSize { get { return 1024; } }
            public byte[] Buffer { get; private set; }
            public byte[] Data { get; set; }
            public StateObject()
            {
                Buffer = new byte[BufferSize];
                Data = new byte[0];
            }
        }
    }
}
