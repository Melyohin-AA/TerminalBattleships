using System;
using TerminalBattleships.Model;

namespace TerminalBattleships.VC
{
	class FleetBuildingDialog
	{
		private Game game;
		private GridV gridV;
		private GridC gridC;

		public FleetBuildingDialog(Game game, GridV gridV)
		{
			this.game = game ?? throw new ArgumentNullException(nameof(game));
			this.gridV = gridV ?? throw new ArgumentNullException(nameof(gridV));
		}

		public void Show()
		{
			PrintShipSets();
			gridC = new GridC(gridV.GridX, gridV.GridY);
			while (!game.OwnFleetCompleted)
				gridC.Control(CompleteFleetControl, SetShipControl);
		}
		private void CompleteFleetControl()
		{
			if (game.TryCompleteFleet())
				ClearShipSetIndicators();
		}
		private void SetShipControl()
		{
			if (game.OwnGrid[gridC.Cursor] == GridTile.IntactWater)
				game.OwnGrid[gridC.Cursor] = GridTile.IntactShip;
			else if (game.OwnGrid[gridC.Cursor] == GridTile.IntactShip)
				game.OwnGrid[gridC.Cursor] = GridTile.IntactWater;
			else throw new Exception();
			GridV.DrawGridTile(game.OwnGrid[gridC.Cursor]);
			UpdateCurrentShipCount();
		}

		private void PrintShipSets()
		{
			var fleet = new Fleet(game.OwnGrid);
			for (byte i = 0; i < fleet.RankedSetShips.Length; i++)
			{
				var set = fleet.RankedSetShips[i];
				Console.ForegroundColor = ConsoleColor.White;
				Console.SetCursorPosition(gridV.GridX, gridV.GridY + 16 + i);
				Console.Write($"[ /{set.RequiredCount}] ");
				for (byte j = 0; j < set.Rank; j++)
					GridV.DrawGridTile(GridTile.IntactShip);
			}
			Console.ForegroundColor = ConsoleColor.White;
			Console.SetCursorPosition(gridV.GridX, Console.CursorTop + 1);
			Console.Write("[ ]");
			UpdateCurrentShipCount(fleet);
		}

		private void UpdateCurrentShipCount()
		{
			var fleet = new Fleet(game.OwnGrid);
			fleet.Scan();
			UpdateCurrentShipCount(fleet);
		}
		private void UpdateCurrentShipCount(Fleet fleet)
		{
			for (byte i = 0; i < fleet.RankedSetShips.Length; i++)
			{
				var set = fleet.RankedSetShips[i];
				Console.SetCursorPosition(gridV.GridX + 1, gridV.GridY + 16 + i);
				Console.ForegroundColor = (set.CurrentCount == set.RequiredCount) ?
					ConsoleColor.Green : ConsoleColor.Red;
				Console.Write(set.CurrentCount);
			}
			Console.SetCursorPosition(gridV.GridX + 1, Console.CursorTop + 1);
			if (fleet.IsComplete)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write('o');
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write('x');
			}
		}

		private void ClearShipSetIndicators()
		{
			var fleet = new Fleet(game.OwnGrid);
			Console.MoveBufferArea(gridV.GridX, gridV.GridY + 16, 16, fleet.RankedSetShips.Length + 1,
				Console.BufferWidth, 0);
		}
	}
}
