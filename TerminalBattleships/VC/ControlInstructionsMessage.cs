using System;

namespace TerminalBattleships.VC
{
	public class ControlInstructionsMessage
	{
		private static readonly string[] messages = new string[] {
			"[Arrows:move cursor|Z:set ship|X:complete]",
			"[Arrows:move cursor|Z:fire]",
			"Just wait and receive shots!",
		};

		public byte X { get; }
		public byte Y { get; }

		public ControlInstructionsMessage(byte x, byte y)
		{
			X = x;
			Y = y;
		}

		public void Show(Situation situation)
		{
			Console.SetCursorPosition(X, Y);
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write(messages[(byte)situation]);
		}
		public void Hide(Situation situation)
		{
			Console.MoveBufferArea(X, Y, messages[(byte)situation].Length, 1, Console.BufferWidth, 0);
		}

		public enum Situation
		{
			FleetBuilding = 0,
			OwnTurn = 1,
			FoeTurn = 2,
		}
	}
}
