using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TerminalBattleships.Network;

namespace TerminalBattleships_Testing.Network
{
	class MockNetMember : INetMember
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

		public MockNetMember()
		{
			Queue<byte> a = new Queue<byte>(), b = new Queue<byte>();
			FrontStream = new ForkStream(a, b);
			BackStream = new ForkStream(b, a);
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

		public static MockNetMember MakeConnected(Action failHandler)
		{
			return new MockNetMember {
				Connected = true,
				DisconnectionHandler = failHandler,
				HandshakeHandler = () => { failHandler(); return false; },
			};
		}
	}
}
