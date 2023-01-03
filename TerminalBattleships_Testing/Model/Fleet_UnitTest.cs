using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TerminalBattleships.Model;

namespace TerminalBattleships_Testing.Model
{
	[TestClass]
	public class Fleet_UnitTest
	{
		private static Random random = new Random();

		// Hoping fleet config reading and writing work fine.

		private void VerifyShipSets(Fleet.RankedSet[] shipSets)
		{
			Assert.IsTrue(shipSets.Length <= 16);
			for (short i = 0; i < shipSets.Length; i++)
			{
				Assert.AreEqual((byte)(i + 1), shipSets[i].Rank);
				Assert.AreEqual((byte)0, shipSets[i].CurrentCount);
			}
		}

		[TestMethod]
		public void MakeShipSets()
		{
			VerifyShipSets(Fleet.MakeShipSets());
		}

		[TestMethod]
		public void SetConfig_ValidRangeTop()
		{
			Fleet.SetConfig(new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6 });
			VerifyShipSets(Fleet.MakeShipSets());
		}
		[TestMethod]
		public void SetConfig_InvalidRangeUpper()
		{
			Action act = () => Fleet.SetConfig(new byte[17] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7 });
			Assert.ThrowsException<ArgumentOutOfRangeException>(act);
		}
		[TestMethod]
		public void SetConfig_InvalidParam()
		{
			Action act = () => Fleet.SetConfig(null);
			Assert.ThrowsException<NullReferenceException>(act);
		}

		[TestMethod]
		public void Constructor_ValidParam()
		{
			Fleet.RankedSet[] shipSets = Fleet.MakeShipSets();
			var grid = Grid.MakeOwnGrid();
			var fleet = new Fleet(grid);
			Assert.AreEqual(grid, fleet.Grid);
			Assert.AreEqual(shipSets.Length, fleet.RankedSetShips.Length);
			for (short i = 0; i < shipSets.Length; i++)
				Assert.AreEqual(shipSets[i], fleet.RankedSetShips[i]);
		}
		[TestMethod]
		public void Constructor_InvalidParam()
		{
			Assert.ThrowsException<ArgumentNullException>(() => new Fleet(null));
		}

		private void SetRightRankedShip(byte rank, Grid grid, byte i, byte j0)
		{
			for (byte j = j0; j < j0 + rank; j++)
				grid[i, j] = GetRandomShip();
		}
		private void SetDownRankedShip(byte rank, Grid grid, byte i0, byte j)
		{
			for (byte i = i0; i < i0 + rank; i++)
				grid[i, j] = GetRandomShip();
		}
		private GridTile GetRandomShip()
		{
			return (random.Next(2) == 0) ? GridTile.IntactShip : GridTile.DamagedShip;
		}

		private void VerifyScanResult(Fleet fleet, bool correctnessStructureExpected, byte shipCountExpected,
			byte[] requiredCountExpected, byte[] currentCountExpected)
		{
			Assert.AreEqual(correctnessStructureExpected, fleet.CorrectStructers);
			Assert.AreEqual(shipCountExpected, fleet.ShipCount);
			for (byte i = 0; i < fleet.RankedSetShips.Length; i++)
			{
				Assert.AreEqual((byte)(i + 1), fleet.RankedSetShips[i].Rank);
				Assert.AreEqual(requiredCountExpected[i], fleet.RankedSetShips[i].RequiredCount);
				Assert.AreEqual(currentCountExpected[i], fleet.RankedSetShips[i].CurrentCount);
			}
		}

		[TestMethod]
		public void Scan_Valid_HorizontalShips()
		{
			var config = new byte[5] { 1, 3, 5, 3, 1 };
			Fleet.SetConfig(config);
			Grid grid = Grid.MakeOwnGrid();
			SetRightRankedShip(5, grid, 0, 0);//  #####.# 00
			SetRightRankedShip(1, grid, 0, 6);//  ....... 01
			SetRightRankedShip(4, grid, 2, 0);//  ####.## 02
			SetRightRankedShip(2, grid, 2, 5);//  ....... 03
			SetRightRankedShip(4, grid, 4, 0);//  ####.## 04
			SetRightRankedShip(2, grid, 4, 5);//  ....... 05
			SetRightRankedShip(4, grid, 6, 0);//  ####.## 06
			SetRightRankedShip(2, grid, 6, 5);//  ....... 07
			SetRightRankedShip(3, grid, 8, 0);//  ###.### 08
			SetRightRankedShip(3, grid, 8, 4);//  ....... 09
			SetRightRankedShip(3, grid, 10, 0);// ###.### 10
			SetRightRankedShip(3, grid, 10, 4);// ....... 11
			SetRightRankedShip(3, grid, 12, 0);// ###.... 12
			var fleet = new Fleet(grid);
			fleet.Scan();
			VerifyScanResult(fleet, true, 39, config, config);
		}
		[TestMethod]
		public void Scan_Valid_VerticalShips()
		{
			var config = new byte[5] { 1, 3, 5, 3, 1 };
			Fleet.SetConfig(config);
			Grid grid = Grid.MakeOwnGrid();
			SetDownRankedShip(5, grid, 0, 0);//  #####.# 00
			SetDownRankedShip(1, grid, 6, 0);//  ....... 01
			SetDownRankedShip(4, grid, 0, 2);//  ####.## 02
			SetDownRankedShip(2, grid, 5, 2);//  ....... 03
			SetDownRankedShip(4, grid, 0, 4);//  ####.## 04
			SetDownRankedShip(2, grid, 5, 4);//  ....... 05
			SetDownRankedShip(4, grid, 0, 6);//  ####.## 06
			SetDownRankedShip(2, grid, 5, 6);//  ....... 07
			SetDownRankedShip(3, grid, 0, 8);//  ###.### 08
			SetDownRankedShip(3, grid, 4, 8);//  ....... 09
			SetDownRankedShip(3, grid, 0, 10);// ###.### 10
			SetDownRankedShip(3, grid, 4, 10);// ....... 11
			SetDownRankedShip(3, grid, 0, 12);// ###.... 12
			var fleet = new Fleet(grid);
			fleet.Scan();
			VerifyScanResult(fleet, true, 39, config, config);
		}
		[TestMethod]
		public void Scan_InvalidFleetComposition_Disbalance()
		{
			var config = new byte[5] { 1, 3, 5, 3, 1 };
			var current = new byte[5] { 1, 4, 3, 4, 1 };
			Fleet.SetConfig(config);
			Grid grid = Grid.MakeOwnGrid();
			SetRightRankedShip(5, grid, 0, 0);//  #####.# 00
			SetRightRankedShip(1, grid, 0, 6);//  ....... 01
			SetRightRankedShip(4, grid, 2, 0);//  ####.## 02
			SetRightRankedShip(2, grid, 2, 5);//  ....... 03
			SetRightRankedShip(4, grid, 4, 0);//  ####.## 04
			SetRightRankedShip(2, grid, 4, 5);//  ....... 05
			SetRightRankedShip(4, grid, 6, 0);//  ####.## 06
			SetRightRankedShip(2, grid, 6, 5);//  ....... 07
			SetRightRankedShip(2, grid, 8, 0);//  ##.#### 08
			SetRightRankedShip(4, grid, 8, 3);//  ....... 09
			SetRightRankedShip(3, grid, 10, 0);// ###.### 10
			SetRightRankedShip(3, grid, 10, 4);// ....... 11
			SetRightRankedShip(3, grid, 12, 0);// ###.... 12
			var fleet = new Fleet(grid);
			fleet.Scan();
			VerifyScanResult(fleet, true, 39, config, current);
		}
		[TestMethod]
		public void Scan_InvalidFleetComposition_Lack()
		{
			var config = new byte[5] { 1, 3, 5, 3, 1 };
			var current = new byte[5] { 1, 3, 4, 3, 1 };
			Fleet.SetConfig(config);
			Grid grid = Grid.MakeOwnGrid();
			SetRightRankedShip(5, grid, 0, 0);//  #####.# 00
			SetRightRankedShip(1, grid, 0, 6);//  ....... 01
			SetRightRankedShip(4, grid, 2, 0);//  ####.## 02
			SetRightRankedShip(2, grid, 2, 5);//  ....... 03
			SetRightRankedShip(4, grid, 4, 0);//  ####.## 04
			SetRightRankedShip(2, grid, 4, 5);//  ....... 05
			SetRightRankedShip(4, grid, 6, 0);//  ####.## 06
			SetRightRankedShip(2, grid, 6, 5);//  ....... 07
			SetRightRankedShip(3, grid, 8, 0);//  ###.### 08
			SetRightRankedShip(3, grid, 8, 4);//  ....... 09
			SetRightRankedShip(3, grid, 10, 0);// ###.### 10
			SetRightRankedShip(3, grid, 10, 4);
			var fleet = new Fleet(grid);
			fleet.Scan();
			VerifyScanResult(fleet, true, 36, config, current);
		}
		[TestMethod]
		public void Scan_InvalidFleetComposition_Excess()
		{
			var config = new byte[5] { 1, 3, 5, 3, 1 };
			var current = new byte[5] { 2, 3, 5, 3, 1 };
			Fleet.SetConfig(config);
			Grid grid = Grid.MakeOwnGrid();
			SetRightRankedShip(5, grid, 0, 0);//  #####.# 00
			SetRightRankedShip(1, grid, 0, 6);//  ....... 01
			SetRightRankedShip(4, grid, 2, 0);//  ####.## 02
			SetRightRankedShip(2, grid, 2, 5);//  ....... 03
			SetRightRankedShip(4, grid, 4, 0);//  ####.## 04
			SetRightRankedShip(2, grid, 4, 5);//  ....... 05
			SetRightRankedShip(4, grid, 6, 0);//  ####.## 06
			SetRightRankedShip(2, grid, 6, 5);//  ....... 07
			SetRightRankedShip(3, grid, 8, 0);//  ###.### 08
			SetRightRankedShip(3, grid, 8, 4);//  ....... 09
			SetRightRankedShip(3, grid, 10, 0);// ###.### 10
			SetRightRankedShip(3, grid, 10, 4);// ....... 11
			SetRightRankedShip(3, grid, 12, 0);// ###.#.. 12
			SetRightRankedShip(1, grid, 12, 4);
			var fleet = new Fleet(grid);
			fleet.Scan();
			VerifyScanResult(fleet, true, 40, config, current);
		}
		[TestMethod]
		public void Scan_InvalidStructure_XCollision_ToLeftDown()
		{
			var config = new byte[2] { 1, 1 };
			Fleet.SetConfig(config);
			Grid grid = Grid.MakeOwnGrid();
			SetRightRankedShip(2, grid, 0, 0);// ##. 0
			SetRightRankedShip(1, grid, 1, 2);// ..# 1
			var fleet = new Fleet(grid);
			fleet.Scan();
			VerifyScanResult(fleet, false, 3, config, config);
		}
		[TestMethod]
		public void Scan_InvalidStructure_XCollision_ToLeftUp()
		{
			var config = new byte[2] { 1, 1 };
			Fleet.SetConfig(config);
			Grid grid = Grid.MakeOwnGrid();
			SetRightRankedShip(1, grid, 0, 2);// ..# 0
			SetRightRankedShip(2, grid, 1, 0);// ##. 1
			var fleet = new Fleet(grid);
			fleet.Scan();
			VerifyScanResult(fleet, false, 3, config, config);
		}
		[TestMethod]
		public void Scan_InvalidStructure_Nonlinear()
		{
			var config = new byte[3] { 0, 0, 1 };
			var current = new byte[3] { 0, 0, 0 };
			Fleet.SetConfig(config);
			Grid grid = Grid.MakeOwnGrid();
			SetRightRankedShip(1, grid, 0, 1);// .# 0
			SetRightRankedShip(2, grid, 1, 0);// ## 1
			var fleet = new Fleet(grid);
			fleet.Scan();
			VerifyScanResult(fleet, false, 3, config, current);
		}
	}
}
