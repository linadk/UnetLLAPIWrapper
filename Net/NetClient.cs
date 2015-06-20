using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class NetClient{

	public int mSocket = -1;
	public int mConnection = -1;
	public bool mConnected = false;
	
	// Use this for initialization
	public NetClient ( int socket ) {
		mSocket = socket;
	}

	public bool Connect( string ip , int port ){

		byte error;
		mConnection = NetworkTransport.Connect( mSocket , ip , port , 0 , out error );

		if( NetUtils.IsNetworkError ( error )){
			Debug.Log("NetClient::Connect( "  + ip + " , " + port.ToString () + " ) Failed with reason '" + NetUtils.GetNetworkError (error) + "'.");
			return false;
		}

		return true;
	}

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

	public object ReceiveStream( byte[] buffer ){
		Stream stream = new MemoryStream(buffer);
		BinaryFormatter f = new BinaryFormatter();
		return f.Deserialize( stream );	
	}

	public void PollEvents(){
		
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

			Debug.Log (networkEvent.ToString () );
			
			switch(networkEvent){
			case NetworkEventType.Nothing:
				break;
			case NetworkEventType.ConnectEvent:
				if( recHostId == mSocket ){
					Debug.Log ("Client: Client connected to " + connectionId.ToString () + "!" );
					
					// Set our flag to let client know that they can start sending data and send some data
					mConnected = true; 
					this.SendStream( 32 + "Hello2!" , 1024 );
				}
				
				break;
				
			case NetworkEventType.DataEvent:
				if( recHostId == mSocket ){
					Debug.Log ("Client: Received Data from " + connectionId.ToString () + "!" );
				}
				break;
				
			case NetworkEventType.DisconnectEvent:
				// Client received disconnect event
				if( recHostId == mSocket && connectionId == mConnection ){
					Debug.Log ("Client: Disconnected from server!");
					
					// Flag to let client know it can no longer send data
					mConnected = false;
				}

				break;
			}
			
		} while ( networkEvent != NetworkEventType.Nothing );
	}

}
