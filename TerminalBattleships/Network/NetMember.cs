using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace TerminalBattleships.Network
{
	public interface INetMember
	{
		bool Connected { get; }
		bool IsServer { get; }
		EndPoint LocalEP { get; }
		Stream Stream { get; }
		int Available { get; }

		void ConnectAsServer(Action handshakeFailureHandler);
		bool ConnectAsClient();

		void Disconnect();

		void AddHook(Action handler);
	}

	public class NetMember : INetMember
	{
		private const byte serverHandshakeCode = 144, clientHandshakeCode = 169;

		private IPEndPoint serverEP;
		private TcpListener listener;
		private TcpClient client;

		public bool Connected { get; private set; }
		public bool IsServer { get; }
		public EndPoint LocalEP => IsServer ? listener.LocalEndpoint : null;
		public Stream Stream => client.GetStream();
		public int Available => client.Available;

		private NetMember(TcpListener listener)
		{
			IsServer = true;
			this.listener = listener;
			listener.Start();
		}
		private NetMember(TcpClient client, IPEndPoint serverEP)
		{
			IsServer = false;
			this.client = client;
			this.serverEP = serverEP;
		}

		public void ConnectAsServer(Action handshakeFailureHandler)
		{
			if (!IsServer || Connected) throw new InvalidOperationException();
			while (client == null)
			{
				if (!listener.Pending())
				{
					Thread.Sleep(5);
					continue;
				}
				TcpClient client = listener.AcceptTcpClient();
				if (TryHandshakeAsServer(client))
				{
					this.client = client;
					Connected = true;
					return;
				}
				else handshakeFailureHandler?.Invoke();
			}
		}
		public bool ConnectAsClient()
		{
			if (IsServer || Connected) throw new InvalidOperationException();
			client.Connect(serverEP.Address, serverEP.Port);
			if (TryHandshakeAsClient())
			{
				Connected = true;
				return true;
			}
			client.Close();
			return false;
		}

		public static NetMember StartServer(ushort port = 0)
		{
			var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
			return new NetMember(listener);
		}
		public static NetMember StartClient(IPEndPoint serverEP, ushort port = 0)
		{
			var client = new TcpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
			return new NetMember(client, serverEP);
		}

		private bool TryHandshakeAsServer(TcpClient client)
		{
			var stream = client.GetStream();
			stream.WriteByte(serverHandshakeCode);
			stream.Flush();
			if (stream.ReadByte() != clientHandshakeCode) return false;
			return true;
		}
		private bool TryHandshakeAsClient()
		{
			var stream = client.GetStream();
			stream.Flush();
			if (stream.ReadByte() != serverHandshakeCode) return false;
			stream.WriteByte(clientHandshakeCode);
			return true;
		}

		public void Disconnect()
		{
			if (!Connected) throw new InvalidOperationException();
			client.Close();
			if (IsServer) listener.Stop();
			Connected = false;
		}

		public void AddHook(Action handler)
		{
			if (handler == null) throw new ArgumentNullException(nameof(handler));
			Task.Run(() => {
				while (client.Available == 0)
					Thread.Sleep(5);
				handler();
			});
		}
	}
}
