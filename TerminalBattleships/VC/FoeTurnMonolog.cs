using System;
using TerminalBattleships.Model;
using TerminalBattleships.Network;

namespace TerminalBattleships.VC
{
	class FoeTurnMonolog
	{
		private FoeTurnResponser foeTurnResponser;
		private Game game;
		private GridV ownGridV, foeGridV;

		public FoeTurnMonolog(NetMember net, Game game, GridV ownGridV, GridV foeGridV)
		{
			foeTurnResponser = new FoeTurnResponser(net, game.ReceiveShot);
			this.game = game;
			this.ownGridV = ownGridV ?? throw new ArgumentNullException(nameof(ownGridV));
			this.foeGridV = foeGridV ?? throw new ArgumentNullException(nameof(foeGridV));
		}

		public void Show()
		{
			foeGridV.DrawLabel(true);
			while (!game.IsOwnTurn && (game.OwnIntactShipCount > 0))
			{
				foeTurnResponser.ReceiveShot(out Coord target, out FireResult fireResult);
				ownGridV.DrawGridTile(target);
			}
			foeGridV.DrawLabel(false);
		}
	}
}
