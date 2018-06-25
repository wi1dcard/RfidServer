using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RfidServer
{
	public class Tag
	{
		public Tag()
		{
			this.timestamp = DateTime.Now;
		}

        public int antennaBits { get; set; }

        public string antennas
        {
            get
            {
                return Convert.ToString(this.antennaBits, 2).PadLeft(4, '0');
            }
        }

        public byte[] epcBytes { get; set; }

		public string epc
		{
			get
			{
				return ByteArrayToString(epcBytes);
			}
		}

		public static string ByteArrayToString(byte[] b)
		{
			return BitConverter.ToString(b).Replace("-", "");
		}

		public Device device { get; set; }

		public DateTime timestamp { get; set; }

    }
}
