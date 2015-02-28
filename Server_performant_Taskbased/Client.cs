using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server_performant_Taskbased
{
    internal delegate void ClientDisconnectedDelegate(Client client);

    internal sealed class Client : IDisposable
    {
        private readonly Socket socket; // SendBufferSize	8192 ReceiveBufferSize	8192
	
        private readonly ClientDisconnectedDelegate doDisconnect;

        private readonly Stream networkStream;

       // private readonly Stream memoryStream = new MemoryStream();

       // private readonly TextReader streamReader;

        public Client(Socket socket, ClientDisconnectedDelegate doDisconnect)
        {
            this.socket = socket;
            this.doDisconnect = doDisconnect; 
            Task.Factory.StartNew(this.CheckForConnection);
            this.networkStream = new NetworkStream(this.socket, true);
          //  this.streamReader = new StreamReader(this.memoryStream);
            
            //this.networkStream.ReadAsync()
            //this.networkStream.WriteAsync()

             //this.socket.BeginReceive()
             //this.socket.BeginSend()
             //this.socket.Blocking
             //this.socket.DontFragment
             //this.socket.EnableBroadcast
             //this.socket.DisconnectAsync
             //this.socket.EndReceive
             //this.socket.EndSend
             //this.socket.NoDelay
             //this.socket.Poll(1, SelectMode.SelectError)
             //this.socket.SendPacketsAsync
             //this.socket.SendToAsync()
             //this.socket.SetIPProtectionLevel( IPProtectionLevel.Unrestricted);
             //this.socket.Ttl
            
        }

        public void Dispose()
        {
           // this.streamReader.Dispose();
            this.networkStream.Dispose();
            this.socket.Dispose();
           // this.memoryStream.Dispose();
        }

        public async void Do()
        {
             try
            {
                 // listen to read :
            var buffer = new byte[4096];
            var bytesRead = await this.networkStream.ReadAsync(buffer, 0, buffer.Length);
            var str = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Console.WriteLine("Received nr of bytes : " + bytesRead.ToString() + ", " + str );
            //Console.WriteLine("Doing some awesome work that takes a few seconds.");
            ////Thread.Sleep(1000);
            //Console.WriteLine("Finished doing work.");

            //// response :
            //    await this.networkStream.WriteAsync(buffer, 0, bytesRead);
            //    await this.networkStream.FlushAsync();
            }
            catch (Exception exc_streamWrite)
            {
                Console.WriteLine("networkStream.ReadAsync or WriteAsync ERROR !, : " + exc_streamWrite.Message);
            }
        }

        private void CheckForConnection()
        {
            while (true)
            {
                bool isDisconnected;

                try
                {
                    isDisconnected = this.socket.Poll(1, SelectMode.SelectRead) && (this.socket.Available == 0);
                }
                catch (SocketException)
                {
                    isDisconnected = true;
                }

                if (isDisconnected && (this.doDisconnect != null))
                {
                    this.doDisconnect(this);
                    return;
                }

                Thread.Sleep(100);
            }
        }

        public async void SendMessage(string message)
        {
            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(message);
                //byte[] buffer = new byte[8192];  // 4096 MTU ??
                //for (int i = 0; i < buffer.Length; i++)
                //{
                //    buffer [i]=Encoding.ASCII.GetBytes("a")[0];
                //}
                //buffer [4095]= Encoding.ASCII.GetBytes("V")[0];

                await this.networkStream.WriteAsync(buffer, 0, buffer.Length);
                await this.networkStream.FlushAsync();
                Console.WriteLine("Message sent to client. ("+message+")");
            }
            catch (Exception exc_streamWrite)
            {
                Console.WriteLine("networkStream WriteAsync ERROR !, : " + exc_streamWrite.Message);
            }
        }

    }
}
