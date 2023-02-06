using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TerminalBattleships.Model;
using TerminalBattleships.Network;

namespace TerminalBattleships_Testing.Network
{
	[TestClass]
	public class Justification_UnitTest
	{
		private static Justification MakeJustificationWithPublicKeys(FakeNetMember net, Random random)
		{
			var justification = new Justification(net, random);
			var publicKey = new byte[Justification.HashSize];
			random.NextBytes(publicKey);
			net.BackStream.Write(publicKey, 0, publicKey.Length);
			justification.SharePublicKeys();
			for (int i = Justification.HashSize; i > 0;)
				i -= net.BackStream.SkipBytes(i);
			return justification;
		}

		[TestMethod]
		public void Constructor_Valid()
		{
			bool failed = false;
			FakeNetMember net = FakeNetMember.MakeConnected(() => failed = true);
			var justification = new Justification(net, new Random());
			Assert.IsFalse(failed);
			Assert.AreEqual(Justification.HashSize, justification.OwnPrivateKey.Length);
			Assert.AreEqual(Justification.HashSize, justification.FoePublicKey.Length);
		}
		[TestMethod]
		public void Constructor_Invalid_NullNet()
		{
			Assert.ThrowsException<ArgumentNullException>(() => new Justification(null, new Random()));
		}
		[TestMethod]
		public void Constructor_Invalid_NullRandom()
		{
			bool failed = false;
			FakeNetMember net = FakeNetMember.MakeConnected(() => failed = true);
			Assert.ThrowsException<NullReferenceException>(() => new Justification(net, null));
			Assert.IsFalse(failed);
		}

		[TestMethod]
		[Timeout(1000)]
		public void SharePublicKeys_Coupled()
		{
			Random random = new Random();
			bool failed = false;
			var serverNet = FakeNetMember.MakeConnected(() => failed = true);
			serverNet.IsServer = true;
			var clientNet = FakeNetMember.MakeConnected(() => failed = true,
				serverNet.BackStream, serverNet.FrontStream);
			clientNet.IsServer = false;
			var serverJustification = new Justification(serverNet, random);
			var clientJustification = new Justification(clientNet, random);
			//
			Task serverTask = Task.Run(() => serverJustification.SharePublicKeys());
			clientJustification.SharePublicKeys();
			serverTask.Wait();
			//
			Assert.IsFalse(failed);
			if (serverTask.Exception != null) throw serverTask.Exception;
			Assert.AreEqual(Justification.HashSize, serverJustification.OwnPublicKey.Length);
			Assert.AreEqual(Justification.HashSize, clientJustification.OwnPublicKey.Length);
			for (byte i = 0; i < Justification.HashSize; i++)
			{
				Assert.AreEqual(serverJustification.FoePublicKey[i], clientJustification.OwnPublicKey[i]);
				Assert.AreEqual(clientJustification.FoePublicKey[i], serverJustification.OwnPublicKey[i]);
			}
		}

		[TestMethod]
		[Timeout(1000)]
		public void SharePrivateKeys_Valid_Coupled()
		{
			Random random = new Random();
			bool failed = false;
			var serverNet = FakeNetMember.MakeConnected(() => failed = true);
			serverNet.IsServer = true;
			var clientNet = FakeNetMember.MakeConnected(() => failed = true,
				serverNet.BackStream, serverNet.FrontStream);
			clientNet.IsServer = false;
			var serverJustification = new Justification(serverNet, random);
			var clientJustification = new Justification(clientNet, random);
			//
			Task serverTask = Task.Run(() => serverJustification.SharePrivateKeys());
			clientJustification.SharePrivateKeys();
			serverTask.Wait();
			//
			Assert.IsFalse(failed);
			if (serverTask.Exception != null) throw serverTask.Exception;
			Assert.AreEqual(Justification.HashSize, serverJustification.FoePrivateKey.Length);
			Assert.AreEqual(Justification.HashSize, clientJustification.FoePrivateKey.Length);
			for (byte i = 0; i < Justification.HashSize; i++)
			{
				Assert.AreEqual(serverJustification.OwnPrivateKey[i], clientJustification.FoePrivateKey[i]);
				Assert.AreEqual(clientJustification.OwnPrivateKey[i], serverJustification.FoePrivateKey[i]);
			}
		}

		[TestMethod]
		[Timeout(1000)]
		public void SendOwnShipEncryptedCoords_Valid_OwnOpenCoordField()
		{
			Coord[] shipCoords = new[] { new Coord(66), new Coord(123), new Coord(124) };
			bool failed = false;
			var net = FakeNetMember.MakeConnected(() => failed = true);
			Justification justification = MakeJustificationWithPublicKeys(net, new Random());
			//
			justification.SendOwnShipEncryptedCoords(shipCoords);
			//
			Assert.IsFalse(failed);
			Assert.AreEqual(shipCoords.Length, justification.OwnShipOpenCoords.Length);
			for (byte i = 0; i < shipCoords.Length; i++)
				Assert.AreEqual(shipCoords[i], justification.OwnShipOpenCoords[i]);
		}
		[TestMethod]
		[Timeout(1000)]
		public void SendOwnShipEncryptedCoords_Valid_OwnEncryptedCoordField()
		{
			Coord[] shipCoords = new[] { new Coord(66), new Coord(123), new Coord(124) };
			bool failed = false;
			var net = FakeNetMember.MakeConnected(() => failed = true);
			Justification justification = MakeJustificationWithPublicKeys(net, new Random());
			//
			justification.SendOwnShipEncryptedCoords(shipCoords);
			//
			Assert.IsFalse(failed);
			Assert.AreEqual(shipCoords.Length, justification.OwnShipEncryptedCoords.Length);
			for (byte i = 0; i < shipCoords.Length; i++)
			{
				var expected = new EncryptedCoord(shipCoords[i],
					justification.OwnPublicKey, justification.OwnPrivateKey);
				Assert.AreEqual(expected, justification.OwnShipEncryptedCoords[i]);
			}
		}
		[TestMethod]
		[Timeout(1000)]
		public void SendOwnShipEncryptedCoords_Valid()
		{
			Coord[] shipCoords = new[] { new Coord(66), new Coord(123), new Coord(124) };
			bool failed = false;
			var net = FakeNetMember.MakeConnected(() => failed = true);
			Justification justification = MakeJustificationWithPublicKeys(net, new Random());
			//
			justification.SendOwnShipEncryptedCoords(shipCoords);
			//
			Assert.IsFalse(failed);
			Assert.AreEqual(shipCoords.Length, net.BackStream.ReadByte());
			for (byte i = 0; i < shipCoords.Length; i++)
			{
				var expected = justification.OwnShipEncryptedCoords[i];
				var actual = new EncryptedCoord(net.BackStream);
				Assert.AreEqual(expected, actual);
			}
		}
		[TestMethod]
		[Timeout(1000)]
		public void SendOwnShipEncryptedCoords_Invalid_NullCoords()
		{
			bool failed = false;
			var net = FakeNetMember.MakeConnected(() => failed = true);
			Justification justification = MakeJustificationWithPublicKeys(net, new Random());
			Assert.IsFalse(failed);
			Assert.ThrowsException<NullReferenceException>(() => justification.SendOwnShipEncryptedCoords(null));
		}
		[TestMethod]
		[Timeout(1000)]
		public void SendOwnShipEncryptedCoords_Invalid_256Coords()
		{
			bool failed = false;
			var net = FakeNetMember.MakeConnected(() => failed = true);
			Justification justification = MakeJustificationWithPublicKeys(net, new Random());
			Assert.IsFalse(failed);
			Assert.ThrowsException<ArgumentOutOfRangeException>(
				() => justification.SendOwnShipEncryptedCoords(new Coord[256]));
		}

		[TestMethod]
		[Timeout(1000)]
		public void ReceiveFoeShipEncryptedCoords_Valid()
		{
			Coord[] shipCoords = new[] { new Coord(66), new Coord(123), new Coord(124) };
			bool failed = false;
			var net = FakeNetMember.MakeConnected(() => failed = true);
			var random = new Random();
			var foePrivateKey = new byte[Justification.HashSize];
			random.NextBytes(foePrivateKey);
			Justification justification = MakeJustificationWithPublicKeys(net, random);
			net.BackStream.WriteByte((byte)shipCoords.Length);
			var foeECs = new EncryptedCoord[shipCoords.Length];
			for (byte i = 0; i < shipCoords.Length; i++)
			{
				foeECs[i] = new EncryptedCoord(shipCoords[i], justification.OwnPublicKey, justification.OwnPrivateKey);
				foeECs[i].Write(net.BackStream);
			}
			//
			justification.ReceiveFoeShipEncryptedCoords();
			//
			Assert.IsFalse(failed);
			Assert.AreEqual(shipCoords.Length, justification.FoeShipEncryptedCoords.Length);
			for (byte i = 0; i < shipCoords.Length; i++)
				Assert.AreEqual(foeECs[i], justification.FoeShipEncryptedCoords[i]);
		}

		[TestMethod]
		[Timeout(1000)]
		public void SendOwnShipOpenCoords_Valid()
		{
			Coord[] expectedCoords = new[] { new Coord(66), new Coord(123), new Coord(124) };
			bool failed = false;
			var net = FakeNetMember.MakeConnected(() => failed = true);
			Justification justification = MakeJustificationWithPublicKeys(net, new Random());
			justification.SendOwnShipEncryptedCoords(expectedCoords);
			while (net.BackStream.Available > 0)
				net.BackStream.SkipBytes(net.BackStream.Available);
			//
			justification.SendOwnShipOpenCoords();
			//
			Assert.IsFalse(failed);
			Assert.AreEqual(expectedCoords.Length, net.BackStream.ReadByte());
			var actualIJs = new byte[expectedCoords.Length];
			for (int i = 0; i < actualIJs.Length; i++)
				i += net.BackStream.Read(actualIJs, i, actualIJs.Length - i);
			for (byte i = 0; i < expectedCoords.Length; i++)
				Assert.AreEqual(expectedCoords[i].IJ, actualIJs[i]);
		}

		[TestMethod]
		[Timeout(1000)]
		public void ReceiveFoeShipOpenCoords_Valid()
		{
			Coord[] expectedCoords = new[] { new Coord(66), new Coord(123), new Coord(124) };
			bool failed = false;
			var net = FakeNetMember.MakeConnected(() => failed = true);
			var justification = new Justification(net, new Random());
			net.BackStream.WriteByte((byte)expectedCoords.Length);
			net.BackStream.Write(expectedCoords.Select(c => c.IJ).ToArray(), 0, expectedCoords.Length);
			//
			justification.ReceiveFoeShipOpenCoords();
			//
			Assert.IsFalse(failed);
			Assert.AreEqual(expectedCoords.Length, justification.FoeShipOpenCoords.Length);
			for (byte i = 0; i < expectedCoords.Length; i++)
				Assert.AreEqual(expectedCoords[i], justification.FoeShipOpenCoords[i]);
		}

		/* TODO: Write tests for IsFoeCheater:
		 - pass
		 - FoeShipOpenCoords.Length != FoeShipEncryptedCoords.Length
		 - GridTile.DamagedShip / GridTile.IntactShip
		 - !fleet.CorrectStructers
		 - fleet.ShipCount != FoeShipOpenCoords.Length
		 - set.CurrentCount != set.RequiredCount
		 - !ec.Equals(FoeShipEncryptedCoords[i])
		 - null foeGrid arg
		*/
	}
}
