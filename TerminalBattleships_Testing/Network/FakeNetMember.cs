using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using TerminalBattleships.Network;

namespace TerminalBattleships_Testing.Network
{
	class FakeNetMember : INetMember
	{
		public Func<bool> HandshakeHandler { get; set; }
		public Action DisconnectionHandler { get; set; }
		public ForkStream FrontStream { get; }
		public ForkStream BackStream { get; }

		public bool Connected { get; set; }
		public bool IsServer { get; set; }
		public EndPoint LocalEP { get; set; }
		public Stream Stream => FrontStream;
		public int Available => FrontStream.Available;

		public FakeNetMember()
		{
			ConcurrentQueue<byte> a = new ConcurrentQueue<byte>(), b = new ConcurrentQueue<byte>();
			FrontStream = new ForkStream(a, b);
			BackStream = new ForkStream(b, a);
		}
		public FakeNetMember(ForkStream frontStream, ForkStream backStream)
		{
			FrontStream = frontStream ?? throw new ArgumentNullException(nameof(frontStream));
			BackStream = backStream ?? throw new ArgumentNullException(nameof(backStream));
		}

		public void ConnectAsServer(Action handshakeFailureHandler)
		{
			if (!IsServer || Connected) throw new InvalidOperationException();
			if ((HandshakeHandler != null) && HandshakeHandler())
				Connected = true;
			else handshakeFailureHandler?.Invoke();
		}
		public bool ConnectAsClient()
		{
			if (IsServer || Connected) throw new InvalidOperationException();
			if ((HandshakeHandler != null) && HandshakeHandler())
				Connected = true;
			return Connected;
		}

		public void Disconnect()
		{
			if (!Connected) throw new InvalidOperationException();
			Connected = false;
			DisconnectionHandler?.Invoke();
		}

		public void AddHook(Action handler)
		{
			if (handler == null) throw new ArgumentNullException(nameof(handler));
			Task.Run(() => {
				while (Available == 0)
					Thread.Sleep(5);
				handler();
			});
		}

		public static FakeNetMember MakeConnected(Action failHandler, ForkStream frontStream, ForkStream backStream)
		{
			var net = new FakeNetMember(frontStream, backStream);
			SetupAsConnected(net, failHandler);
			return net;
		}
		public static FakeNetMember MakeConnected(Action failHandler)
		{
			var net = new FakeNetMember();
			SetupAsConnected(net, failHandler);
			return net;
		}
		private static void SetupAsConnected(FakeNetMember net, Action failHandler)
		{
			net.Connected = true;
			net.DisconnectionHandler = failHandler;
			net.HandshakeHandler = () => { failHandler(); return false; };
		}
	}
}
