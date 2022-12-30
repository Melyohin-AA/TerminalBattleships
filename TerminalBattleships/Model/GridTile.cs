using System;

namespace TerminalBattleships.Model
{
	public enum GridTile
	{
		Uncertainty = 0,
		IntactWater = 1,
		ShotWater = 2,
		IntactShip = 3,
		DamagedShip = 4,
	}

	public static class GridTileExtension
	{
		public static bool IsWater(this GridTile tile)
		{
			return (tile == GridTile.IntactWater) || (tile == GridTile.ShotWater);
		}
		public static bool IsShip(this GridTile tile)
		{
			return (tile == GridTile.IntactShip) || (tile == GridTile.DamagedShip);
		}
	}
}
