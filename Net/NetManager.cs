using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// The NetManager is responsible for creating our client and server sockets and housing our messaging queue/delegate system.
/// </summary>
public class NetManager : MonoBehaviour {

	// Connection config vars
	static private ConnectionConfig mConnectionConfig;
	static public byte mChannelReliable;
	static public byte mChannelUnreliable;

	// True if Init has ran.
	static public bool mIsInitialized = false;

	// Client and server sockets
	static public NetClient mClient = null;
	static public NetServer mServer = null;

	// Delegates for polling
	public delegate void NetEventHandler( int connectionId , int channelId , byte[] buffer , int datasize );
	public static NetEventHandler OnServerConnection = null;
	public static NetEventHandler OnServerData = null;
	public static NetEventHandler OnServerDisconnect = null;
	public static NetEventHandler OnClientConnection = null;
	public static NetEventHandler OnClientData = null;
	public static NetEventHandler OnClientDisconnect = null;

	/// <summary>
	/// Initialize our low level network APIs.
	/// </summary>
	public static void Init (){

		// Set up NetworkTransport
		GlobalConfig gc = new GlobalConfig();
		gc.ReactorModel = ReactorModel.FixRateReactor;
		gc.ThreadAwakeTimeout = 10;
		NetworkTransport.Init (gc);

		// Set up our channel configuration
		mConnectionConfig = new ConnectionConfig();
		mChannelReliable = mConnectionConfig.AddChannel (QosType.ReliableSequenced);
		mChannelUnreliable = mConnectionConfig.AddChannel(QosType.UnreliableSequenced);

		mIsInitialized = true;

	}

	/// <summary>
	/// Creates a server that listens on a given port.
	/// </summary>
	/// <returns>The server object.</returns>
	/// <param name="maxConnections">Max connections.</param>
	/// <param name="port">Port.</param>
	public static NetServer CreateServer ( int maxConnections , int port ){

		if(!mIsInitialized){
			Debug.Log ("NetManager::CreateServer( ... ) - NetManager was not initialized. Did you forget to call NetManager.Init()?");
			return null;
		}

		if(mServer != null)
		{
			Debug.Log ("NetManager::CreateServer( ... ) - Server already running!");
			return mServer;
		}

		HostTopology ht = new HostTopology( mConnectionConfig , maxConnections  );
		int ssocket = NetworkTransport.AddHost ( ht , port  );

		if(!NetUtils.IsSocketValid (ssocket)){
			Debug.Log ("NetManager::CreateServer( " + maxConnections + " , " + port.ToString () + " ) returned an invalid socket ( " + ssocket.ToString() + " )" );
		}

		NetServer s = new NetServer(ssocket);
		mServer = s;

		return s;

	}

	/// <summary>
	/// Create a client that is ready to connect with a server.
	/// </summary>
	/// <returns>The client.</returns>
	public static NetClient CreateClient (){

		if(!mIsInitialized){
			Debug.Log ("NetManager::CreateServer( ... ) - NetManager was not initialized. Did you forget to call NetManager.Init()?");
			return null;
		}

		if(mClient != null)
		{
			Debug.Log ("NetManager::CreateClient( ... ) - Client already running!");
		}

		HostTopology ht = new HostTopology( mConnectionConfig , 1 ); // Clients only need 1 connection
		int csocket = NetworkTransport.AddHost ( ht  );

		if(!NetUtils.IsSocketValid (csocket)){
			Debug.Log ("NetManager::CreateClient() returned an invalid socket ( " + csocket + " )" );
		}

		NetClient c = new NetClient(csocket);
		mClient = c;

		return c;
	}

	/// <summary>
	/// Reads network events and delegates how they are used
	/// </summary>
	public static void PollEvents(){
		
		int recHostId; 
		int connectionId;
		int channelId;
		int dataSize;
		byte[] buffer = new byte[1024];
		byte error;
		
		NetworkEventType networkEvent = NetworkEventType.DataEvent;
		
		// Poll both server/client events
		do
		{
			networkEvent = NetworkTransport.Receive( out recHostId , out connectionId , out channelId , buffer , 1024 , out dataSize , out error );
			
			switch(networkEvent){

			// Nothing
			case NetworkEventType.Nothing:
				break;

			// Connect
			case NetworkEventType.ConnectEvent:
				// Server Connect Event
				if(mServer != null){
					if( recHostId == mServer.mSocket){
						OnServerConnection( connectionId , channelId , buffer , dataSize );
						mServer.AddClient ( connectionId );
					}
				}

				// Client Connect Event
				if(mClient != null){
					if( recHostId == mClient.mSocket ){
						OnClientConnection( connectionId , channelId , buffer , dataSize );
						mClient.mConnected = true; // Set client connected to true
					}
				}
				
				break;

			// Data 
			case NetworkEventType.DataEvent:

				// Server Received Data
				if(mServer != null){
					if( recHostId == mServer.mSocket ){

						// Server Data Delegate
						OnServerData( connectionId , channelId , buffer , dataSize );
					}
				}

				// Client Received Data
				if(mClient != null){
					if( recHostId == mClient.mSocket ){

						// Client Data Delegate
						OnClientData(  connectionId , channelId , buffer , dataSize );
					}
				}
				break;

			// Disconnect
			case NetworkEventType.DisconnectEvent:

				// Server Received Disconnect
				if(mServer != null){
					if( recHostId == mServer.mSocket ){
						OnServerDisconnect( connectionId , channelId , buffer , dataSize );
						mServer.RemoveClient ( connectionId );
					}
				}

				// Client Received Disconnect
				if(mClient != null){
					if( recHostId == mClient.mSocket && connectionId == mClient.mConnection){
						
						// Flag to let client know it can no longer send data
						mClient.mConnected = false;

						OnClientDisconnect(  connectionId , channelId , buffer , dataSize );
					}
				}
				

				break;
			}
			
		} while ( networkEvent != NetworkEventType.Nothing );
	}

}
