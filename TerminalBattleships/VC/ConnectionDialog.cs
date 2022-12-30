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
			Net.ConnectedEvent += () => Console.WriteLine("Done");
			Net.ConnectionFailedEvent += () =>
			{
				if (isServer)
					Console.Write("!");
				else Console.WriteLine("Failed");
			};
			Net.Connect();
		}

		private bool IsServerDialog()
		{
			Console.Write("Are you server? [Y|N] ");
			bool isServer = Console.ReadKey().Key == ConsoleKey.Y;
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
			Console.Write("Enter server ip: ");
			string strIP = Console.ReadLine();
			if (strIP.Length == 0) strIP = "127.0.0.1";
			var ip = IPAddress.Parse(strIP);
			Console.Write("Enter server port: ");
			ushort port = ushort.Parse(Console.ReadLine());
			Net = NetMember.StartClient(new IPEndPoint(ip, port));
		}
	}
}
