using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TerminalBattleships.Model;
using TerminalBattleships.Network;

namespace TerminalBattleships_Testing.Network
{
	[TestClass]
	public class EncryptedCoord_UnitTest
	{
		private static Random random = new Random();

		private static byte[] GetRandomKey(int size = EncryptedCoord.HashSize)
		{
			var key = new byte[size];
			random.NextBytes(key);
			return key;
		}

		[TestMethod]
		public void EncryptionConstructor_Valid_HashSize()
		{
			var coord = new Coord(123);
			byte[] publicKey = GetRandomKey(), privateKey = GetRandomKey();
			var ec = new EncryptedCoord(coord, publicKey, privateKey);
			Assert.AreEqual(EncryptedCoord.HashSize, ec.Hash.Length);
		}
		[TestMethod]
		public void EncryptionConstructor_Valid_Deterministic()
		{
			var coord = new Coord(123);
			byte[] publicKey = GetRandomKey(), privateKey = GetRandomKey();
			var ec1 = new EncryptedCoord(coord, publicKey, privateKey);
			var ec2 = new EncryptedCoord(coord, publicKey, privateKey);
			for (byte i = 0; i < EncryptedCoord.HashSize; i++)
				Assert.IsTrue(ec1.Hash[i] == ec2.Hash[i]);
		}
		[TestMethod]
		public void EncryptionConstructor_Invalid_PublicKeyIsNull()
		{
			var coord = new Coord(123);
			byte[] publicKey = null, privateKey = GetRandomKey();
			Assert.ThrowsException<NullReferenceException>(() => new EncryptedCoord(coord, publicKey, privateKey));
		}
		[TestMethod]
		public void EncryptionConstructor_Invalid_PrivateKeyIsNull()
		{
			var coord = new Coord(123);
			byte[] publicKey = GetRandomKey(), privateKey = null;
			Assert.ThrowsException<NullReferenceException>(() => new EncryptedCoord(coord, publicKey, privateKey));
		}
		[TestMethod]
		public void EncryptionConstructor_Invalid_PublicKeyHasWrongSize()
		{
			var coord = new Coord(123);
			byte[] publicKey = GetRandomKey(EncryptedCoord.HashSize + 1), privateKey = GetRandomKey();
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => new EncryptedCoord(coord, publicKey, privateKey));
		}
		[TestMethod]
		public void EncryptionConstructor_Invalid_PrivateKeyHasWrongSize()
		{
			var coord = new Coord(123);
			byte[] publicKey = GetRandomKey(), privateKey = GetRandomKey(EncryptedCoord.HashSize + 1);
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => new EncryptedCoord(coord, publicKey, privateKey));
		}

		[TestMethod]
		public void ReadConstructor_Valid()
		{
			var coord = new Coord(123);
			byte[] publicKey = GetRandomKey(), privateKey = GetRandomKey();
			var expectedEC = new EncryptedCoord(coord, publicKey, privateKey);
			var stream = new MemoryStream(EncryptedCoord.HashSize);
			expectedEC.Write(stream);
			stream.Position = 0;
			var actualEC = new EncryptedCoord(stream);
			Assert.AreEqual(expectedEC, actualEC);
		}
		[TestMethod]
		public void ReadConstructor_Invalid_NullStream()
		{
			Assert.ThrowsException<NullReferenceException>(() => new EncryptedCoord(null));
		}
		[TestMethod]
		[Timeout(1000)]
		public void ReadConstructor_DataPartitialDelay()
		{
			var coord = new Coord(123);
			byte[] publicKey = GetRandomKey(), privateKey = GetRandomKey();
			var ec = new EncryptedCoord(coord, publicKey, privateKey);
			var stream = new MemoryStream(EncryptedCoord.HashSize);
			ec.Write(stream);
			stream.Position = 1;
			Task inner = Task.Run(() => new EncryptedCoord(stream));
			while (stream.Position < stream.Length) Thread.Sleep(5);
			if (inner.Exception != null) throw inner.Exception;
			Assert.IsFalse(inner.IsCompleted);
			stream.Position -= 1;
			while (stream.Position < stream.Length) Thread.Sleep(5);
			if (inner.Exception != null) throw inner.Exception;
			Assert.IsTrue(inner.IsCompleted);
		}

		[TestMethod]
		public void Write_Valid()
		{
			var coord = new Coord(123);
			byte[] publicKey = GetRandomKey(), privateKey = GetRandomKey();
			var ec = new EncryptedCoord(coord, publicKey, privateKey);
			var stream = new MemoryStream();
			ec.Write(stream);
			Assert.AreEqual(EncryptedCoord.HashSize, stream.Position);
		}
		[TestMethod]
		public void Write_Invalid_NullStream()
		{
			var coord = new Coord(123);
			byte[] publicKey = GetRandomKey(), privateKey = GetRandomKey();
			var ec = new EncryptedCoord(coord, publicKey, privateKey);
			Assert.ThrowsException<NullReferenceException>(() => ec.Write(null));
		}

		[TestMethod]
		public void Equals_True_CmpWithSelf()
		{
			var coord = new Coord(123);
			byte[] publicKey = GetRandomKey(), privateKey = GetRandomKey();
			var ec = new EncryptedCoord(coord, publicKey, privateKey);
			Assert.IsTrue(ec.Equals(ec));
		}
		[TestMethod]
		public void Equals_True_CmpWithOther()
		{
			var coord = new Coord(123);
			byte[] publicKey = GetRandomKey(), privateKey = GetRandomKey();
			var ec1 = new EncryptedCoord(coord, publicKey, privateKey);
			var ec2 = new EncryptedCoord(coord, publicKey, privateKey);
			Assert.IsTrue(ec1.Equals(ec2));
			Assert.IsTrue(ec2.Equals(ec1));
		}
		[TestMethod]
		public void Equals_False_CmpWithOther()
		{
			Coord coord1 = new Coord(123), coord2 = new Coord(216);
			byte[] publicKey = GetRandomKey(), privateKey = GetRandomKey();
			var ec1 = new EncryptedCoord(coord1, publicKey, privateKey);
			var ec2 = new EncryptedCoord(coord2, publicKey, privateKey);
			Assert.IsFalse(ec1.Equals(ec2));
			Assert.IsFalse(ec2.Equals(ec1));
		}
		[TestMethod]
		public void Equals_False_CmpWithNull()
		{
			Coord coord = new Coord(123);
			byte[] publicKey = GetRandomKey(), privateKey = GetRandomKey();
			var ec = new EncryptedCoord(coord, publicKey, privateKey);
			Assert.IsFalse(ec.Equals(null));
		}

		[TestMethod]
		public void GetHashCode_HashSize()
		{
			Coord coord = new Coord(123);
			byte[] publicKey = GetRandomKey(), privateKey = GetRandomKey();
			var ec = new EncryptedCoord(coord, publicKey, privateKey);
			int hashCode = ec.GetHashCode();
			Assert.IsTrue((hashCode >> 24) != 0);
			Assert.IsTrue(((hashCode >> 16) & 0xFF) != 0);
			Assert.IsTrue(((hashCode >> 8) & 0xFF) != 0);
			Assert.IsTrue((hashCode & 0xFF) != 0);
		}
		[TestMethod]
		public void GetHashCode_Deterministic()
		{
			Coord coord = new Coord(123);
			byte[] publicKey = GetRandomKey(), privateKey = GetRandomKey();
			var ec = new EncryptedCoord(coord, publicKey, privateKey);
			int hashCode1 = ec.GetHashCode();
			int hashCode2 = ec.GetHashCode();
			Assert.IsTrue(hashCode1 == hashCode2);
		}
	}
}
