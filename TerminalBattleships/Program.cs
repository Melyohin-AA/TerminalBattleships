using System;
using System.Threading;

namespace TerminalBattleships
{
	class Program
	{
		private static Network.NetMember net;
		private static Network.Justification justification;
		private static Model.Game game;
		private static VC.GridV ownGridV, foeGridV;
		private static VC.PlayerFleetReadyMessage ownFleetReadyMessage, foeFleetReadyMessage;
		private static VC.ControlInstructionsMessage controlInstructionsMessage;

		public static void DisposeKeys()
		{
			while (Console.KeyAvailable) Console.ReadKey(true);
		}
		public static bool MakeDiscreteChoice()
		{
			Console.ForegroundColor = ConsoleColor.White;
			int left = Console.CursorLeft, top = Console.CursorTop;
			Console.Write("[Y|N]");
			bool yes;
			while (true)
			{
				switch (Console.ReadKey(true).Key)
				{
					case ConsoleKey.Y:
						Console.SetCursorPosition(left + 1, top);
						yes = true;
						break;
					case ConsoleKey.N:
						Console.SetCursorPosition(left + 3, top);
						yes = false;
						break;
					default: continue;
				}
				Console.ForegroundColor = ConsoleColor.Black;
				Console.BackgroundColor = ConsoleColor.White;
				Console.Write(yes ? "Y" : "N");
				Console.BackgroundColor = ConsoleColor.Black;
				Console.ForegroundColor = ConsoleColor.White;
				return yes;
			}
		}

		static void Main(string[] args)
		{
			InitConsole();
			PrintShipSets();
			var connectionDialog = new VC.ConnectionDialog();
			connectionDialog.Show();
			net = connectionDialog.Net;
			Console.Write("Ship sets argeement . . . ");
			if (new Network.ShipSetsAgreement(net).TryAgree())
				Console.WriteLine("Done");
			else
			{
				Console.WriteLine("Failed");
				Console.ReadKey(true);
				return;
			}
			controlInstructionsMessage = new VC.ControlInstructionsMessage(0, 0);
			while (true)
			{
				GameSessionInteraction();
				if (!AskIfNewGame()) break;
				Console.Clear();
			}
		}
		private static void GameSessionInteraction()
		{
			Console.CursorVisible = true;
			justification = new Network.Justification(net, new Random());
			KeySharingMessage();
			game = new Model.Game();
			DetWhoIsFirstMessage();
			Console.ReadKey(true);
			Console.Clear();
			net.AddHook(ReceiveFoeEncryptedKeyHandler);
			ownGridV = new VC.GridV(1, 2, game.OwnGrid, true);
			foeGridV = new VC.GridV(24, 2, game.FoeGrid, false);
			ownGridV.DrawAll();
			foeGridV.DrawAll();
			controlInstructionsMessage.Show(VC.ControlInstructionsMessage.Situation.FleetBuilding);
			new VC.FleetBuildingDialog(game, ownGridV).Show();
			controlInstructionsMessage.Hide(VC.ControlInstructionsMessage.Situation.FleetBuilding);
			Console.CursorVisible = false;
			ownFleetReadyMessage = new VC.PlayerFleetReadyMessage(ownGridV);
			ownFleetReadyMessage.Show();
			justification.SendOwnShipEncryptedCoords(game.OwnGrid.GetShipCoords());
			while (!game.FoeFleetCompleted) Thread.Sleep(5);
			Thread.Sleep(1000);
			ownFleetReadyMessage.Hide();
			foeFleetReadyMessage.Hide();
			game.EndFleetBuildingStage();
			while ((game.OwnIntactShipCount > 0) && (game.FoeIntactShipCount > 0))
			{
				if (game.IsOwnTurn)
				{
					controlInstructionsMessage.Show(VC.ControlInstructionsMessage.Situation.OwnTurn);
					Console.CursorVisible = true;
					new VC.OwnTurnDialog(net, game, ownGridV, foeGridV).Show();
					Console.CursorVisible = false;
					controlInstructionsMessage.Hide(VC.ControlInstructionsMessage.Situation.OwnTurn);
				}
				else
				{
					controlInstructionsMessage.Show(VC.ControlInstructionsMessage.Situation.FoeTurn);
					new VC.FoeTurnMonolog(net, game, ownGridV, foeGridV).Show();
					controlInstructionsMessage.Hide(VC.ControlInstructionsMessage.Situation.FoeTurn);
				}
			}
			bool victory = game.OwnIntactShipCount > 0;
			game.EndPlayingStage();
			var justificationView = new VC.JustificationView(justification, game.FoeGrid, foeGridV);
			justificationView.Show();
			new VC.GameSessionResultMessage(17, 22, victory).Show();
		}

		private static void InitConsole()
		{
			Console.OutputEncoding = Console.InputEncoding = System.Text.Encoding.UTF8;
			ConsoleColorsLib.ConsoleColors.ApplyColorValues();
			Console.Title = "Terminal Battleships";
			Console.SetWindowSize(20, 20);
			Console.SetBufferSize(45, 28);
			Console.SetWindowSize(45, 28);
			Console.CursorVisible = false;
			Console.CursorSize = 100;
			Console.ForegroundColor = ConsoleColor.White;
			Console.BackgroundColor = ConsoleColor.Black;
			Console.Clear();
			Console.WriteLine();
			Console.WriteLine("\t-=# Terminal Battleships #=-");
			Console.WriteLine();
			Console.WriteLine($"\tv[{System.Windows.Forms.Application.ProductVersion}]");
			Console.WriteLine("\t[Nt]");
			Console.CursorVisible = true;
			Console.WriteLine();
			Console.ReadKey(true);
		}

		private static void PrintShipSets()
		{
			Console.Write("Fleet:");
			var shipSets = Model.Fleet.MakeShipSets();
			foreach (var set in shipSets)
				Console.Write($" #{set.Rank}x{set.RequiredCount}");
			Console.WriteLine();
		}

		private static void KeySharingMessage()
		{
			Console.Write("Sharing keys . . . ");
			justification.SharePublicKeys();
			Console.WriteLine("Done");
		}
		private static void DetWhoIsFirstMessage()
		{
			Console.Write("Determining who is first . . . ");
			var random = new Network.P2PRandom(new Random());
			random.Generate(net);
			game.DetIsFirstTurnOwn(random.Number, net.IsServer);
			Console.WriteLine("Done");
			Console.WriteLine($"\tYou are " + (game.IsOwnTurn ? "FIRST" : "SECOND"));
		}

		private static void ReceiveFoeEncryptedKeyHandler()
		{
			justification.ReceiveFoeShipEncryptedCoords();
			game.FoeFleetCompleted = true;
			foeFleetReadyMessage = new VC.PlayerFleetReadyMessage(foeGridV);
			foeFleetReadyMessage.Show();
		}

		private static bool AskIfNewGame()
		{
			Console.CursorVisible = true;
			Console.ForegroundColor = ConsoleColor.White;
			Console.SetCursorPosition(0, 0);
			Console.Write("Play new game? ");
			bool yes = MakeDiscreteChoice();
			Console.WriteLine();
			return yes;
		}
	}
}
