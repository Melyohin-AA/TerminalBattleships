using System;
using TerminalBattleships.Model;

namespace TerminalBattleships.Network
{
	public class OwnTurnRequester
	{
		private readonly INetMember net;

		public OwnTurnRequester(INetMember net)
		{
			this.net = net ?? throw new ArgumentNullException(nameof(net));
		}

		public FireResult Fire(Coord target)
		{
			net.Stream.WriteByte(target.IJ);
			net.Stream.Flush();
			byte response = net.ReadByte();
			return (FireResult)response;
		}
	}
}
