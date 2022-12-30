using System;

namespace TerminalBattleships.VC
{
	class GameSessionResultMessage
	{
		public byte X { get; }
		public byte Y { get; }
		public bool Victory { get; }

		public GameSessionResultMessage(byte x, byte y, bool victory)
		{
			X = x;
			Y = y;
			Victory = victory;
		}

		public void Show()
		{
			Console.SetCursorPosition(X, Y);
			if (Victory)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write("-=VICTORY=-");
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.Write("-=DEFEAT=-");
			}
		}
	}
}
