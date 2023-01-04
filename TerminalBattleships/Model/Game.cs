using System;
using System.Collections.Generic;

namespace TerminalBattleships.Model
{
	public class Game
	{
		public Grid OwnGrid { get; }
		public Grid FoeGrid { get; }

		private bool foeFleetCompleted;
		public bool FoeFleetCompleted
		{
			get => foeFleetCompleted;
			set
			{
				if (Stage != GameStage.BuildingFleet) throw new InvalidOperationException();
				foeFleetCompleted = value;
			}
		}
		public bool OwnFleetCompleted { get; private set; }

		public byte OwnIntactShipCount { get; private set; }
		public byte FoeIntactShipCount { get; private set; }

		public GameStage Stage { get; private set; }
		public bool IsOwnTurn { get; private set; }
		public bool IsFirstTurnDetermined { get; private set; }

		public Game()
		{
			OwnGrid = Grid.MakeOwnGrid();
			FoeGrid = Grid.MakeFoeGrid();
		}

		public void EndFleetBuildingStage()
		{
			if (!((Stage == GameStage.BuildingFleet) &&
				FoeFleetCompleted && OwnFleetCompleted && IsFirstTurnDetermined))
				throw new InvalidOperationException();
			Stage = GameStage.Playing;
		}
		public void EndPlayingStage()
		{
			if ((Stage != GameStage.Playing) || ((OwnIntactShipCount > 0) && (FoeIntactShipCount > 0)))
				throw new InvalidOperationException();
			Stage = GameStage.Over;
		}

		public bool TryCompleteFleet()
		{
			if ((Stage != GameStage.BuildingFleet) || OwnFleetCompleted) throw new InvalidOperationException();
			var fleet = new Fleet(OwnGrid);
			fleet.Scan();
			if (!fleet.IsComplete) return false;
			OwnFleetCompleted = true;
			OwnIntactShipCount = FoeIntactShipCount = fleet.ShipCount;
			return true;
		}
		public void ResetOwnFleetCompletedFlag()
		{
			if ((Stage != GameStage.BuildingFleet) || !OwnFleetCompleted) throw new InvalidOperationException();
			OwnFleetCompleted = false;
		}

		public FireResult ReceiveShot(Coord target)
		{
			if ((Stage != GameStage.Playing) || IsOwnTurn) throw new InvalidOperationException();
			switch (OwnGrid[target])
			{
				case GridTile.Uncertainty: throw new Exception();
				case GridTile.IntactWater:
					OwnGrid[target] = GridTile.ShotWater;
					IsOwnTurn = true;
					return FireResult.Miss;
				case GridTile.ShotWater: return FireResult.Error400;
				case GridTile.IntactShip:
					return HandleOwnShipHitResult(target);
				case GridTile.DamagedShip: return FireResult.Error400;
				default: throw new Exception();
			}
		}
		private FireResult HandleOwnShipHitResult(Coord target)
		{
			if (OwnIntactShipCount == 0) throw new InvalidOperationException();
			OwnIntactShipCount--;
			OwnGrid[target] = GridTile.DamagedShip;
			var checkedTiles = new HashSet<Coord>();
			var dfs = new Stack<Coord>();
			dfs.Push(target);
			while (dfs.Count > 0)
			{
				Coord coord = dfs.Pop();
				if (checkedTiles.Contains(coord)) continue;
				checkedTiles.Add(coord);
				switch (OwnGrid[coord])
				{
					case GridTile.Uncertainty: throw new Exception();
					case GridTile.IntactWater:
					case GridTile.ShotWater: continue;
					case GridTile.IntactShip: return FireResult.Hit;
					case GridTile.DamagedShip: break;
					default: throw new Exception();
				}
				if (coord.I > 0) dfs.Push(coord.Up);
				if (coord.I < 15) dfs.Push(coord.Down);
				if (coord.J > 0) dfs.Push(coord.Left);
				if (coord.J < 15) dfs.Push(coord.Right);
			}
			return FireResult.Drown;
		}

		public void Fire(Coord target, FireResult result, Action<Coord> handleUncertaintyDiscovery)
		{
			if ((Stage != GameStage.Playing) || !IsOwnTurn) throw new InvalidOperationException();
			if (FoeGrid[target] != GridTile.Uncertainty)
				throw new ArgumentException("Player must fire at Uncertainty GridTiles only!", nameof(target));
			switch (result)
			{
				case FireResult.Miss: HandleFireMiss(target); break;
				case FireResult.Hit: HandleFireHit(target); break;
				case FireResult.Drown: HandleFireDrown(target, handleUncertaintyDiscovery); break;
				default: throw new Exception();
			}
		}
		private void HandleFireMiss(Coord target)
		{
			FoeGrid[target] = GridTile.ShotWater;
			IsOwnTurn = false;
		}
		private void HandleFireHit(Coord target)
		{
			if (FoeIntactShipCount == 0) throw new InvalidOperationException();
			FoeIntactShipCount--;
			FoeGrid[target] = GridTile.DamagedShip;
		}
		private void HandleFireDrown(Coord target, Action<Coord> handleUncertaintyDiscovery)
		{
			HandleFireHit(target);
			var checkedTiles = new HashSet<Coord> { target };
			var dfs = new Stack<Coord>();
			dfs.Push(target);
			while (dfs.Count > 0)
			{
				Coord coord = dfs.Pop();
				switch (FoeGrid[coord])
				{
					case GridTile.Uncertainty:
						FoeGrid[coord] = GridTile.IntactWater;
						handleUncertaintyDiscovery?.Invoke(coord);
						continue;
					case GridTile.IntactWater:
					case GridTile.ShotWater: continue;
					case GridTile.IntactShip: throw new Exception();
					case GridTile.DamagedShip: break;
					default: throw new Exception();
				}
				for (sbyte si = -1; si <= 1; si++)
				{
					for (sbyte sj = -1; sj <= 1; sj++)
					{
						if ((si == 0) && (sj == 0)) continue;
						short i = (short)(coord.I + si), j = (short)(coord.J + sj);
						if (!Grid.IsInBounds(i, j)) continue;
						var neighbour = new Coord((byte)i, (byte)j);
						if (checkedTiles.Contains(neighbour)) continue;
						checkedTiles.Add(neighbour);
						dfs.Push(neighbour);
					}
				}
			}
		}

		public void DetIsFirstTurnOwn(byte[] hash, bool isServer)
		{
			if ((Stage != GameStage.BuildingFleet) || IsFirstTurnDetermined) throw new InvalidOperationException();
			byte xhash = 0;
			foreach (byte b in hash)
				xhash ^= b;
			xhash = (byte)((xhash >> 4) ^ (xhash & 0x0F));
			xhash = (byte)((xhash >> 2) ^ (xhash & 0x03));
			xhash = (byte)((xhash >> 1) ^ (xhash & 0x01));
			IsOwnTurn = (xhash == 1) == isServer;
			IsFirstTurnDetermined = true;
		}
	}
}
