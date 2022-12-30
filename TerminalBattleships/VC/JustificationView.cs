using System;
using TerminalBattleships.Model;
using TerminalBattleships.Network;

namespace TerminalBattleships.VC
{
	class JustificationView
	{
		private Justification justification;
		private Grid foeGrid;
		private GridV foeGridV;

		public bool IsFoeCheater { get; private set; }

		public JustificationView(Justification justification, Grid foeGrid, GridV foeGridV)
		{
			this.justification = justification ?? throw new ArgumentNullException(nameof(justification));
			this.foeGrid = foeGrid ?? throw new ArgumentNullException(nameof(foeGrid));
			this.foeGridV = foeGridV ?? throw new ArgumentNullException(nameof(foeGridV));
		}

		public void Show()
		{
			justification.SharePrivateKeys();
			justification.SendOwnShipOpenCoords();
			justification.ReceiveFoeShipOpenCoords();
			IsFoeCheater = justification.IsFoeCheater(foeGrid);
			foeGridV.DrawGrid();
			if (IsFoeCheater)
			{
				Console.BackgroundColor = ConsoleColor.Red;
				Console.ForegroundColor = ConsoleColor.White;
				Console.SetCursorPosition(foeGridV.X + GridV.LabelX - 1, foeGridV.Y + GridV.LabelY);
				Console.Write("!CHEATER!");
				Console.BackgroundColor = ConsoleColor.Black;
			}
		}
	}
}
