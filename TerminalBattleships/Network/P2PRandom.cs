using System;
using System.Security.Cryptography;

namespace TerminalBattleships.Network
{
	public class P2PRandom
	{
		public const byte HashSize = Justification.HashSize;

		private Random random;
		
		public byte[] Number { get; } = new byte[HashSize];

		public P2PRandom(Random random)
		{
			this.random = random ?? throw new ArgumentNullException(nameof(random));
		}

		public void Generate(NetMember net)
		{
			if (net.IsServer) random.NextBytes(Number);
			var ownNumber = new byte[HashSize];
			var foeNumber = new byte[HashSize];
			random.NextBytes(ownNumber);
			for (byte i = 0; i < HashSize; i++)
			{
				net.Stream.WriteByte(ownNumber[i]);
				net.Stream.Flush();
				foeNumber[i] = net.ReadByte();
			}
			var encryptor = SHA512.Create();
			ownNumber = encryptor.ComputeHash(ownNumber);
			foeNumber = encryptor.ComputeHash(foeNumber);
			for (byte i = 0; i < HashSize; i++)
				Number[i] = (byte)(ownNumber[i] ^ foeNumber[i]);
		}
	}
}
