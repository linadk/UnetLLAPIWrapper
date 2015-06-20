using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public static class NetUtils {

	/// <summary>
	/// Return string value of any network error if it is an error, otherwise return "";
	/// </summary>
	/// <param name="error">Error as string or "" if no error.</param>
	public static string GetNetworkError(byte error){
		if( error != (byte)NetworkError.Ok){
			NetworkError nerror = (NetworkError)error;
			return nerror.ToString ();
		}

		return "";
	}	

	/// <summary>
	/// Returns true if the given code is a network error, false if not.
	/// </summary>
	public static bool IsNetworkError( byte error ){
		if( error != (byte)NetworkError.Ok){
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Returns true if our socket id is valid, false if not.
	/// </summary>
	public static bool IsSocketValid( int sock ){
		if( sock < 0 ){
			return false;
		}
		else
		{
			return true;
		}
	}
	
}
