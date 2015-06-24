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

		NetworkTransport.Shutdown();
	}
}
