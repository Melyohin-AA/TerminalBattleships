using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TerminalBattleships.Model;
using TerminalBattleships.Network;

namespace TerminalBattleships_Testing.Network
{
	[TestClass]
	public class FoeTurnResponser_UnitTest
	{
		[TestMethod]
		public void Constructor_Valid()
		{
			bool failed = false;
			FakeNetMember net = FakeNetMember.MakeConnected(() => failed = true);
			Func<Coord, FireResult> shotHandler = c =>
			{
				failed = true;
				return FireResult.Error400;
			};
			var responser = new FoeTurnResponser(net, shotHandler);
			Assert.IsFalse(failed);
		}
		[TestMethod]
		public void Constructor_Invalid_NullNet()
		{
			bool failed = false;
			Func<Coord, FireResult> shotHandler = c =>
			{
				failed = true;
				return FireResult.Error400;
			};
			Assert.ThrowsException<ArgumentNullException>(() => new FoeTurnResponser(null, shotHandler));
			Assert.IsFalse(failed);
		}
		[TestMethod]
		public void Constructor_Invalid_NullShotHandler()
		{
			bool failed = false;
			FakeNetMember net = FakeNetMember.MakeConnected(() => failed = true);
			Assert.ThrowsException<ArgumentNullException>(() => new FoeTurnResponser(net, null));
			Assert.IsFalse(failed);
		}

		[TestMethod]
		public void ReceiveShot_WaitForRequest()
		{
			const byte targetIJ = 123;
			bool failed = false;
			FakeNetMember net = FakeNetMember.MakeConnected(() => failed = true);
			Func<Coord, FireResult> shotHandler = c => FireResult.Miss;
			var responser = new FoeTurnResponser(net, shotHandler);
			bool requestSent = false;
			Task.Run(() =>
			{
				Thread.Sleep(100);
				requestSent = true;
				net.BackStream.WriteByte(targetIJ);
			});
			responser.ReceiveShot(out Coord target, out FireResult fireResult);
			Assert.IsTrue(requestSent);
			Assert.AreEqual(0, net.Available);
			Assert.IsFalse(failed);
		}
		[TestMethod]
		public void ReceiveShot_TargetCoord()
		{
			const byte targetIJ = 123;
			bool failed = false;
			FakeNetMember net = FakeNetMember.MakeConnected(() => failed = true);
			Func<Coord, FireResult> shotHandler = c =>
			{
				if (targetIJ != c.IJ) failed = true;
				return FireResult.Miss;
			};
			net.BackStream.WriteByte(targetIJ);
			var responser = new FoeTurnResponser(net, shotHandler);
			responser.ReceiveShot(out Coord target, out FireResult fireResult);
			Assert.AreEqual(targetIJ, target.IJ);
			Assert.IsFalse(failed);
		}
		[TestMethod]
		public void ReceiveShot_FireResult()
		{
			const byte targetIJ = 123;
			bool failed = false;
			const FireResult expectedFireResult = FireResult.Miss;
			FakeNetMember net = FakeNetMember.MakeConnected(() => failed = true);
			Func<Coord, FireResult> shotHandler = c => expectedFireResult;
			net.BackStream.WriteByte(targetIJ);
			var responser = new FoeTurnResponser(net, shotHandler);
			responser.ReceiveShot(out Coord target, out FireResult actualFireResult);
			Assert.AreEqual(expectedFireResult, actualFireResult);
			Assert.IsFalse(failed);
		}
	}
}
