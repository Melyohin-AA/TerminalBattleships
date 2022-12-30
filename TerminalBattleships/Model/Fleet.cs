using System;

namespace TerminalBattleships.Model
{
	public class Fleet
	{
		public Grid Grid { get; }
		public RankedSet[] RankedSetShips { get; }
		public byte ShipCount { get; private set; }
		public bool CorrectStructers { get; private set; }

		public Fleet(Grid grid)
		{
			Grid = grid ?? throw new ArgumentNullException(nameof(grid));
			RankedSetShips = new[]
			{
				new RankedSet(1, 1),
				new RankedSet(2, 3),
				new RankedSet(3, 5),
				new RankedSet(4, 3),
				new RankedSet(5, 1),
			};
		}

		public void Scan()
		{
			CorrectStructers = true;
			for (byte i = 0; i < RankedSetShips.Length; i++)
				RankedSetShips[i].CurrentCount = 0;
			var checkedTiles = new bool[256];
			for (short ij = 0; ij < 256; ij++)
			{
				if (checkedTiles[ij]) continue;
				checkedTiles[ij] = true;
				switch (Grid[ij])
				{
					case GridTile.Uncertainty:
					case GridTile.IntactWater:
					case GridTile.ShotWater: continue;
					case GridTile.IntactShip:
					case GridTile.DamagedShip: break;
					default: throw new Exception();
				}
				var coord = new Coord((byte)ij);
				DetDirs(coord, checkedTiles, out bool upShip, out bool downShip, out bool leftShip, out bool rightShip);
				if (!upShip && !downShip && !leftShip && !rightShip)
				{
					RankedSetShips[0].CurrentCount++;
					continue;
				}
				bool vertical = upShip || downShip;
				bool horizontal = leftShip || rightShip;
				if (vertical && horizontal)
				{
					CorrectStructers = false;
					continue;
				}
				if (vertical)
				{
					byte size = DetShipSizeDown(coord, checkedTiles);
					if (upShip) size++;
					if (size <= RankedSetShips.Length)
						RankedSetShips[size - 1].CurrentCount++;
					else CorrectStructers = false;
					continue;
				}
				if (horizontal)
				{
					byte size = DetShipSizeRight(coord, checkedTiles);
					if (leftShip) size++;
					if (size <= RankedSetShips.Length)
						RankedSetShips[size - 1].CurrentCount++;
					else CorrectStructers = false;
					continue;
				}
				throw new Exception();
			}
			CountShips();
		}

		private void CheckForXCollisions(Coord coord)
		{
			if (!CorrectStructers) return;
			if (coord.I > 0)
			{
				if (((coord.J > 0) && Grid[coord.UpLeft].IsShip()) ||
					((coord.J < 15) && Grid[coord.UpRight].IsShip()))
				{
					CorrectStructers = false;
					return;
				}
			}
			if (coord.I < 15)
			{
				if (((coord.J > 0) && Grid[coord.DownLeft].IsShip()) ||
					((coord.J < 15) && Grid[coord.DownRight].IsShip()))
					CorrectStructers = false;
			}
		}

		private void DetDirs(Coord coord, bool[] checkedTiles,
			out bool upShip, out bool downShip, out bool leftShip, out bool rightShip)
		{
			if (coord.I > 0)
			{
				var up = coord.Up;
				checkedTiles[up.IJ] = true;
				upShip = Grid[up].IsShip();
				if (upShip) CheckForXCollisions(up);
			}
			else upShip = false;
			if (coord.I < 15)
			{
				var down = coord.Down;
				checkedTiles[down.IJ] = true;
				downShip = Grid[down].IsShip();
			}
			else downShip = false;
			if (coord.J > 0)
			{
				var left = coord.Left;
				checkedTiles[left.IJ] = true;
				leftShip = Grid[left].IsShip();
				if (leftShip) CheckForXCollisions(left);
			}
			else leftShip = false;
			if (coord.J < 15)
			{
				var right = coord.Right;
				checkedTiles[right.IJ] = true;
				rightShip = Grid[right].IsShip();
			}
			else rightShip = false;
		}

		private byte DetShipSizeDown(Coord origin, bool[] checkedTiles)
		{
			byte size = 0;
			for (byte i = origin.I; i < 16; i++)
			{
				var coord = new Coord(i, origin.J);
				checkedTiles[coord.IJ] = true;
				if (!Grid[coord].IsShip()) break;
				CheckForXCollisions(coord);
				size++;
			}
			return size;
		}
		private byte DetShipSizeRight(Coord origin, bool[] checkedTiles)
		{
			byte size = 0;
			for (byte j = origin.J; j < 16; j++)
			{
				var coord = new Coord(origin.I, j);
				checkedTiles[coord.IJ] = true;
				if (!Grid[coord].IsShip()) break;
				CheckForXCollisions(coord);
				size++;
			}
			return size;
		}

		private void CountShips()
		{
			ShipCount = 0;
			foreach (GridTile tile in Grid.Tiles)
				if (tile.IsShip())
					ShipCount++;
		}

		public struct RankedSet
		{
			public byte Rank { get; }
			public byte RequiredCount { get; }

			public byte CurrentCount { get; set; }

			public RankedSet(byte rank, byte requiredCount, byte currentCount = 0)
			{
				Rank = rank;
				RequiredCount = requiredCount;
				CurrentCount = currentCount;
			}
		}
	}
}
