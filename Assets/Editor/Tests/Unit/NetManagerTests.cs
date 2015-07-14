using NUnit.Framework;
using UnityEngine.Networking;

[TestFixture]
public class NetManagerTesting {

	[Test]
	public void Init()
	{
		NetManager.Init ();

		Assert.AreEqual ( true , NetManager.mIsInitialized );
		Assert.AreNotEqual ( null , NetManager.mChannelReliable );
		Assert.AreNotEqual ( null , NetManager.mChannelUnreliable );
		Assert.AreNotEqual( null , NetManager.mConnectionConfig );

		NetManager.Shutdown ();
	}

	[Test]
	public void CreateServer()
	{
		NetManager.Init();

		NetServer s = NetManager.CreateServer( 30 , 7777 );

		Assert.IsNotNull (s);
		Assert.IsTrue( NetUtils.IsSocketValid ( s.mSocket ) );
		Assert.IsTrue ( s.mIsRunning );
		Assert.AreEqual( 7777 , s.mPort );

		NetworkTransport.RemoveHost( s.mSocket );
		NetManager.Shutdown ();
	}

	[Test]
	public void CreateServerWithSim()
	{
		NetManager.Init();
		
		NetServer s = NetManager.CreateServer( 30 , 7777 , 10 , 100 );
		
		Assert.IsNotNull (s);
		Assert.IsTrue( NetUtils.IsSocketValid ( s.mSocket ) );
		Assert.IsTrue ( s.mIsRunning );
		Assert.AreEqual( 7777 , s.mPort );
		
		NetManager.Shutdown ();
	}




		


}
