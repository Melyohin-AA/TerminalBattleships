using System;
using TerminalBattleships.Model;

namespace TerminalBattleships.Network
{
	public class FoeTurnResponser
	{
		private readonly INetMember net;
		private readonly Func<Coord, FireResult> shotHandler;

		public FoeTurnResponser(INetMember net, Func<Coord, FireResult> shotHandler)
		{
			this.net = net ?? throw new ArgumentNullException(nameof(net));
			this.shotHandler = shotHandler ?? throw new ArgumentNullException(nameof(shotHandler));
		}

		public void ReceiveShot(out Coord target, out FireResult fireResult)
		{
			while (net.Available == 0)
				System.Threading.Thread.Sleep(5);
			byte request = net.ReadByte();
			target = new Coord(request);
			fireResult = shotHandler(target);
			net.Stream.WriteByte((byte)fireResult);
			net.Stream.Flush();
		}
	}
}
