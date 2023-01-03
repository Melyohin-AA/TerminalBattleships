using System;
using System.Net.Sockets;

namespace TerminalBattleships.Network
{
	public static class NetMemberExtension
	{
		public static byte ReadByte(this INetMember net)
		{
			int b = net.Stream.ReadByte();
			if (b == -1)
			{
				net.Disconnect();
				throw new SocketException();
			}
			return (byte)b;
		}
	}
}
