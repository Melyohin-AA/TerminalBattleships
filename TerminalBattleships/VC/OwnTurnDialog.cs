using System;
using TerminalBattleships.Model;
using TerminalBattleships.Network;

namespace TerminalBattleships.VC
{
	class OwnTurnDialog
	{
		private OwnTurnRequester ownTurnRequester;
		private Game game;
		private GridV ownGridV, foeGridV;
		private GridC foeGridC;
		private FireResult fireResult;

		public OwnTurnDialog(NetMember net, Game game, GridV ownGridV, GridV foeGridV)
		{
			ownTurnRequester = new OwnTurnRequester(net);
			this.game = game ?? throw new ArgumentNullException(nameof(game));
			this.ownGridV = ownGridV ?? throw new ArgumentNullException(nameof(ownGridV));
			this.foeGridV = foeGridV ?? throw new ArgumentNullException(nameof(foeGridV));
		}

		public void Show()
		{
			Program.DisposeKeys();
			ownGridV.DrawLabel(true);
			foeGridC = new GridC(foeGridV.GridX, foeGridV.GridY);
			while (game.IsOwnTurn && (game.FoeIntactShipCount > 0))
			{
				foeGridC.Control(null, FireControl);
				if (fireResult == FireResult.Error400) throw new Exception("!400!");
			}
			ownGridV.DrawLabel(false);
		}
		private void FireControl()
		{
			if (game.FoeGrid[foeGridC.Cursor] != GridTile.Uncertainty) return;
			fireResult = ownTurnRequester.Fire(foeGridC.Cursor);
			game.Fire(foeGridC.Cursor, fireResult, foeGridV.DrawGridTile);
			foeGridV.DrawGridTile(foeGridC.Cursor);
		}
	}
}
