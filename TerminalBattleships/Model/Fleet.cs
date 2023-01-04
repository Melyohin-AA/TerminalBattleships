using System;
using System.IO;

namespace TerminalBattleships.Model
{
	public class Fleet
	{
		public const string ConfigFilePath = "fleet.txt";

		public Grid Grid { get; }
		public RankedSet[] RankedSetShips { get; }
		public byte ShipCount { get; private set; }
		public bool CorrectStructers { get; private set; }
		public bool IsComplete { get; private set; }

		private static byte[] config = new byte[5] { 1, 3, 5, 3, 1 };
		
		public static void ReadConfigFromFile()
		{
			if (File.Exists(ConfigFilePath))
			{
				string[] lines = File.ReadAllLines(ConfigFilePath, System.Text.Encoding.UTF8);
				try
				{
					config = new byte[byte.Parse(lines[0])];
					if (lines.Length > 16)
					{
						WriteConfigToFile();
						return;
					}
					for (short i = 1; i < lines.Length; i++)
						config[i - 1] = byte.Parse(lines[i]);
				}
				catch (Exception)
				{
					WriteConfigToFile();
				}
			}
			else WriteConfigToFile();
		}
		public static void WriteConfigToFile()
		{
			var lines = new System.Collections.Generic.List<string>(config.Length + 1);
			lines.Add(config.Length.ToString());
			foreach (byte rankedShipRequiredCount in config)
				lines.Add(rankedShipRequiredCount.ToString());
			File.WriteAllLines(ConfigFilePath, lines, System.Text.Encoding.UTF8);
		}

		public static void SetConfig(byte[] config)
		{
			if (config.Length > 16) throw new ArgumentOutOfRangeException(nameof(config));
			Fleet.config = (byte[])config.Clone();
		}
		public static RankedSet[] MakeShipSets()
		{
			var shipSets = new RankedSet[config.Length];
			for (short i = 0; i < config.Length; i++)
				shipSets[i] = new RankedSet((byte)(i + 1), config[i]);
			return shipSets;
		}

		public Fleet(Grid grid)
		{
			Grid = grid ?? throw new ArgumentNullException(nameof(grid));
			RankedSetShips = MakeShipSets();
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
					if (!TryDetShipSizeDown(coord, checkedTiles, out byte size))
					{
						CorrectStructers = false;
						continue;
					}
					if (upShip) size++;
					if (size <= RankedSetShips.Length)
						RankedSetShips[size - 1].CurrentCount++;
					else CorrectStructers = false;
					continue;
				}
				if (horizontal)
				{
					if (!TryDetShipSizeRight(coord, checkedTiles, out byte size))
					{
						CorrectStructers = false;
						continue;
					}
					if (leftShip) size++;
					if (size <= RankedSetShips.Length)
						RankedSetShips[size - 1].CurrentCount++;
					else CorrectStructers = false;
					continue;
				}
				throw new Exception();
			}
			CountShips();
			DetIfIsComplete();
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

		private bool TryDetShipSizeDown(Coord origin, bool[] checkedTiles, out byte size)
		{
			size = 0;
			for (byte i = origin.I; i < 16; i++)
			{
				var coord = new Coord(i, origin.J);
				checkedTiles[coord.IJ] = true;
				if (!Grid[coord].IsShip()) break;
				CheckForXCollisions(coord);
				if (((origin.J > 0) && Grid[i, (byte)(origin.J - 1)].IsShip()) ||
					((origin.J < 15) && Grid[i, (byte)(origin.J + 1)].IsShip())) return false;
				size++;
			}
			return true;
		}
		private bool TryDetShipSizeRight(Coord origin, bool[] checkedTiles, out byte size)
		{
			size = 0;
			for (byte j = origin.J; j < 16; j++)
			{
				var coord = new Coord(origin.I, j);
				checkedTiles[coord.IJ] = true;
				if (!Grid[coord].IsShip()) break;
				CheckForXCollisions(coord);
				if (((origin.I > 0) && Grid[(byte)(origin.I - 1), j].IsShip()) ||
					((origin.I < 15) && Grid[(byte)(origin.I + 1), j].IsShip())) return false;
				size++;
			}
			return true;
		}

		private void CountShips()
		{
			ShipCount = 0;
			foreach (GridTile tile in Grid.Tiles)
				if (tile.IsShip())
					ShipCount++;
		}

		private void DetIfIsComplete()
		{
			IsComplete = false;
			if (!CorrectStructers) return;
			foreach (RankedSet set in RankedSetShips)
				if (set.CurrentCount != set.RequiredCount) return;
			IsComplete = true;
			return;
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
