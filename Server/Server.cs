using Newtonsoft.Json;
using Server.Properties;
using Server.Util;
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
            Directory.CreateDirectory(Settings.Default.FileFolder);
            using (listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
#if USE_LOCAL
                listener.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 33000));
#else
                listener.Bind(new IPEndPoint(IPAddress.Any, Settings.Default.ServerPort));
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

            resetEvent.Set();

            lock (connectedClients)
                connectedClients.Add(client);
#if DEBUG
            Console.WriteLine($"Client {client.Socket.RemoteEndPoint} connected to the server.");
#endif
            MessageState state = new MessageState(client);


            client.Socket.BeginReceive(state.Buffer, 0, MessageState.BufferSize, 0, ReceiveMessageCallback, state);
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
            FileStateObject state = (FileStateObject)ar.AsyncState;

            Socket handler = state.Client.Socket;

            int read = handler.EndReceive(ar);

            if (read > 0)
            {
                state.Output = state.Output ?? new Func<FileStream>(() =>
                {
                    string id = RandomIDGenerator.GetBase62(5);
                    state.ID = id;
                    string fileName = Settings.Default.FileFolder + id;

                    return new FileStream(fileName, FileMode.CreateNew, FileAccess.Write, FileShare.None, Settings.Default.FileWriteBufferSize, true);
                })();
                state.Output.BeginWrite(state.Buffer, 0, read, WriteFileCallback, state);
            }
            else
            {
                throw new Exception("Read 0"); // DEBUG
            }
        }

        /// <summary>
        /// Callback voor download schrijven naar bestand.
        /// </summary>
        /// <param name="ar"></param>
        private void WriteFileCallback(IAsyncResult ar)
        {
            FileStateObject state = (FileStateObject)ar.AsyncState;
            state.Output.EndWrite(ar);

            Socket handler = state.Client.Socket;

            if (state.BytesToReceive == state.BytesReceived)
            {
                string json = JsonConvert.SerializeObject(NetworkUtils.ToJson(state.File));
                File.WriteAllText(Settings.Default.FileFolder + state.ID + ".json", json);
                SendPacket(state.Client, new DownloadID(state.ID), SendCallback, FollowUpTask.DISCONNECT);
            }
            else
                handler.BeginReceive(state.Buffer, 0, FileStateObject.BufferSize, 0, ReceiveFileCallback,
                    state);
        }

        /// <summary>
        /// Verstuurt een packet naar de ingegeven client.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="packet"></param>
        /// <param name="callback"></param>
        /// <param name="task"></param>
        private void SendPacket(Client client, IPacket packet, AsyncCallback callback, FollowUpTask task)
        {
            Socket handler = client.Socket;
            byte[] buffer = Util.NetworkUtils.CreatePacket(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(packet.ToJson())));
            handler.BeginSend(buffer, 0, buffer.Length, 0, callback, new Tuple<Client, FollowUpTask>(client, task));
        }

        /// <summary>
        /// Callback verzenden.
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar)
        {
            Tuple<Client, FollowUpTask> tuple = (Tuple<Client, FollowUpTask>)ar.AsyncState;

            Client client = tuple.Item1;
            Socket handler = client.Socket;
            FollowUpTask followUp = tuple.Item2;

            int sent = handler.EndSend(ar);
#if DEBUG
            Console.WriteLine($"Sent {sent} bytes to {handler.RemoteEndPoint}");
#endif

            switch (followUp)
            {
                case FollowUpTask.RECEIVE_MSG:
                    MessageState state = new MessageState(client);
                    //handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
                    Receive(client, state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
                    break;
                case FollowUpTask.DISCONNECT:
                    DisconnectClient(client);
                    break;
                default:
                    break;
            }
        }

        private void Receive(Client client, byte[] buffer, int offset, int size, SocketFlags flags, AsyncCallback callback, object state)
        {

            try
            {
                client.Socket.BeginReceive(buffer, offset, size, flags, callback, state);
            }
            catch (SocketException sx)
            {
                if (sx.SocketErrorCode == SocketError.ConnectionAborted)
                {
                    ClientDisconnected(client);
                }
                else
                    throw sx;
            }

        }

        private void ClientDisconnected(Client client)
        {
            Console.WriteLine($"Client: {client.Socket.RemoteEndPoint} disconnected.");
            lock (connectedClients)
                connectedClients.Remove(client);
        }

        /// <summary>
        /// Dropt de client van de server.
        /// </summary>
        /// <param name="client"></param>
        private void DisconnectClient(Client client)
        {
            Socket socket = client.Socket;
            if (socket != null && socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
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
                    HandleFileDownloadRequest(FileDownloadRequest.ToClass(jsonData), state.Client);
                    break;
                case nameof(FileUploadRequest):
                    HandleFileUploadRequest(FileUploadRequest.ToClass(jsonData), state.Client);
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
            string path = Settings.Default.FileFolder + request.ID;
            string fileWithExtension = NetworkUtils.GetAssociatedFile(Settings.Default.FileFolder, request.ID);
            bool exists = File.Exists(fileWithExtension);
            

            FileInfoResponse response = new FileInfoResponse();
            if (exists)
            {
                string text = File.ReadAllText(path + ".json");
                dynamic json = JsonConvert.DeserializeObject(text);
                NetworkFile file = Util.NetworkUtils.FromJson(json);
                FileInfo fi = new FileInfo(fileWithExtension);

                file.FileSize = fi.Length;
                file.CreationDate = fi.CreationTimeUtc;
                response = new FileInfoResponse(file);
            }
            //state.Client.Socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveMessageCallback, state);
            SendPacket(state.Client, response, SendCallback, FollowUpTask.RECEIVE_MSG);
        }

        /// <summary>
        /// Handelt een bestand download af.
        /// </summary>
        /// <param name="request"></param>
        private void HandleFileDownloadRequest(FileDownloadRequest request, Client client)
        {
            string path = NetworkUtils.GetAssociatedFile(Settings.Default.FileFolder, request.ID);
            bool exists = File.Exists(path);

            if (exists)
                client.Socket.BeginSendFile(path, SendFileCallback, client);
        }

        /// <summary>
        /// Beëindigd de file send.
        /// </summary>
        /// <param name="ar"></param>
        private void SendFileCallback(IAsyncResult ar)
        {
            Client client = (Client)ar.AsyncState;

            client.Socket.EndSendFile(ar);

            DisconnectClient(client);
        }

        /// <summary>
        /// Handelt de bestand upload af.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="client"></param>
        private void HandleFileUploadRequest(FileUploadRequest request, Client client)
        {
            FileStateObject state = new FileStateObject(client, request.File);
            client.Socket.BeginReceive(state.Buffer, 0, FileStateObject.BufferSize, 0, ReceiveFileCallback, state);
        }

        /// <summary>
        /// Update voor server taken.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateElapsed(object sender, ElapsedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private abstract class StateObject
        {
            public Client Client { get; private set; }
            public static int BufferSize { get { return Settings.Default.MessageBufferSize; } }
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

            public MessageState(Client client) : base(client)
            {
                Data = new byte[0];
            }
        }

        private class FileStateObject : StateObject
        {
            public new static int BufferSize { get { return Settings.Default.FileWriteBufferSize; } }
            public FileStream Output { get; set; }
            public NetworkFile File { get; private set; }
            public long BytesToReceive { get; private set; }
            public long BytesReceived { get; set; }

            public string ID { get; set; }
            public FileStateObject(Client client, NetworkFile file) : base(client)
            {
                File = file;
                BytesToReceive = file.FileSize;
            }
        }

        private enum FollowUpTask
        {
            RECEIVE_MSG,
            RECEIVE_FILE,
            DISCONNECT,
            NOTHING
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
