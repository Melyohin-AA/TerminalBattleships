using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TerminalBattleships.Model;

namespace TerminalBattleships_Testing.Model
{
	[TestClass]
	public class Coord_UnitTest
	{
		[TestMethod]
		public void SingleParamConstructor()
		{
			const byte ij = 123;
			var coord = new Coord(ij);
			Assert.AreEqual(ij, coord.IJ);
			Assert.AreEqual((byte)(ij >> 4), coord.I);
			Assert.AreEqual((byte)(ij & 0x0F), coord.J);
		}

		[TestMethod]
		public void PairParamConstructor_ValidRange()
		{
			const byte i = 12, j = 5;
			var coord = new Coord(i, j);
			Assert.AreEqual((byte)((i << 4) | j), coord.IJ);
			Assert.AreEqual(i, coord.I);
			Assert.AreEqual(j, coord.J);
		}
		[TestMethod]
		public void PairParamConstructor_I_InvalidRange()
		{
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Coord(16, 5));
		}
		[TestMethod]
		public void PairParamConstructor_J_InvalidRange()
		{
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Coord(12, 16));
		}

		[TestMethod]
		public void Up()
		{
			const byte i = 12, j = 5;
			var coord = new Coord(i, j);
			var up = coord.Up;
			Assert.AreEqual(i - 1, up.I);
			Assert.AreEqual(j, up.J);
		}
		[TestMethod]
		public void Down()
		{
			const byte i = 12, j = 5;
			var coord = new Coord(i, j);
			var down = coord.Down;
			Assert.AreEqual(i + 1, down.I);
			Assert.AreEqual(j, down.J);
		}
		[TestMethod]
		public void Left()
		{
			const byte i = 12, j = 5;
			var coord = new Coord(i, j);
			var left = coord.Left;
			Assert.AreEqual(i, left.I);
			Assert.AreEqual(j - 1, left.J);
		}
		[TestMethod]
		public void Right()
		{
			const byte i = 12, j = 5;
			var coord = new Coord(i, j);
			var right = coord.Right;
			Assert.AreEqual(i, right.I);
			Assert.AreEqual(j + 1, right.J);
		}
	}
}
