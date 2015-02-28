using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server_performant_Taskbased
{
    internal sealed class Listener : IDisposable
    {
        private readonly int port;

        private readonly TcpListener tcpListener;

       public IList<Client> clients = new List<Client>();

        public Listener(int port)
        {
            this.port = port;
            this.tcpListener = new TcpListener(IPAddress.Any, this.port);
            this.tcpListener.Start();
             // this.tcpListener.BeginAcceptSocket()
            Task.Factory.StartNew(this.ListenLoop);
        }

        public int Port
        {
            get
            {
                return this.port;
            }
        }

        public void Dispose()
        {
            foreach (var client in this.clients)
            {
                client.Dispose();
            }
        }

        private async void ListenLoop()
        {
            while (true)
            {
                var socket = await this.tcpListener.AcceptSocketAsync();
                // 		NoDelay	false	bool , a false a default: ilyenkor nem küldi el azonnal, hanem összevárja

                if (socket == null)
                {
                    break;
                }

                var client = new Client(socket, this.ClientDisconnected);

                this.clients.Add(client);

               IPEndPoint _socketIPEndPoint = socket.RemoteEndPoint as IPEndPoint;
               String _ssocketIPEndPoint = "?";
               if (_socketIPEndPoint != null){ _ssocketIPEndPoint = _socketIPEndPoint.Address.ToString() + ":" + _socketIPEndPoint.Port.ToString(); }
                Console.WriteLine("Client Connected :) data received: "+socket.Available.ToString() +", Remote address: "+_ssocketIPEndPoint);
                await Task.Factory.StartNew(client.Do);
            }
        }

        private void ClientDisconnected(Client client)
        {
            if (client == null)
            {
                Console.WriteLine("Client disconnected by null");
                return;
            }

            client.Dispose();
            this.clients.Remove(client);
            Console.WriteLine("Client disconnected -removed");
        }
    }
}
