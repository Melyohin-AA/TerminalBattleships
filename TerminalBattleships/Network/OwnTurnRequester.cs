using System;
using System.Net.Sockets;
using TerminalBattleships.Model;

namespace TerminalBattleships.Network
{
	public class OwnTurnRequester
	{
		private NetMember net;

		public OwnTurnRequester(NetMember net)
		{
			this.net = net ?? throw new ArgumentNullException(nameof(net));
		}

		public FireResult Fire(Coord target)
		{
			net.Stream.WriteByte(target.IJ);
			net.Stream.Flush();
			int response = net.Stream.ReadByte();
			if (response == -1) throw new SocketException();
			return (FireResult)response;
		}
	}
}
