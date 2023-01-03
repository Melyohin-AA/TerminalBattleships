using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TerminalBattleships.Model;

namespace TerminalBattleships_Testing.Model
{
	[TestClass]
	public class Grid_UnitTest
	{
		[TestMethod]
		public void InBoundsInt16_I_ValidRangeBottom()
		{
			Assert.IsTrue(Grid.IsInBounds((short)0, (short)9));
		}
		[TestMethod]
		public void InBoundsInt16_I_ValidRangeTop()
		{
			Assert.IsTrue(Grid.IsInBounds((short)15, (short)9));
		}
		[TestMethod]
		public void InBoundsInt16_I_InvalidRangeLower()
		{
			Assert.IsFalse(Grid.IsInBounds((short)-1, (short)9));
		}
		[TestMethod]
		public void InBoundsInt16_I_InvalidRangeUpper()
		{
			Assert.IsFalse(Grid.IsInBounds((short)16, (short)9));
		}
		[TestMethod]
		public void InBoundsInt16_J_ValidRangeBottom()
		{
			Assert.IsTrue(Grid.IsInBounds((short)9, (short)0));
		}
		[TestMethod]
		public void InBoundsInt16_J_ValidRangeTop()
		{
			Assert.IsTrue(Grid.IsInBounds((short)9, (short)15));
		}
		[TestMethod]
		public void InBoundsInt16_J_InvalidRangeLower()
		{
			Assert.IsFalse(Grid.IsInBounds((short)9, (short)-1));
		}
		[TestMethod]
		public void InBoundsInt16_J_InvalidRangeUpper()
		{
			Assert.IsFalse(Grid.IsInBounds((short)9, (short)16));
		}

		[TestMethod]
		public void InBoundsUInt8_I_ValidRangeTop()
		{
			Assert.IsTrue(Grid.IsInBounds((byte)15, (byte)9));
		}
		[TestMethod]
		public void InBoundsUInt8_I_InvalidRangeUpper()
		{
			Assert.IsFalse(Grid.IsInBounds((byte)16, (byte)9));
		}
		[TestMethod]
		public void InBoundsUInt8_J_ValidRangeTop()
		{
			Assert.IsTrue(Grid.IsInBounds((byte)9, (byte)15));
		}
		[TestMethod]
		public void InBoundsUInt8_J_InvalidRangeUpper()
		{
			Assert.IsFalse(Grid.IsInBounds((byte)9, (byte)16));
		}

		[TestMethod]
		public void MakeOwnGrid()
		{
			Grid grid = Grid.MakeOwnGrid();
			Assert.AreEqual(256, grid.Tiles.Length);
			for (short ij = 0; ij < 256; ij++)
				Assert.AreEqual(GridTile.IntactWater, grid.Tiles[ij]);
		}
		[TestMethod]
		public void MakeFoeGrid()
		{
			Grid grid = Grid.MakeFoeGrid();
			Assert.AreEqual(256, grid.Tiles.Length);
			for (short ij = 0; ij < 256; ij++)
				Assert.AreEqual(GridTile.Uncertainty, grid.Tiles[ij]);
		}

		[TestMethod]
		public void IndexingInt16_Get()
		{
			const byte ij = 123;
			Grid grid = Grid.MakeOwnGrid();
			grid.Tiles[ij] = GridTile.ShotWater;
			Assert.AreEqual(GridTile.ShotWater, grid[(short)ij]);
		}
		[TestMethod]
		public void IndexingInt16_Set()
		{
			const byte ij = 123;
			Grid grid = Grid.MakeOwnGrid();
			grid[(short)ij] = GridTile.ShotWater;
			Assert.AreEqual(GridTile.ShotWater, grid.Tiles[ij]);
		}

		[TestMethod]
		public void IndexingUInt8_Get()
		{
			const byte ij = 123;
			Grid grid = Grid.MakeOwnGrid();
			grid.Tiles[ij] = GridTile.ShotWater;
			Assert.AreEqual(GridTile.ShotWater, grid[(byte)ij]);
		}
		[TestMethod]
		public void IndexingUInt8_Set()
		{
			const byte ij = 123;
			Grid grid = Grid.MakeOwnGrid();
			grid[(byte)ij] = GridTile.ShotWater;
			Assert.AreEqual(GridTile.ShotWater, grid.Tiles[ij]);
		}

		[TestMethod]
		public void IndexingCoord_Get()
		{
			const byte ij = 123;
			Grid grid = Grid.MakeOwnGrid();
			var coord = new Coord(ij);
			grid.Tiles[ij] = GridTile.ShotWater;
			Assert.AreEqual(GridTile.ShotWater, grid[coord]);
		}
		[TestMethod]
		public void IndexingCoord_Set()
		{
			const byte ij = 123;
			Grid grid = Grid.MakeOwnGrid();
			var coord = new Coord(ij);
			grid[coord] = GridTile.ShotWater;
			Assert.AreEqual(GridTile.ShotWater, grid.Tiles[ij]);
		}

		[TestMethod]
		public void IndexingUInt8Pair_Get()
		{
			const byte ij = 123;
			Grid grid = Grid.MakeOwnGrid();
			var coord = new Coord(ij);
			grid.Tiles[ij] = GridTile.ShotWater;
			Assert.AreEqual(GridTile.ShotWater, grid[coord.I, coord.J]);
		}
		[TestMethod]
		public void IndexingUInt8Pair_Set()
		{
			const byte ij = 123;
			Grid grid = Grid.MakeOwnGrid();
			var coord = new Coord(ij);
			grid[coord.I, coord.J] = GridTile.ShotWater;
			Assert.AreEqual(GridTile.ShotWater, grid.Tiles[ij]);
		}

		private void VerifyGetShipCoords(Coord[] expected, GridTile setTile)
		{
			Grid grid = Grid.MakeOwnGrid();
			foreach (Coord coord in expected)
				grid[coord] = setTile;
			Coord[] actual = grid.GetShipCoords();
			Assert.AreEqual(expected.Length, actual.Length);
			for (byte i = 0; i < expected.Length; i++)
				Assert.AreEqual(expected[i], actual[i]);
		}

		[TestMethod]
		public void GetShipCoords_IntactShips()
		{
			VerifyGetShipCoords(new[] { new Coord(96), new Coord(123), new Coord(214) }, GridTile.IntactShip);
		}
		[TestMethod]
		public void GetShipCoords_DamagedShips()
		{
			VerifyGetShipCoords(new[] { new Coord(96), new Coord(123), new Coord(214) }, GridTile.DamagedShip);
		}
	}
}
