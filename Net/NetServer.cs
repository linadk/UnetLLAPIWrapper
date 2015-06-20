using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class NetServer {

	public int mSocket = -1;
	private int mPollBufferSize = 1024;

	// Todo : Maintain list of clients

	public bool mIsRunning = false;

	public NetServer( int socket ) {
		mSocket = socket;
		mIsRunning = true;
	}

	public void BroadcastStream( object o , long buffersize , int channel ){
	}

	public bool SendStream( object o , long buffsize , int connId , int channel ){
		
		byte error;
		byte[] buffer = new byte[buffsize];
		Stream stream = new MemoryStream(buffer);
		BinaryFormatter f = new BinaryFormatter();
		
		f.Serialize ( stream , o );
		
		NetworkTransport.Send ( mSocket , connId , NetManager.mChannelReliable , buffer , (int)stream.Position , out error );
		
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

		// If server isnt running we don't poll
		if(!mIsRunning){
			return;
		}
		
		int recHostId; 
		int connectionId;
		int channelId;
		int dataSize;
		byte[] buffer = new byte[mPollBufferSize];
		byte error;
		
		NetworkEventType networkEvent = NetworkEventType.DataEvent;
		
		// Poll both server/client events
		do
		{
			networkEvent = NetworkTransport.Receive( out recHostId , out connectionId , out channelId , buffer , mPollBufferSize , out dataSize , out error );

			switch(networkEvent){

			case NetworkEventType.Nothing:
				break;

			case NetworkEventType.ConnectEvent:
				// Server received disconnect event
				if( recHostId == mSocket ){
					Debug.Log ("Server: Player " + connectionId.ToString () + " connected!" );
				}
				
				break;
				
			case NetworkEventType.DataEvent:
				// Server received data
				if( recHostId == mSocket ){
					
					// Let's decode data
					Stream stream = new MemoryStream(buffer);
					BinaryFormatter f = new BinaryFormatter();
					string msg = f.Deserialize( stream ).ToString ();
					
					Debug.Log ("Server: Received Data from " + connectionId.ToString () + "! Message: " + msg );
				}

				break;
				
			case NetworkEventType.DisconnectEvent:

				// Server received disconnect event
				if( recHostId == mSocket ){
					Debug.Log ("Server: Received disconnect from " + connectionId.ToString () );
				}
				break;
			}
			
		} while ( networkEvent != NetworkEventType.Nothing );
	}

}
