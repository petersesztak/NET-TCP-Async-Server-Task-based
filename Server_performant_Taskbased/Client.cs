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

        public CancellationTokenSource cancellationTokenSourceForAsyncRead = new CancellationTokenSource();
        public CancellationToken cancellationTokenForAsyncRead;

       // private readonly Stream memoryStream = new MemoryStream();

       // private readonly TextReader streamReader;

        public Client(Socket socket, ClientDisconnectedDelegate doDisconnect)
        {
            this.socket = socket;
            

            this.doDisconnect = doDisconnect; 
            Task.Factory.StartNew(this.CheckForConnection);
            cancellationTokenForAsyncRead = cancellationTokenSourceForAsyncRead.Token;
            this.cancellationTokenForAsyncRead.Register(RegisteredActionOfcancellationTokenForAsyncRead );

            this.networkStream = new NetworkStream(this.socket, true);
          

          //  this.streamReader = new StreamReader(this.memoryStream);
            
            //this.socket.Blocking          // 		Blocking	true	bool
            //this.socket.Connected         // 		Connected	true	bool
            //this.socket.DontFragment      // 		DontFragment	true	bool
            //this.socket.IsBound           //		IsBound	true	bool

            //ReceiveBufferSize	8192	int
            //ReceiveTimeout	0	int
            //SendBufferSize	8192	int
            //SendTimeout	0	int
            //this.socket.NoDelay           //		NoDelay	false	bool // Nagle Algorithm
            //this.socket.ProtocolType      //		ProtocolType	Tcp	System.Net.Sockets.ProtocolType
            //this.socket.SocketType       // 	    SocketType	Stream	System.Net.Sockets.SocketType
            //this.socket.Ttl              //		Ttl	128	short // Time To Live (TTL). e.g. 128 router hops.

            //this.socket.DualMode          // +		DualMode	'socket.DualMode' threw an exception of type 'System.NotSupportedException'	bool {System.NotSupportedException}
                                            //		Message	"This protocol version is not supported."	string
            //this.socket.EnableBroadcast   //-		EnableBroadcast	'socket.EnableBroadcast' threw an exception of type 'System.Net.Sockets.SocketException'	bool {System.Net.Sockets.SocketException}
                                            // 		Message	"An unknown, invalid, or unsupported option or level was specified in a getsockopt or setsockopt call"	string
            //-		LingerState	{System.Net.Sockets.LingerOption}	System.Net.Sockets.LingerOption // Socket will delay closing a socket in an attempt to send all pending data
                                   		    // Enabled	false	bool
		                                    // LingerTime	0	int

            //+		MulticastLoopback	'socket.MulticastLoopback' threw an exception of type 'System.Net.Sockets.SocketException'	bool {System.Net.Sockets.SocketException}

            //this.socket.Poll(1, SelectMode.SelectError)
            //this.socket.SetIPProtectionLevel( IPProtectionLevel.Unrestricted);
            
            //this.networkStream.ReadAsync()
            //this.networkStream.WriteAsync()
            //this.networkStream.FlushAsync()
            //this.socket.DisconnectAsync
            
            //this.socket.SendPacketsAsync
            //this.socket.SendToAsync()

            

            
        }

        Action RegisteredActionOfcancellationTokenForAsyncRead =
        () =>
            Console.WriteLine("ActionOfcancellationTokenForAsyncRead !" );

        public void Dispose()
        {
            cancellationTokenSourceForAsyncRead.CancelAfter(0);

           // this.streamReader.Dispose();
            if (this.networkStream!=null)
            {
                this.networkStream.Dispose(); 
            }
            //this.cancellationTokenSourceForAsyncRead.Dispose();
            if (this.socket!=null)
            {
                this.socket.Dispose(); 
            }
           // this.memoryStream.Dispose();
        }

        public async void Do_ReadAsync()
        {
               // listen to read :
            while (cancellationTokenForAsyncRead.IsCancellationRequested != true) // If you don't call ReadAsync on the stream repeatedly, as long as the underlying TCP connection is open it will continue receiving data into the buffer, but your program cannot get them.
                {
                     try
                    {
                        var buffer = new byte[4096];
                        var bytesRead = await this.networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationTokenForAsyncRead);
                        if (bytesRead != 0 && this.networkStream != null && cancellationTokenForAsyncRead.IsCancellationRequested != true)
                        {
                            var str = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            Console.WriteLine("Received nr of bytes : " + bytesRead.ToString() + ", " + str); 
                        }
                    }
                     catch (Exception exc_streamWrite)
                     {
                         Console.WriteLine("networkStream.WriteAsync ERROR !, : " + exc_streamWrite.Message);
                     }
                }
            //Console.WriteLine("Doing some awesome work that takes a few seconds.");
            ////Thread.Sleep(1000);
            //Console.WriteLine("Finished doing work.");

            //// response :
            //    await this.networkStream.WriteAsync(buffer, 0, bytesRead);
            //    await this.networkStream.FlushAsync();
            
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
                catch (SocketException socketExc)
                {
                    isDisconnected = true;
                    Console.WriteLine("SocketException ERROR during determinate socket status !, : " + socketExc.Message);
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
