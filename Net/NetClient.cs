using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// The net client handles connecting to a server as well as sending and receiving messages. 
/// </summary>
public class NetClient{

	public int mSocket = -1;
	public int mConnection = -1;
	public string mServerIP = null;
	public int mPort = -1;
	public bool mIsConnected = false;

	/// <summary>
	/// Our delegate that gets called to handle processing data 	
	/// </summary>
	public delegate void NetEventHandler( NetworkEventType net , int connectionId , int channelId , byte[] buffer , int datasize );
	public NetEventHandler OnMessage = null;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="NetClient"/> class.
	/// </summary>
	/// <param name="socket">Valid socket id for the NetClient. Given by NetManager.</param>
	public NetClient () {

		HostTopology ht = new HostTopology( NetManager.mConnectionConfig , 1 ); // Clients only need 1 connection
		int csocket = NetworkTransport.AddHost ( ht  );
		
		if(!NetUtils.IsSocketValid (csocket)){
			Debug.Log ("NetManager::CreateClient() returned an invalid socket ( " + csocket + " )" );
		}

		mSocket = csocket;
	}

	/// <summary>
	/// Connect the specified ip and port.
	/// </summary>
	/// <param name="ip">Ip.</param>
	/// <param name="port">Port.</param>
	public bool Connect( string ip , int port ){

		byte error;
		mConnection = NetworkTransport.Connect( mSocket , ip , port , 0 , out error );

		if( NetUtils.IsNetworkError ( error )){
			Debug.Log("NetClient::Connect( "  + ip + " , " + port.ToString () + " ) Failed with reason '" + NetUtils.GetNetworkError (error) + "'.");
			return false;
		}

		mServerIP = ip;
		mPort = port;

		return true;
	}

	/// <summary>
	/// Disconnect the client from the server.
	/// </summary>
	public bool Disconnect( ){

		if(!mIsConnected){
			Debug.Log ("NetClient::Disconnect() Failed with reason 'Not connected to server!");
			return false;
		}

		byte error;

		NetworkTransport.Disconnect ( mSocket , mConnection , out error );

		if( NetUtils.IsNetworkError ( error )){
			Debug.Log("NetClient::Disconnect() Failed with reason '" + NetUtils.GetNetworkError (error) + "'.");
			return false;
		}

		mIsConnected = false;
		
		return true;
	}

	/// <summary>
	/// Sends the stream.
	/// </summary>
	/// <returns><c>true</c>, if stream was sent, <c>false</c> otherwise.</returns>
	/// <param name="o">The object you wish to send as a serialized stream.</param>
	/// <param name="buffsize">Max buffer size for your data.</param>
	public bool SendStream( object o , long buffsize ){

		byte error;
		byte[] buffer = new byte[buffsize];
		Stream stream = new MemoryStream(buffer);
		BinaryFormatter f = new BinaryFormatter();

		f.Serialize ( stream , o );
		
		NetworkTransport.Send ( mSocket , mConnection , NetManager.mChannelReliable , buffer , (int)stream.Position , out error );
		
		if( NetUtils.IsNetworkError ( error )){
			Debug.Log("NetClient::SendStream( " + o.ToString () + " , " + buffsize.ToString () + " ) Failed with reason '" + NetUtils.GetNetworkError (error) + "'.");
			return false;
		}

		return true;
	}

	/// <summary>
	/// Returns a deserialized stream object.
	/// </summary>
	/// <param name="buffer">Buffer that contains the data.</param>
	public object ReceiveStream( byte[] buffer ){
		Stream stream = new MemoryStream(buffer);
		BinaryFormatter f = new BinaryFormatter();
		return f.Deserialize( stream );	
	}

}
