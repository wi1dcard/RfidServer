using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RfidServer
{
	static class CRC16
	{
		private const ushort PRESET_VALUE = 0xFFFF;
		private const ushort POLYNOMIAL = 0x8408;

		private static ushort uiCrc16Cal(byte[] pucY)
		{
			ushort uiCrcValue = PRESET_VALUE;
			var ucX = pucY.Length;

			for (int ucI = 0; ucI < ucX; ucI++)
			{
				uiCrcValue = (ushort)(uiCrcValue ^ pucY[ucI]);
				for (int ucJ = 0; ucJ < 8; ucJ++)
				{
					if ((uiCrcValue & 0x0001) != 0)
					{
						uiCrcValue = (ushort)((uiCrcValue >> 1) ^ POLYNOMIAL);
					}
					else
					{
						uiCrcValue = (ushort)(uiCrcValue >> 1);
					}
				}
			}
			return uiCrcValue;
		}

		public static byte[] Compute(params byte[] buffer)
		{
			return BitConverter.GetBytes(uiCrc16Cal(buffer));
		}
	}
}
