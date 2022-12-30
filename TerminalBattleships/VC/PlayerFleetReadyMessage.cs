using System;

namespace TerminalBattleships.VC
{
	class PlayerFleetReadyMessage
	{
		private GridV gridV;

		public PlayerFleetReadyMessage(GridV gridV)
		{
			this.gridV = gridV ?? throw new ArgumentNullException(nameof(gridV));
		}

		public void Show()
		{
			int clWas = Console.CursorLeft, ctWas = Console.CursorTop;
			Console.SetCursorPosition(gridV.X + GridV.LabelX, gridV.GridY + 17);
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write("[");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write("READY");
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write("]");
			Console.SetCursorPosition(clWas, ctWas);
		}

		public void Hide()
		{
			Console.MoveBufferArea(gridV.X + GridV.LabelX, gridV.GridY + 17, 7, 1, Console.BufferWidth, 0);
		}
	}
}
