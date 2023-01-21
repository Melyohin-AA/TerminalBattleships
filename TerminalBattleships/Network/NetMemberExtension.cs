using System;
using System.Net.Sockets;

namespace TerminalBattleships.Network
{
	public static class NetMemberExtension
	{
		public static byte ReadByte(this INetMember net)
		{
			int read;
			do
			{
				read = net.Stream.ReadByte();
			} while (read == -1);
			return (byte)read;
		}
	}
}
