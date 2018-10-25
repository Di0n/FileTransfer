using Newtonsoft.Json;
using Shared;
using Shared.Packets;
using System;
using System.Collections.Generic;
using System.IO;
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
            MessageState state = new MessageState(client);

            client.Socket.BeginReceive(state.Buffer, 0, MessageState.BufferSize, 0, ReceiveCallback, state);
        }

        private void ReceiveMessageCallback(IAsyncResult ar)
        {
            MessageState state = (MessageState)ar.AsyncState;
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
                        HandlePacket(jsonData, state);
                        return;
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
            handler.BeginReceive(state.Buffer, 0, MessageState.BufferSize, 0, ReceiveCallback, state);
        }
        
        private void ReceiveFileCallback(IAsyncResult ar)
        {

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

          /*  if (state is MessageState)
                HandleMessage(state as MessageState, read);
            else if (state is FileStateObject)
                HandleFile(state as FileStateObject, read);*/
            
            /*if (dataType == 0)
                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
            else if (dataType == 1)
                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveFileCallback, state);*/
        }

        private void HandleFile(FileStateObject state, int read)
        {

        }


        /// <summary>
        /// Bekijkt wat voor soort packet er is gestuurd.
        /// </summary>
        /// <param name="jsonData"></param>
        /// <param name="socket"></param>
        private void HandlePacket(dynamic jsonData, StateObject state)
        {
            string packetType = (string)jsonData.packetType;
            switch (packetType)
            {
                case nameof(FileInfoRequest):
                    HandleFileInfoRequest(FileInfoRequest.ToClass(jsonData), state);
                    break;
                case nameof(FileDownloadRequest):
                    HandleFileDownloadRequest(FileDownloadRequest.ToClass(jsonData), state);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handelt een bestand info aanvraag af.
        /// </summary>
        /// <param name="request"></param>
        private void HandleFileInfoRequest(FileInfoRequest request, StateObject state)
        {
            
        }

        /// <summary>
        /// Handelt een bestand download af.
        /// </summary>
        /// <param name="request"></param>
        private void HandleFileDownloadRequest(FileDownloadRequest request, StateObject state)
        {
            
        }

        private void HandleFileUploadRequest(FileUploadRequest request, StateObject state)
        {
            FileStateObject fileState = new FileStateObject(state.Client, request.FileName, request.FileSize);
            state.Client.Socket.BeginReceive(state.Buffer, 0, FileStateObject.BufferSize, 0, ReceiveCallback, fileState);
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

        private abstract class StateObject
        {
            public Client Client { get; private set; }
            public static int BufferSize { get { return 1024; } }
            public byte[] Buffer { get; private set; }
            public StateObject(Client client)
            {
                this.Client = client;
                this.Buffer = new byte[BufferSize];
            }
        }

        private class MessageState : StateObject
        { 
            public byte[] Data { get; set; }

            public MessageState(Client client):base(client)
            {
                Data = new byte[0];
            }
        }

        private class FileStateObject : StateObject
        {
            public new static int BufferSize { get { return 4096; } }
            public FileStateObject Output { get; set; }
            public string FileName { get; private set; }
            public long BytesToReceive { get; private set; }
            public FileStateObject(Client client, string fileName, long bytesToReceive):base(client)
            {
                FileName = fileName;
                BytesToReceive = bytesToReceive;
            }
        }


        /*private class StateObject
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
        }*/
    }
}
