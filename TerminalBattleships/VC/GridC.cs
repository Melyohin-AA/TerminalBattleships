using System;
using TerminalBattleships.Model;

namespace TerminalBattleships.VC
{
	class GridC
	{
		public byte X { get; }
		public byte Y { get; }
		public Coord Cursor { get; private set; }

		public GridC(byte x, byte y)
		{
			X = x;
			Y = y;
		}

		public void Control(Action z, Action x)
		{
			Console.SetCursorPosition(X + Cursor.J, Y + Cursor.I);
			switch (Console.ReadKey(true).Key)
			{
				case ConsoleKey.UpArrow:
					if (Cursor.I > 0) Cursor = Cursor.Up;
					break;
				case ConsoleKey.DownArrow:
					if (Cursor.I < 15) Cursor = Cursor.Down;
					break;
				case ConsoleKey.LeftArrow:
					if (Cursor.J > 0) Cursor = Cursor.Left;
					break;
				case ConsoleKey.RightArrow:
					if (Cursor.J < 15) Cursor = Cursor.Right;
					break;
				case ConsoleKey.Home:
					Cursor = new Coord(Cursor.I, 0);
					break;
				case ConsoleKey.End:
					Cursor = new Coord(Cursor.I, 15);
					break;
				case ConsoleKey.PageUp:
					Cursor = new Coord(0, Cursor.J);
					break;
				case ConsoleKey.PageDown:
					Cursor = new Coord(15, Cursor.J);
					break;
				case ConsoleKey.Z:
					Console.SetCursorPosition(X + Cursor.J, Y + Cursor.I);
					z?.Invoke();
					break;
				case ConsoleKey.X:
					Console.SetCursorPosition(X + Cursor.J, Y + Cursor.I);
					x?.Invoke();
					break;
			}
			Console.SetCursorPosition(X + Cursor.J, Y + Cursor.I);
		}
	}
}
