using System;
using System.Net.Sockets;
using TerminalBattleships.Model;

namespace TerminalBattleships.Network
{
	public class FoeTurnResponser
	{
		private NetMember net;
		private Func<Coord, FireResult> shotHandler;

		public FoeTurnResponser(NetMember net, Func<Coord, FireResult> shotHandler)
		{
			this.net = net ?? throw new ArgumentNullException(nameof(net));
			this.shotHandler = shotHandler ?? throw new ArgumentNullException(nameof(shotHandler));
		}

		public void ReceiveShot(out Coord target, out FireResult fireResult)
		{
			while (net.Available == 0)
				System.Threading.Thread.Sleep(5);
			int request = net.Stream.ReadByte();
			if (request == -1) throw new SocketException();
			target = new Coord((byte)request);
			fireResult = shotHandler(target);
			net.Stream.WriteByte((byte)fireResult);
			net.Stream.Flush();
		}
	}
}
