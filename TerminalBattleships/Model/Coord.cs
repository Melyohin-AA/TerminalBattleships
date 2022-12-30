using System;

namespace TerminalBattleships.Model
{
	public struct Coord
	{
		public byte IJ { get; }
		public byte I => (byte)(IJ >> 4);
		public byte J => (byte)(IJ & 0x0F);

		public Coord Up => new Coord((byte)(I - 1), J);
		public Coord Down => new Coord((byte)(I + 1), J);
		public Coord Left => new Coord(I, (byte)(J - 1));
		public Coord Right => new Coord(I, (byte)(J + 1));

		public Coord UpLeft => new Coord((byte)(I - 1), (byte)(J - 1));
		public Coord UpRight => new Coord((byte)(I - 1), (byte)(J + 1));
		public Coord DownLeft => new Coord((byte)(I + 1), (byte)(J - 1));
		public Coord DownRight => new Coord((byte)(I + 1), (byte)(J + 1));

		public Coord(byte ij)
		{
			IJ = ij;
		}
		public Coord(byte i, byte j)
		{
			if (i > 15) throw new ArgumentOutOfRangeException(nameof(i));
			if (j > 15) throw new ArgumentOutOfRangeException(nameof(j));
			IJ = (byte)((i << 4) | j);
		}
	}
}
