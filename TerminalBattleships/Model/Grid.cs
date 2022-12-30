using System;
using System.Collections.Generic;

namespace TerminalBattleships.Model
{
	public class Grid
	{
		public GridTile[] Tiles { get; } = new GridTile[256];

		public GridTile this[short ij]
		{
			get => Tiles[ij];
			set => Tiles[ij] = value;
		}
		public GridTile this[byte ij]
		{
			get => Tiles[ij];
			set => Tiles[ij] = value;
		}
		public GridTile this[Coord coord]
		{
			get => Tiles[coord.IJ];
			set => Tiles[coord.IJ] = value;
		}
		public GridTile this[byte i, byte j]
		{
			get => Tiles[new Coord(i, j).IJ];
			set => Tiles[new Coord(i, j).IJ] = value;
		}

		public static Grid MakeOwnGrid()
		{
			var grid = new Grid();
			for (short ij = 0; ij < 256; ij++)
				grid.Tiles[ij] = GridTile.IntactWater;
			return grid;
		}
		public static Grid MakeFoeGrid()
		{
			var grid = new Grid();
			for (short ij = 0; ij < 256; ij++)
				grid[ij] = GridTile.Uncertainty;
			return grid;
		}

		public static bool IsInBounds(short i, short j)
		{
			return (i >= 0) && (i < 16) && (j >= 0) && (j < 16);
		}
		public static bool IsInBounds(byte i, byte j)
		{
			return (i < 16) && (j < 16);
		}

		public Coord[] GetShipCoords()
		{
			var coords = new List<Coord>(64);
			for (short ij = 0; ij < 256; ij++)
				if (Tiles[ij].IsShip())
					coords.Add(new Coord((byte)ij));
			return coords.ToArray();
		}
	}
}
