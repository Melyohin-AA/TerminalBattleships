using System;
using System.IO;
using System.Security.Cryptography;

namespace TerminalBattleships.Network
{
	public class EncryptedCoord
	{
		public const byte HashSize = Justification.HashSize;

		public byte[] Hash { get; }

		public EncryptedCoord(Model.Coord coord, byte[] publicKey, byte[] privateKey)
		{
			if (publicKey.Length != HashSize) throw new ArgumentOutOfRangeException(nameof(publicKey));
			if (privateKey.Length != HashSize) throw new ArgumentOutOfRangeException(nameof(privateKey));
			var encryptor = SHA512.Create();
			Hash = encryptor.ComputeHash(new[] { coord.IJ });
			Hash = ApplyKey(encryptor, publicKey);
			Hash = ApplyKey(encryptor, privateKey);
			if (Hash.Length != HashSize) throw new Exception();
		}
		private byte[] ApplyKey(SHA512 encryptor, byte[] key)
		{
			for (byte i = 0; i < HashSize; i++)
				Hash[i] ^= key[i];
			return encryptor.ComputeHash(Hash);
		}

		public EncryptedCoord(Stream stream)
		{
			Hash = new byte[HashSize];
			for (int i = 0; i < HashSize; )
				i += stream.Read(Hash, i, HashSize - i);
		}
		public void Write(Stream stream)
		{
			stream.Write(Hash, 0, HashSize);
		}

		public override bool Equals(object obj)
		{
			var other = obj as EncryptedCoord;
			if (other == null) return false;
			for (byte i = 0; i < HashSize; i++)
				if (Hash[i] != other.Hash[i])
					return false;
			return true;
		}
		public override int GetHashCode()
		{
			int hash32 = 0;
			for (byte i = 0; i < HashSize; i++)
			{
				switch (i & 3)
				{
					case 0: hash32 ^= Hash[i]; break;
					case 1: hash32 ^= Hash[i] << 8; break;
					case 2: hash32 ^= Hash[i] << 16; break;
					case 3: hash32 ^= Hash[i] << 24; break;
					default: throw new Exception();
				}
			}
			return hash32;
		}
	}
}
