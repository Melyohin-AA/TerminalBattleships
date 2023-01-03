using System;
using TerminalBattleships.Model;

namespace TerminalBattleships.Network
{
	public class ShipSetsAgreement
	{
		private readonly INetMember net;

		public ShipSetsAgreement(INetMember net)
		{
			this.net = net ?? throw new ArgumentNullException(nameof(net));
		}

		public bool TryAgree()
		{
			Fleet.RankedSet[] ownShipSets = Fleet.MakeShipSets();
			SendOwnShipSets(ownShipSets);
			Fleet.RankedSet[] foeShipSets = ReceiveFoeShipSets();
			if (ownShipSets.Length != foeShipSets.Length) return false;
			for (short i = 0; i < ownShipSets.Length; i++)
				if ((ownShipSets[i].Rank != foeShipSets[i].Rank) ||
					ownShipSets[i].RequiredCount != foeShipSets[i].RequiredCount) return false;
			return true;
		}
		private void SendOwnShipSets(Fleet.RankedSet[] ownShipSets)
		{
			if (ownShipSets.Length > 255) throw new ArgumentOutOfRangeException(nameof(ownShipSets));
			net.Stream.WriteByte((byte)ownShipSets.Length);
			foreach (Fleet.RankedSet set in ownShipSets)
			{
				net.Stream.WriteByte(set.Rank);
				net.Stream.WriteByte(set.RequiredCount);
			}
			net.Stream.Flush();
		}
		private Fleet.RankedSet[] ReceiveFoeShipSets()
		{
			byte count = net.ReadByte();
			var foeShipSets = new Fleet.RankedSet[count];
			var buffer = new byte[2];
			for (short i = 0; i < count; i++)
			{
				net.Stream.Read(buffer, 0, 2);
				foeShipSets[i] = new Fleet.RankedSet(buffer[0], buffer[1]);
			}
			return foeShipSets;
		}
	}
}
