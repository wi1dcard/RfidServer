using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RfidServer
{
	class TagSet
	{
		[JsonProperty(PropertyName = "start_time")]
		public DateTime startTime { get; protected set; }

		[JsonProperty(PropertyName = "end_time")]
		public DateTime endTime { get; protected set; }

		[JsonProperty(PropertyName = "device_id")]
		public int deviceId { get; protected set; }

		[JsonProperty(PropertyName = "count")]
		public int times { get; protected set; }

		[JsonProperty(PropertyName = "epc")]
		public string epc { get; protected set; }

        [JsonProperty(PropertyName = "antennas")]
        public string antennas { get; protected set; }


        public TagSet(Tag tag)
		{
			this.startTime = tag.timestamp;
			this.endTime = tag.timestamp;
			this.deviceId = tag.device.Id;
			this.epc = tag.epc;
            this.antennas = tag.antennas; // 兼容分体设备
		}

		public int Increment(Tag tag)
		{
			//起始时间
			if (tag.timestamp < this.startTime)
			{
				this.startTime = tag.timestamp;
			}
			//结束时间
			if (tag.timestamp > this.endTime)
			{
				this.endTime = tag.timestamp;
			}
			return ++this.times;
		}
	}
}
