/// <summary>
/// Unet LLAPI Wrapper Test Console
/// 
/// This test script will enable a console that will allow you to run the following commands
/// 
/// server.start <maxconnections> <port> - start a server
/// client.connect <ip> <port>  - create and connect with a client
/// client.send <message> - Send a message to the server
/// server.send <clientid> <message> - Send a message to a specific client
/// server.getclients - Get all clients connected to the server
/// server.broadcast <msg> - Broadcast a message to all clients
/// 
/// You can read through the code below to see how the commands from the console translate to actual code.
/// </summary>

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkTest : MonoBehaviour {

	NetServer mServer;
	NetClient mClient;


	// Use this for initialization
	void Start () {

		NetManager.Init ();

		// We take these out in favor of using console commands
		//mServer = NetManager.CreateServer( 10 , 7777 );
		//mClient = NetManager.CreateClient ();
		//mClient.Connect( "127.0.0.1" , 7777 );

		// Client Event Callbacks
		NetManager.OnClientConnection = ClientConnection;
		NetManager.OnClientData = ClientData;
		NetManager.OnClientDisconnect = ClientDisconnect;

		// Set up our console
		DebugConsole.IsOpen = true;
		DebugConsole.RegisterCommand( "server.start" , ConsoleServerStart );
		DebugConsole.RegisterCommand( "client.connect" , ConsoleClientConnect );
		DebugConsole.RegisterCommand( "client.disconnect" , ConsoleClientDisconnect );
		DebugConsole.RegisterCommand( "client.send" , ConsoleClientSend );
		DebugConsole.RegisterCommand( "server.send" , ConsoleServerSend );
		DebugConsole.RegisterCommand( "server.getclients" , ConsoleServerGetClients );
		DebugConsole.RegisterCommand( "server.broadcast" , ConsoleServerBroadcast );
		DebugConsole.RegisterCommand("server.disconnect" , ConsoleServerDisconnect );
		DebugConsole.RegisterCommand ("network.test" , FullLocalTest );
		
	}

	/// <summary>
	/// Console command for starting a server.
	/// </summary>
	public string ConsoleServerStart( params string[] args ){
		if( args.Length < 3 ){
			return "Invalid Number of Arguments : server.start <maxconnections> <port>";
		}

		mServer = NetManager.CreateServer( int.Parse ( args[1] ) , int.Parse( args[2] ) );
		
		if(mServer != null){ 
			mServer.OnMessage = OnServerMessage;
			return "Server is running!";
		} else {
			return "Server failed to start!";
		}
	}

	/// <summary>
	/// Console command for connecting a client.
	/// </summary>
	public string ConsoleClientConnect( params string[] args ){
		if( args.Length < 3 ){
			return "Invalid Number of Arguments : client.connect <ip> <port>";
		}

		mClient = NetManager.CreateClient ();
		mClient.Connect ( args[1] , int.Parse ( args[2] ) );
		
		if(mClient!= null){ 
			return "Client is connected!";
		} else {
			return "Client connection failed!";
		}
	}

	/// <summary>
	/// Consoles command for connecting a client.
	/// </summary>
	public string ConsoleClientDisconnect( params string[] args ){
		if( args.Length > 1  ){
			return "Invalid Number of Arguments : client.disconnect takes no arguments";
		}
		
		bool ret = mClient.Disconnect ();

		if(ret){
			return "Client has been disconnected!";
		}
		else
		{
			return "Client disconnection failed!";
		}
		
	}

	/// <summary>
	/// Console command to send a message from the client to the server.
	/// </summary>
	public string ConsoleClientSend( params string[] args ){
		if( args.Length < 2 ){
			return "Invalid Number of Arguments : client.send <message>";
		}

		if( !mClient.mIsConnected ){
			return "Client not connected!";
		}

		mClient.SendStream ( args[1] , 1024 );

		return "Message sent!";
	}

	/// <summary>
	/// Console command to send a message from the server to a specific client.
	/// </summary>
	public string ConsoleServerSend( params string[] args ){
		if( args.Length < 3 ){
			return "Invalid Number of Arguments : server.send <clientid> <message>";
		}
		
		if( !mServer.mIsRunning ){
			return "Server not running!";
		}

		mServer.SendStream ( args[2] , 1024 , int.Parse (args[1]) , NetManager.mChannelReliable );
		
		return "Message sent!";
	}

	/// <summary>
	/// Console command to broadcast a message to all connected clients.
	/// </summary>
	public string ConsoleServerBroadcast( params string[] args ){
		if( args.Length < 2){
			return "Invalid Number of Arguments : server.broadcast <message>";
		}
		
		if( !mServer.mIsRunning ){
			return "Server not running!";
		}
		
		mServer.BroadcastStream ( args[1] , 1024 , NetManager.mChannelReliable );
		
		return "";
	}

	/// <summary>
	/// Get a list of all clients currently connected to the server.
	/// </summary>
	public string ConsoleServerGetClients( params string[] args ){

		if( !mServer.mIsRunning ){
			return "Server not running!";
		}

		string users = "";

		foreach (int element in mServer.mClients ){
			users += "User #" + element.ToString () + "\n";
		}

		return users;
	}

	/// <summary>
	/// Disconnect a client of a given id from server. Usage: server.disconnect <clientid>
	/// </summary>
	public string ConsoleServerDisconnect( params string[] args ){
		
		if( !mServer.mIsRunning ){
			return "Server not running!";
		}

		if( args.Length < 2){
			return "Invalid Number of Arguments : server.broadcast <message>";
		}

		bool res = mServer.DisconnectClient( int.Parse (args[1]) );
		if( res ){ return "User " + args[1] + "disconnected!"; } else { return "User " + args[1] + "not disconnected!"; }

	}

	/// <summary>
	/// Runs network test to determine interface functionality status. Currently has bug where running twice crashes.
	/// </summary>
	/// <returns>The local test.</returns>
	/// <param name="args">Arguments.</param>
	public string FullLocalTest( params string[] args ){

		int port = Random.Range( 100 , 99999 );
		int maxUsers = Random.Range( 2 , 100 );
		string ip = "127.0.0.1";

		Debug.Log ("FullLocalTest() START : Port( " + port.ToString () + " ) MaxUsers( " + maxUsers.ToString() + ")");

		mServer = NetManager.CreateServer( maxUsers , port );

		mServer.OnMessage = OnServerMessage;

		
		if(mServer == null){ Debug.Log ("FullLocalTest() ERROR : Server instance not created!" ); return ""; }
		if(!NetUtils.IsSocketValid( mServer.mSocket )){ Debug.Log ("FullLocalTest() ERROR : Server socket invalid!" ); return ""; }
		if(!mServer.mIsRunning){ Debug.Log ("FullLocalTest() ERROR : Server is not running after create server called!" ); return ""; }
		
		Debug.Log ("FullLocalTest() : Server started successfully!");

		mClient = NetManager.CreateClient ();

		if(mClient == null){ Debug.Log ("FullLocalTest() ERROR : Client instance not created!" ); return ""; }
		if(!NetUtils.IsSocketValid ( mClient.mSocket ) ){ Debug.Log ("FullLocalTest() ERROR : Client socket invalid!"); return ""; }

		mClient.OnMessage = OnClientMessage;
		
		Debug.Log ("FullLocalTest() : Client started successfully!");

		if(! mClient.Connect ( ip , port ) ){ Debug.Log ("FullLocalTest() ERROR : Client connect failed!"); return ""; }

		// Now we go to our On*Message delegates to do the rest of the testing
		return "";

	}


	/// <summary>
	/// Poll for our network events. 
	/// </summary>
	void Update() {
		NetManager.PollEvents();
	}

	IEnumerator Wait()
	{
			yield return new WaitForSeconds(1.0f);
	}
	
	
	/// <summary>
	/// Handle server messages	
	/// </summary>
	/// <param name="networkEvent">Network event.</param>
	/// <param name="connectionId">Connection identifier.</param>
	/// <param name="channelId">Channel identifier.</param>
	/// <param name="buffer">Buffer.</param>
	/// <param name="datasize">Datasize.</param>
	public void OnServerMessage( NetworkEventType networkEvent , int connectionId , int channelId , byte[] buffer, int datasize ){


		switch(networkEvent){
			
			// Nothing
		case NetworkEventType.Nothing:
			break;
			
			// Connect
		case NetworkEventType.ConnectEvent:
			mServer.SendStream("Hello from the server!" , 1024 , connectionId , NetManager.mChannelReliable );
			break;
			
			// Data 
		case NetworkEventType.DataEvent:
			string data = mServer.ReceiveStream ( buffer ).ToString ();

			switch(data){
			case "Hello from the client!":
				Debug.Log ( "Test SUCCESS: Received stream from client" );
				mServer.BroadcastStream ("Broadcast test!" , 1024 , NetManager.mChannelReliable );
				break;
			default:
				Debug.Log ( "Test FAIL : Data received on server was incorrect!" );
				break;
			}
			
			break;
			
			// Disconnect
		case NetworkEventType.DisconnectEvent:
			Debug.Log ("Client disconnect event processed, shutting down server");
			NetManager.Shutdown ();
			DebugConsole.Log ("All tests passed!");
			break;
		}
	}

	/// <summary>
	/// Handle client messages
	/// </summary>
	/// <param name="networkEvent">Network event.</param>
	/// <param name="connectionId">Connection identifier.</param>
	/// <param name="channelId">Channel identifier.</param>
	/// <param name="buffer">Buffer.</param>
	/// <param name="datasize">Datasize.</param>
	public void OnClientMessage( NetworkEventType networkEvent , int connectionId , int channelId , byte[] buffer, int datasize ){
		
		
		switch(networkEvent){
			
		// Connect
		case NetworkEventType.ConnectEvent:
			break;
			
		// Data 
		case NetworkEventType.DataEvent:
			string data = mClient.ReceiveStream( buffer ).ToString ();

			switch(data){
			case "Hello from the server!":
				mClient.SendStream ("Hello from the client!", 1024);
				Debug.Log("Test SUCCESS : Received stream from server!");
				break;
			case "Broadcast test!":
				Debug.Log ("Test SUCCESS : Received broadcast from server!");
				mClient.Disconnect ();
				break;
			default:
				Debug.Log ("Test FAIL : One or more data messages were incorrect!(" + data + ")");
				break;
			}

			break;
			
		// Disconnect
		case NetworkEventType.DisconnectEvent:
			break;
		}
	}

/// <summary>
	/// Callback that is fired on the client when the client connects to a server
	/// </summary>
	public void ClientConnection( int connectionId , int channelId , byte[] buffer , int datasize ){
		DebugConsole.Log ( "Client: Connection to " + connectionId.ToString () );
	}

	/// <summary>
	/// Callback that is fired on the client when the client receives data.
	/// </summary>
	public void ClientData( int connectionId , int channelId , byte[] buffer , int datasize ){
		DebugConsole.Log ("Server says: " + mClient.ReceiveStream (buffer));
	}

	/// <summary>
	/// Callback that is fired on the client when the client disconnects.
	/// </summary>
	public void ClientDisconnect( int connectionId , int channelId , byte[] buffer , int datasize ){
		DebugConsole.Log ("Client: User disconnected from server! ");
	}
}




	