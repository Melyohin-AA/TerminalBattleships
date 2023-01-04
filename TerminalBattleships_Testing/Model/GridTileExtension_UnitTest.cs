using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TerminalBattleships.Model;

namespace TerminalBattleships_Testing.Model
{
	[TestClass]
	public class GridTileExtension_UnitTest
	{
		[TestMethod]
		public void IsWater()
		{
			Assert.IsFalse(GridTile.Uncertainty.IsWater());
			Assert.IsTrue(GridTile.IntactWater.IsWater());
			Assert.IsTrue(GridTile.ShotWater.IsWater());
			Assert.IsFalse(GridTile.IntactShip.IsWater());
			Assert.IsFalse(GridTile.DamagedShip.IsWater());
		}

		[TestMethod]
		public void IsShip()
		{
			Assert.IsFalse(GridTile.Uncertainty.IsShip());
			Assert.IsFalse(GridTile.IntactWater.IsShip());
			Assert.IsFalse(GridTile.ShotWater.IsShip());
			Assert.IsTrue(GridTile.IntactShip.IsShip());
			Assert.IsTrue(GridTile.DamagedShip.IsShip());
		}
	}
}
