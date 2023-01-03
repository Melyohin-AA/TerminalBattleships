using System;
using System.Linq;
using TerminalBattleships.Model;

namespace TerminalBattleships.Network
{
	public class Justification
	{
		public const byte HashSize = 64;

		private readonly INetMember net;

		public byte[] OwnPublicKey { get; private set; }
		public byte[] OwnPrivateKey { get; private set; }
		public byte[] FoePublicKey { get; private set; }
		public byte[] FoePrivateKey { get; private set; }

		public Coord[] OwnShipOpenCoords { get; private set; }
		public EncryptedCoord[] OwnShipEncryptedCoords { get; private set; }
		public Coord[] FoeShipOpenCoords { get; private set; }
		public EncryptedCoord[] FoeShipEncryptedCoords { get; private set; }

		public Justification(INetMember net, Random random)
		{
			this.net = net ?? throw new ArgumentNullException(nameof(net));
			OwnPrivateKey = new byte[HashSize];
			FoePublicKey = new byte[HashSize];
			random.NextBytes(OwnPrivateKey);
			random.NextBytes(FoePublicKey);
		}

		public void SharePublicKeys()
		{
			if (OwnPublicKey != null) throw new InvalidOperationException();
			net.Stream.Write(FoePublicKey, 0, HashSize);
			net.Stream.Flush();
			OwnPublicKey = new byte[HashSize];
			net.Stream.Read(OwnPublicKey, 0, HashSize);
		}
		public void SharePrivateKeys()
		{
			if (FoePrivateKey != null) throw new InvalidOperationException();
			net.Stream.Write(OwnPrivateKey, 0, HashSize);
			net.Stream.Flush();
			FoePrivateKey = new byte[HashSize];
			net.Stream.Read(FoePrivateKey, 0, HashSize);
		}

		public void SendOwnShipEncryptedCoords(Coord[] coords)
		{
			if (coords.Length > 255) throw new ArgumentOutOfRangeException(nameof(coords));
			OwnShipOpenCoords = coords;
			OwnShipEncryptedCoords = coords.Select(c => new EncryptedCoord(c, OwnPublicKey, OwnPrivateKey)).ToArray();
			net.Stream.WriteByte((byte)OwnShipEncryptedCoords.Length);
			foreach (EncryptedCoord ec in OwnShipEncryptedCoords)
				ec.Write(net.Stream);
			net.Stream.Flush();
		}
		public void ReceiveFoeShipEncryptedCoords()
		{
			byte count = net.ReadByte();
			FoeShipEncryptedCoords = new EncryptedCoord[count];
			for (short i = 0; i < count; i++)
				FoeShipEncryptedCoords[i] = new EncryptedCoord(net.Stream);
		}

		public void SendOwnShipOpenCoords()
		{
			net.Stream.WriteByte((byte)OwnShipOpenCoords.Length);
			net.Stream.Write(OwnShipOpenCoords.Select(c => c.IJ).ToArray(), 0, OwnShipOpenCoords.Length);
			net.Stream.Flush();
		}
		public void ReceiveFoeShipOpenCoords()
		{
			byte count = net.ReadByte();
			var buffer = new byte[count];
			net.Stream.Read(buffer, 0, count);
			FoeShipOpenCoords = buffer.Select(ij => new Coord(ij)).ToArray();
		}

		public bool IsFoeCheater(Grid foeGrid)
		{
			if (FoeShipOpenCoords.Length != FoeShipEncryptedCoords.Length) return true;
			foreach (Coord coord in FoeShipOpenCoords)
			{
				if (foeGrid[coord] == GridTile.DamagedShip) continue;
				foeGrid[coord] = GridTile.IntactShip;
			}
			var fleet = new Fleet(foeGrid);
			fleet.Scan();
			if (!fleet.CorrectStructers || (fleet.ShipCount != FoeShipOpenCoords.Length)) return true;
			foreach (Fleet.RankedSet set in fleet.RankedSetShips)
				if (set.CurrentCount != set.RequiredCount) return true;
			for (short i = 0; i < FoeShipOpenCoords.Length; i++)
			{
				var ec = new EncryptedCoord(FoeShipOpenCoords[i], FoePublicKey, FoePrivateKey);
				if (ec != FoeShipEncryptedCoords[i]) return true;
			}
			return false;
		}
	}
}
