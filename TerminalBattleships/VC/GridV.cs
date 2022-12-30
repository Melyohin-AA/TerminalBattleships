using System;
using TerminalBattleships.Model;

namespace TerminalBattleships.VC
{
	class GridV
	{
		public const byte RegionX = 0, RegionY = 0, RegionW = 18, RegionH = 18;
		public const byte LabelX = 6, LabelY = 0;
		public const ConsoleColor LabelFColor = ConsoleColor.White;
		public const byte FrameX = 0, FrameY = 2;
		public const ConsoleColor FrameFColor = ConsoleColor.White;

		public byte X { get; }
		public byte Y { get; }
		public Grid Grid { get; }
		public bool IsOwn { get; }

		public byte GridX => (byte)(X + FrameX + 2);
		public byte GridY => (byte)(Y + FrameY + 1);

		public GridV(byte x, byte y, Grid grid, bool isOwn)
		{
			X = x;
			Y = y;
			Grid = grid ?? throw new ArgumentNullException(nameof(grid));
			IsOwn = isOwn;
		}

		public void DrawAll()
		{
			ClearRegion();
			DrawLabelWings();
			DrawFrame();
			DrawLabel();
			DrawGrid();
		}

		public void DrawLabel(bool invertColors = false)
		{
			Console.SetCursorPosition(X + LabelX + 2, Y + LabelY);
			if (invertColors)
			{
				Console.ForegroundColor = ConsoleColor.Black;
				Console.BackgroundColor = LabelFColor;
			}
			else Console.ForegroundColor = LabelFColor;
			Console.Write(IsOwn ? "OWN" : "FOE");
			Console.BackgroundColor = ConsoleColor.Black;
		}

		public void DrawGrid()
		{
			Console.SetCursorPosition(GridX, GridY);
			for (short ij = 0; ij < 256; ij++)
			{
				DrawGridTile(Grid[ij]);
				if ((ij & 15) == 15)
				{
					Console.CursorLeft = GridX;
					Console.CursorTop += 1;
				}
			}
		}
		public void DrawGridTile(Coord coord)
		{
			Console.SetCursorPosition(GridX + coord.J, GridY + coord.I);
			DrawGridTile(Grid[coord]);
		}
		public static void DrawGridTile(GridTile tile)
		{
			switch (tile)
			{
				case GridTile.Uncertainty:
					Console.ForegroundColor = ConsoleColor.DarkMagenta;
					Console.Write('?');
					break;
				case GridTile.IntactWater:
					Console.ForegroundColor = ConsoleColor.DarkCyan;
					Console.Write('.');
					break;
				case GridTile.ShotWater:
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.Write('*');
					break;
				case GridTile.IntactShip:
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Write('#');
					break;
				case GridTile.DamagedShip:
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Write('#');
					break;
				default: throw new Exception();
			}
		}

		public void ClearRegion()
		{
			Console.MoveBufferArea(X + RegionX, Y + RegionY, RegionW, RegionH, Console.BufferWidth, 0);
		}

		public void DrawLabelWings()
		{
			Console.SetCursorPosition(X + LabelX, Y + LabelY);
			Console.ForegroundColor = LabelFColor;
			Console.Write("-=");
			Console.CursorLeft += 3;
			Console.Write("=-");
		}

		public void DrawFrame()
		{
			const string hex = "0123456789ABCDEF";
			Console.SetCursorPosition(X + FrameX, Y + FrameY);
			Console.ForegroundColor = FrameFColor;
			Console.WriteLine($"  {hex}");
			foreach (char digit in hex)
			{
				Console.CursorLeft = X + FrameX;
				Console.WriteLine($"{digit}0");
			}
		}
	}
}
