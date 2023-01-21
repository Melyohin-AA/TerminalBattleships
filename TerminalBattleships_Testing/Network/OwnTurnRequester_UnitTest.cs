using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TerminalBattleships.Model;
using TerminalBattleships.Network;

namespace TerminalBattleships_Testing.Network
{
	[TestClass]
	public class OwnTurnRequester_UnitTest
	{
		[TestMethod]
		public void Constructor_Valid()
		{
			bool failed = false;
			MockNetMember net = MockNetMember.MakeConnected(() => failed = true);
			var requester = new OwnTurnRequester(net);
			Assert.IsFalse(failed);
		}
		[TestMethod]
		public void Constructor_Invalid_NullNet()
		{
			Assert.ThrowsException<ArgumentNullException>(() => new OwnTurnRequester(null));
		}

		[TestMethod]
		public void Fire_WaitForRequest()
		{
			const byte targetIJ = 123;
			bool failed = false;
			MockNetMember net = MockNetMember.MakeConnected(() => failed = true);
			var requester = new OwnTurnRequester(net);
			bool responseSent = false;
			Task.Run(() =>
			{
				Thread.Sleep(100);
				if (net.BackStream.Available == 0) failed = true;
				responseSent = true;
				net.BackStream.WriteByte((byte)FireResult.Miss);
			});
			requester.Fire(new Coord(targetIJ));
			Assert.IsTrue(responseSent);
			Assert.AreEqual(0, net.Available);
			Assert.IsFalse(failed);
		}
		[TestMethod]
		public void Fire_TargetCoord()
		{
			const byte expectedTargetIJ = 123;
			bool failed = false;
			MockNetMember net = MockNetMember.MakeConnected(() => failed = true);
			var requester = new OwnTurnRequester(net);
			Task.Run(() =>
			{
				while (net.BackStream.Available == 0) Thread.Sleep(5);
				int actualTargetIJ = net.BackStream.ReadByte();
				if (expectedTargetIJ != actualTargetIJ) failed = true;
				net.BackStream.WriteByte((byte)FireResult.Miss);
			});
			requester.Fire(new Coord(expectedTargetIJ));
			Assert.IsFalse(failed);
		}
		[TestMethod]
		public void Fire_FireResult()
		{
			const byte targetIJ = 123;
			bool failed = false;
			const FireResult expectedFireResult = FireResult.Miss;
			FireResult actualFireResult;
			MockNetMember net = MockNetMember.MakeConnected(() => failed = true);
			var requester = new OwnTurnRequester(net);
			Task.Run(() =>
			{
				while (net.BackStream.Available == 0) Thread.Sleep(5);
				net.BackStream.ReadByte();
				net.BackStream.WriteByte((byte)expectedFireResult);
			});
			actualFireResult = requester.Fire(new Coord(targetIJ));
			Assert.AreEqual(expectedFireResult, actualFireResult);
			Assert.IsFalse(failed);
		}
	}
}
