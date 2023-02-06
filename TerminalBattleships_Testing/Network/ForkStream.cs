using System;
using System.IO;
using System.Collections.Concurrent;

namespace TerminalBattleships_Testing.Network
{
	class ForkStream : Stream
	{
		public ConcurrentQueue<byte> Input { get; }
		public ConcurrentQueue<byte> Output { get; }
		public int Available => Input.Count;

		public override bool CanRead => true;
		public override bool CanWrite => true;
		public override bool CanSeek => false;

		public override long Length => Input.Count;
		public override long Position { get => 0; set => throw new NotSupportedException(); }

		public ForkStream(ConcurrentQueue<byte> input, ConcurrentQueue<byte> output)
		{
			Input = input ?? throw new ArgumentNullException(nameof(input));
			Output = output ?? throw new ArgumentNullException(nameof(output));
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int end = offset + count;
			for (int i = offset; i < end; i++)
			{
				if (Input.Count == 0)
					return i - offset;
				while (!Input.TryDequeue(out buffer[i]))
					System.Threading.Thread.Sleep(5);
			}
			return count;
		}
		public override void Write(byte[] buffer, int offset, int count)
		{
			int end = offset + count;
			for (int i = offset; i < end; i++)
				Output.Enqueue(buffer[i]);
		}

		public override void Flush() { }
		public override void SetLength(long value) => throw new NotSupportedException();
		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

		public int SkipBytes(int count)
		{
			for (int i = 0; i < count; i++)
			{
				if (Input.Count == 0)
					return i;
				while (!Input.TryDequeue(out byte _))
					System.Threading.Thread.Sleep(5);
			}
			return count;
		}
	}
}
