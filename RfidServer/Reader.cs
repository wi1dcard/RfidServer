using SuperSocket.ProtoBase;
using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RfidServer
{
	public class ReaderFilter : FixedHeaderReceiveFilter<BufferedPackageInfo>
	{
		public ReaderFilter()
			: base(1)
		{

		}

		public override BufferedPackageInfo ResolvePackage(IBufferStream bufferStream)
		{
			return new BufferedPackageInfo("", bufferStream.Buffers);
		}

		protected override int GetBodyLengthFromHeader(IBufferStream bufferStream, int length)
		{
			return bufferStream.ReadByte();
		}
	}

	public class Reader : EasyClient<BufferedPackageInfo>
	{
		public Reader()
		{
			base.Initialize(new ReaderFilter());
		}

		public async new Task Send(byte[] data)
		{
			byte[] fullData;
			using (var stream = new System.IO.MemoryStream())
			{
				stream.WriteByte((byte)(data.Length + 3)); // 长度
				stream.WriteByte(0xFF); // CommAdr
				stream.Write(data, 0, data.Length); // 数据
				var bytes = stream.ToArray();
				var hash = CRC16.Compute(bytes); // 计算CRC16
				stream.Write(hash, 0, hash.Length);
				fullData = stream.ToArray();
			}
			await Task.Run(() => base.Send(fullData));
		}

		public IEnumerable<byte> VerifyResponse(IList<ArraySegment<byte>> data, byte expectCommand, out byte status)
		{
			// 拼接
			var fullData = new List<byte>();
			foreach(var v in data)
			{
				fullData.AddRange(v);
			}
			if(fullData.Count < 6) // 至少6字节
			{
				throw new Exceptions.ReaderException("Response format error");
			}
            var command = fullData.Skip(2).Take(1).Single();
            // Command
            if (command != expectCommand)
			{
				throw new Exceptions.ReaderException($"Response command doesn't match, expect {expectCommand}, given {command}");
			}
			// Status
			status = fullData.Skip(3).Take(1).Single();
			// CRC16
			var crc16 = CRC16.Compute(fullData.ToArray());
			if (!crc16.SequenceEqual(new byte[] { 0, 0 }))
			{
				throw new Exceptions.ReaderException("CRC16 verify failed");
			}
			return fullData.Skip(4).Take(fullData.Count - 4 - 2);
		}
	}
}
