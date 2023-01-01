using System;
using System.Net;
using TerminalBattleships.Network;

namespace TerminalBattleships.VC
{
	class ConnectionDialog
	{
		public NetMember Net { get; private set; }

		public void Show()
		{
			bool isServer = IsServerDialog();
			if (isServer)
				ServerStartingDialog();
			else ClientStartingDialog();
			Console.Write("Connecting . . . ");
			if (isServer)
				Net.ConnectAsServer(() => Console.Write("!"));
			else
			{
				if (!Net.ConnectAsClient())
				{
					Console.WriteLine("Handshake failed");
					throw new Exception("Can do nothing with handshake failure...");
				}
			}
			Console.WriteLine("Done");
		}

		private bool IsServerDialog()
		{
			Console.Write("Are you server? ");
			bool isServer = Program.MakeDiscreteChoice();
			Console.WriteLine();
			return isServer;
		}

		private void ServerStartingDialog()
		{
			Net = NetMember.StartServer();
			string port = Net.LocalEP.ToString().Substring("127.0.0.1:".Length);
			Console.WriteLine($"Port: {port}");
		}

		private void ClientStartingDialog()
		{
			IPAddress ip = ReadServerIP();
			ushort port = ReadServerPort();
			Net = NetMember.StartClient(new IPEndPoint(ip, port));
		}
		private IPAddress ReadServerIP()
		{
			Console.Write("Enter server ip: ");
			int left = Console.CursorLeft, top = Console.CursorTop;
			while (true)
			{
				string strIP = Console.ReadLine();
				if (strIP.Length == 0) strIP = "127.0.0.1";
				if (IPAddress.TryParse(strIP, out IPAddress ip))
				{
					Console.SetCursorPosition(left, top);
					Console.WriteLine(ip);
					return ip;
				}
				Console.Write("Invalid input. Reenter: ");
				left = Console.CursorLeft;
				top = Console.CursorTop;
			}
		}
		private ushort ReadServerPort()
		{
			Console.Write("Enter server port: ");
			int left = Console.CursorLeft, top = Console.CursorTop;
			while (true)
			{
				if (ushort.TryParse(Console.ReadLine(), out ushort port)) return port;
				Console.Write("Invalid input. Reenter: ");
				left = Console.CursorLeft;
				top = Console.CursorTop;
			}
		}
	}
}
