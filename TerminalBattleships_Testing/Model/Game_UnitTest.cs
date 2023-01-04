using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TerminalBattleships.Model;

namespace TerminalBattleships_Testing.Model
{
	[TestClass]
	public class Game_UnitTest
	{
		private Random random = new Random();

		private byte[] GenerateTurnDetHash()
		{
			var hash = new byte[64];
			random.NextBytes(hash);
			return hash;
		}

		private Game GetGameAtPlayingStage(byte shipIJ = 123)
		{
			Fleet.SetConfig(new byte[] { 1 });
			var game = new Game();
			game.OwnGrid[shipIJ] = GridTile.IntactShip;
			game.TryCompleteFleet();
			game.FoeFleetCompleted = true;
			game.DetIsFirstTurnOwn(GenerateTurnDetHash(), true);
			game.EndFleetBuildingStage();
			return game;
		}
		private Game GetGameAtPlayingStage(bool firstTurnOwn, byte[] fleetConfig, Action<Grid> buildFleet)
		{
			byte[] hash = GenerateTurnDetHash();
			Fleet.SetConfig(fleetConfig);
			var serverGame = new Game();
			var clientGame = new Game();
			serverGame.DetIsFirstTurnOwn(hash, true);
			clientGame.DetIsFirstTurnOwn(hash, false);
			Game game = (serverGame.IsOwnTurn == firstTurnOwn) ? serverGame : clientGame;
			buildFleet(game.OwnGrid);
			game.TryCompleteFleet();
			game.FoeFleetCompleted = true;
			game.EndFleetBuildingStage();
			return game;
		}
		private Game GetGameAtPlayingStage(bool firstTurnOwn, byte shipIJ)
		{
			return GetGameAtPlayingStage(firstTurnOwn, new byte[] { 1 }, g => g[shipIJ] = GridTile.IntactShip);
		}

		[TestMethod]
		public void Constructor()
		{
			var game = new Game();
			Assert.AreEqual(GameStage.BuildingFleet, game.Stage);
			Assert.IsFalse(game.IsFirstTurnDetermined);
			Assert.IsFalse(game.FoeFleetCompleted);
			Assert.IsFalse(game.OwnFleetCompleted);
			for (short ij = 0; ij < 256; ij++)
			{
				Assert.AreEqual(GridTile.IntactWater, game.OwnGrid[ij]);
				Assert.AreEqual(GridTile.Uncertainty, game.FoeGrid[ij]);
			}
		}

		[TestMethod]
		public void TryCompleteFleet_Valid_Success()
		{
			Fleet.SetConfig(new byte[1] { 1 });
			var game = new Game();
			game.OwnGrid[123] = GridTile.IntactShip;
			bool success = game.TryCompleteFleet();
			Assert.IsTrue(success);
			Assert.IsTrue(game.OwnFleetCompleted);
		}
		[TestMethod]
		public void TryCompleteFleet_Valid_Failure()
		{
			Fleet.SetConfig(new byte[1] { 2 });
			var game = new Game();
			game.OwnGrid[123] = GridTile.IntactShip;
			bool success = game.TryCompleteFleet();
			Assert.IsFalse(success);
			Assert.IsFalse(game.OwnFleetCompleted);
		}
		[TestMethod]
		public void TryCompleteFleet_Invalid_WrongStage()
		{
			Game game = GetGameAtPlayingStage();
			Assert.ThrowsException<InvalidOperationException>(() => game.TryCompleteFleet());
		}
		[TestMethod]
		public void TryCompleteFleet_Invalid_OwnFleetAlreadyCompleted()
		{
			Fleet.SetConfig(new byte[1] { 1 });
			var game = new Game();
			game.OwnGrid[123] = GridTile.IntactShip;
			game.TryCompleteFleet();
			Assert.ThrowsException<InvalidOperationException>(() => game.TryCompleteFleet());
		}

		[TestMethod]
		public void ResetOwnFleetCompletedFlag_Valid()
		{
			Fleet.SetConfig(new byte[1] { 1 });
			var game = new Game();
			game.OwnGrid[123] = GridTile.IntactShip;
			game.TryCompleteFleet();
			game.ResetOwnFleetCompletedFlag();
			Assert.IsFalse(game.OwnFleetCompleted);
		}
		[TestMethod]
		public void ResetOwnFleetCompletedFlag_Invalid_OwnFleetIsYetIncomplete()
		{
			Fleet.SetConfig(new byte[1] { 1 });
			var game = new Game();
			Assert.ThrowsException<InvalidOperationException>(() => game.ResetOwnFleetCompletedFlag());
		}
		[TestMethod]
		public void ResetOwnFleetCompletedFlag_Invalid_WrongStage()
		{
			Game game = GetGameAtPlayingStage();
			Assert.ThrowsException<InvalidOperationException>(() => game.ResetOwnFleetCompletedFlag());
		}

		[TestMethod]
		public void EndFleetBuildingStage_Valid()
		{
			Fleet.SetConfig(new byte[1] { 1 });
			var game = new Game();
			game.OwnGrid[123] = GridTile.IntactShip;
			game.TryCompleteFleet();
			game.FoeFleetCompleted = true;
			game.DetIsFirstTurnOwn(GenerateTurnDetHash(), true);
			game.EndFleetBuildingStage();
			Assert.AreEqual(GameStage.Playing, game.Stage);
		}
		[TestMethod]
		public void EndFleetBuildingStage_Invalid_FirstTurnIsYetUndetermined()
		{
			Fleet.SetConfig(new byte[1] { 1 });
			var game = new Game();
			game.TryCompleteFleet();
			game.FoeFleetCompleted = true;
			Assert.ThrowsException<InvalidOperationException>(() => game.EndFleetBuildingStage());
		}
		[TestMethod]
		public void EndFleetBuildingStage_Invalid_OwnFleetIsYetIncomplete()
		{
			Fleet.SetConfig(new byte[1] { 1 });
			var game = new Game();
			game.FoeFleetCompleted = true;
			game.DetIsFirstTurnOwn(GenerateTurnDetHash(), true);
			Assert.ThrowsException<InvalidOperationException>(() => game.EndFleetBuildingStage());
		}
		[TestMethod]
		public void EndFleetBuildingStage_Invalid_FoeFleetIsYetIncomplete()
		{
			Fleet.SetConfig(new byte[1] { 1 });
			var game = new Game();
			game.OwnGrid[123] = GridTile.IntactShip;
			game.TryCompleteFleet();
			game.DetIsFirstTurnOwn(GenerateTurnDetHash(), true);
			Assert.ThrowsException<InvalidOperationException>(() => game.EndFleetBuildingStage());
		}
		[TestMethod]
		public void EndFleetBuildingStage_Invalid_WrongStage()
		{
			Game game = GetGameAtPlayingStage();
			Assert.ThrowsException<InvalidOperationException>(() => game.EndFleetBuildingStage());
		}

		[TestMethod]
		public void DetIsFirstTurnOwn_Valid_IsFirstTurnDetermined()
		{
			var game = new Game();
			Assert.IsFalse(game.IsFirstTurnDetermined);
			game.DetIsFirstTurnOwn(GenerateTurnDetHash(), true);
			Assert.IsTrue(game.IsFirstTurnDetermined);
		}
		[TestMethod]
		public void DetIsFirstTurnOwn_Valid_SameHashCausesDifferentResultOnServerAndClient()
		{
			byte[] hash = GenerateTurnDetHash();
			var serverGame = new Game();
			serverGame.DetIsFirstTurnOwn(hash, true);
			var clientGame = new Game();
			clientGame.DetIsFirstTurnOwn(hash, false);
			Assert.IsFalse(serverGame.IsOwnTurn == clientGame.IsOwnTurn);
		}
		[TestMethod]
		public void DetIsFirstTurnOwn_Invalid_FirstTurnIsAlreadyDetermined()
		{
			byte[] hash = GenerateTurnDetHash();
			var game = new Game();
			game.DetIsFirstTurnOwn(hash, true);
			Assert.ThrowsException<InvalidOperationException>(() => game.DetIsFirstTurnOwn(hash, true));
		}
		[TestMethod]
		public void DetIsFirstTurnOwn_Invalid_WrongStage()
		{
			Game game = GetGameAtPlayingStage();
			Action act = () => game.DetIsFirstTurnOwn(GenerateTurnDetHash(), true);
			Assert.ThrowsException<InvalidOperationException>(act);
		}

		[TestMethod]
		public void FoeFleetCompleted_Valid()
		{
			var game = new Game();
			game.FoeFleetCompleted = false;
			Assert.IsFalse(game.FoeFleetCompleted);
			game.FoeFleetCompleted = true;
			Assert.IsTrue(game.FoeFleetCompleted);
		}
		[TestMethod]
		public void FoeFleetCompleted_Invalid_WrongStage()
		{
			Game game = GetGameAtPlayingStage();
			Assert.ThrowsException<InvalidOperationException>(() => game.FoeFleetCompleted = false);
			Assert.ThrowsException<InvalidOperationException>(() => game.FoeFleetCompleted = true);
		}

		[TestMethod]
		public void ReceiveShot_Valid_AtIntactWater()
		{
			const byte shipIJ = 98, shotIJ = 89;
			Game game = GetGameAtPlayingStage(false, shipIJ);
			FireResult fireResult = game.ReceiveShot(new Coord(shotIJ));
			Assert.AreEqual(1, game.OwnIntactShipCount);
			Assert.AreEqual(FireResult.Miss, fireResult);
			Assert.AreEqual(GridTile.ShotWater, game.OwnGrid[shotIJ]);
			Assert.IsTrue(game.IsOwnTurn);
		}
		[TestMethod]
		public void ReceiveShot_Valid_AtLastIntactShip()
		{
			const byte shipIJ = 98;
			Game game = GetGameAtPlayingStage(false, shipIJ);
			FireResult fireResult = game.ReceiveShot(new Coord(shipIJ));
			Assert.AreEqual(0, game.OwnIntactShipCount);
			Assert.AreEqual(FireResult.Drown, fireResult);
			Assert.AreEqual(GridTile.DamagedShip, game.OwnGrid[shipIJ]);
			Assert.IsFalse(game.IsOwnTurn);
		}
		[TestMethod]
		public void ReceiveShot_Valid_AtNotLastIntactShip_Drown()
		{
			const byte drownShipIJ = 98, intactShipIJ = 89;
			Game game = GetGameAtPlayingStage(false, new byte[] { 2 }, g =>
			{
				g[drownShipIJ] = GridTile.IntactShip;
				g[intactShipIJ] = GridTile.IntactShip;
			});
			FireResult fireResult = game.ReceiveShot(new Coord(drownShipIJ));
			Assert.AreEqual(1, game.OwnIntactShipCount);
			Assert.AreEqual(FireResult.Drown, fireResult);
			Assert.AreEqual(GridTile.DamagedShip, game.OwnGrid[drownShipIJ]);
			Assert.IsFalse(game.IsOwnTurn);
		}
		[TestMethod]
		public void ReceiveShot_Valid_AtNotLastIntactShip_Hit()
		{
			const byte shipIJLeft = 98;
			Game game = GetGameAtPlayingStage(false, new byte[] { 0, 1 }, g =>
			{
				g[shipIJLeft] = GridTile.IntactShip;
				g[shipIJLeft + 1] = GridTile.IntactShip;
			});
			FireResult fireResult = game.ReceiveShot(new Coord(shipIJLeft));
			Assert.AreEqual(1, game.OwnIntactShipCount);
			Assert.AreEqual(FireResult.Hit, fireResult);
			Assert.AreEqual(GridTile.DamagedShip, game.OwnGrid[shipIJLeft]);
			Assert.IsFalse(game.IsOwnTurn);
		}
		[TestMethod]
		public void ReceiveShot_Valid_AtShotWater()
		{
			const byte shipIJ = 98, shotIJ = 89;
			Game game = GetGameAtPlayingStage(false, shipIJ);
			game.OwnGrid[shotIJ] = GridTile.ShotWater;
			FireResult fireResult = game.ReceiveShot(new Coord(shotIJ));
			Assert.AreEqual(1, game.OwnIntactShipCount);
			Assert.AreEqual(FireResult.Error400, fireResult);
			Assert.AreEqual(GridTile.ShotWater, game.OwnGrid[shotIJ]);
			Assert.IsFalse(game.IsOwnTurn);
		}
		[TestMethod]
		public void ReceiveShot_Valid_AtDamagedShip()
		{
			const byte shipIJLeft = 98;
			Game game = GetGameAtPlayingStage(false, new byte[] { 0, 1 }, g =>
			{
				g[shipIJLeft] = GridTile.IntactShip;
				g[shipIJLeft + 1] = GridTile.IntactShip;
			});
			game.ReceiveShot(new Coord(shipIJLeft));
			FireResult fireResult = game.ReceiveShot(new Coord(shipIJLeft));
			Assert.AreEqual(1, game.OwnIntactShipCount);
			Assert.AreEqual(FireResult.Error400, fireResult);
			Assert.AreEqual(GridTile.DamagedShip, game.OwnGrid[shipIJLeft]);
			Assert.IsFalse(game.IsOwnTurn);
		}
		[TestMethod]
		public void ReceiveShot_Invalid_AtUncertainty()
		{
			const byte shipIJ = 98, shotIJ = 89;
			Game game = GetGameAtPlayingStage(false, shipIJ);
			game.OwnGrid[shotIJ] = GridTile.Uncertainty;
			Assert.ThrowsException<Exception>(() => game.ReceiveShot(new Coord(shotIJ)));
		}
		[TestMethod]
		public void ReceiveShot_Invalid_OwnTurn()
		{
			const byte shipIJ = 98, shotIJ = 89;
			Game game = GetGameAtPlayingStage(true, shipIJ);
			Assert.ThrowsException<InvalidOperationException>(() => game.ReceiveShot(new Coord(shotIJ)));
		}
		[TestMethod]
		public void ReceiveShot_Invalid_WrongStage()
		{
			const byte shotIJ = 89;
			var game = new Game();
			Assert.ThrowsException<InvalidOperationException>(() => game.ReceiveShot(new Coord(shotIJ)));
		}

		[TestMethod]
		public void Fire_Valid_Miss()
		{
			const byte shipIJ = 98, shotIJ = 89;
			Game game = GetGameAtPlayingStage(true, shipIJ);
			game.Fire(new Coord(shotIJ), FireResult.Miss, c => Assert.Fail());
			Assert.AreEqual(1, game.FoeIntactShipCount);
			Assert.AreEqual(GridTile.ShotWater, game.FoeGrid[shotIJ]);
			Assert.IsFalse(game.IsOwnTurn);
		}
		[TestMethod]
		public void Fire_Valid_Drown()
		{
			Coord shotCoord = new Coord(98);
			var shotNeighbours = new HashSet<Coord>(8) {
				shotCoord.Up, shotCoord.Down, shotCoord.Left, shotCoord.Right,
				shotCoord.UpLeft, shotCoord.UpRight, shotCoord.DownLeft, shotCoord.DownRight,
			};
			Game game = GetGameAtPlayingStage(true, shotCoord.IJ);
			game.Fire(shotCoord, FireResult.Drown, c => Assert.IsTrue(shotNeighbours.Contains(c)));
			Assert.AreEqual(0, game.FoeIntactShipCount);
			Assert.AreEqual(GridTile.DamagedShip, game.FoeGrid[shotCoord]);
			Assert.IsTrue(game.IsOwnTurn);
		}
		[TestMethod]
		public void Fire_Valid_Hit()
		{
			const byte shipIJLeft = 98, shotIJ = 89;
			Game game = GetGameAtPlayingStage(true, new byte[] { 0, 1 }, g =>
			{
				g[shipIJLeft] = GridTile.IntactShip;
				g[shipIJLeft + 1] = GridTile.IntactShip;
			});
			game.Fire(new Coord(shotIJ), FireResult.Hit, c => Assert.Fail());
			Assert.AreEqual(1, game.FoeIntactShipCount);
			Assert.AreEqual(GridTile.DamagedShip, game.FoeGrid[shotIJ]);
			Assert.IsTrue(game.IsOwnTurn);
		}
		[TestMethod]
		public void Fire_Invalid_Error400()
		{
			const byte shipIJ = 98, shotIJ = 89;
			Game game = GetGameAtPlayingStage(true, shipIJ);
			Action act = () => game.Fire(new Coord(shotIJ), FireResult.Error400, c => Assert.Fail());
			Assert.ThrowsException<Exception>(act);
		}
		[TestMethod]
		public void Fire_Invalid_NotUncertainty()
		{
			const byte shipIJ = 98, shotIJ = 89;
			Game game = GetGameAtPlayingStage(true, shipIJ);
			game.FoeGrid[shotIJ] = GridTile.ShotWater;
			Action act = () => game.Fire(new Coord(shotIJ), FireResult.Miss, c => Assert.Fail());
			Assert.ThrowsException<ArgumentException>(act);
		}
		[TestMethod]
		public void Fire_Invalid_FoeTurn()
		{
			const byte shipIJ = 98, shotIJ = 89;
			Game game = GetGameAtPlayingStage(false, shipIJ);
			Action act = () => game.Fire(new Coord(shotIJ), FireResult.Miss, c => Assert.Fail());
			Assert.ThrowsException<InvalidOperationException>(act);
		}
		[TestMethod]
		public void Fire_Invalid_WrongStage()
		{
			const byte shotIJ = 89;
			var game = new Game();
			Action act = () => game.Fire(new Coord(shotIJ), FireResult.Miss, c => Assert.Fail());
			Assert.ThrowsException<InvalidOperationException>(act);
		}

		[TestMethod]
		public void EndPlayingStage_Valid_OwnVictory()
		{
			const byte shotIJ = 98;
			Game game = GetGameAtPlayingStage(true, 123);
			game.Fire(new Coord(shotIJ), FireResult.Drown, null);
			game.EndPlayingStage();
			Assert.AreEqual(GameStage.Over, game.Stage);
			Assert.AreEqual(1, game.OwnIntactShipCount);
			Assert.AreEqual(0, game.FoeIntactShipCount);
		}
		[TestMethod]
		public void EndPlayingStage_Valid_OwnDefeat()
		{
			const byte shipIJ = 98;
			Game game = GetGameAtPlayingStage(false, shipIJ);
			game.ReceiveShot(new Coord(shipIJ));
			game.EndPlayingStage();
			Assert.AreEqual(GameStage.Over, game.Stage);
			Assert.AreEqual(0, game.OwnIntactShipCount);
			Assert.AreEqual(1, game.FoeIntactShipCount);
		}
		[TestMethod]
		public void EndPlayingStage_Invalid_BothPlayersHaveIntactShips()
		{
			Game game = GetGameAtPlayingStage();
			Assert.ThrowsException<InvalidOperationException>(() => game.EndPlayingStage());
		}
		[TestMethod]
		public void EndPlayingStage_Invalid_WrongStage()
		{
			Game game = new Game();
			Assert.ThrowsException<InvalidOperationException>(() => game.EndPlayingStage());
		}
	}
}
