using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RfidServer
{
	class TagQueue : List<Tag>
	{
		public DateTime startTime;

		public DateTime endTime;

		public TagQueue() : base(500)
		{
			this.startTime = DateTime.Now;
			this.endTime = DateTime.Now;
		}

		public new void Add(Tag item)
		{
			lock (this)
			{
				base.Add(item);
			}
			this.endTime = DateTime.Now;
		}

		public void Submit()
		{
			Tag[] tags;
			lock (this)
			{
				tags = base.ToArray();
			}
			var tagSets = new Dictionary<string, TagSet>();

			var startTime = this.startTime;
			var endTime = this.endTime;
			this.startTime = DateTime.Now;
			this.endTime = DateTime.Now;

			// 转换
			foreach (var tag in tags)
			{
                var uniqueId = $"{tag.device.Id}-{tag.epc}-{tag.antennas}";
				if (!tagSets.ContainsKey(uniqueId))
				{
					tagSets[uniqueId] = new TagSet(tag);
				}
				tagSets[uniqueId].Increment(tag);
			}

			// 提交
			var httpData = new JObject()
			{
				{"start_time", startTime},
				{"end_time", endTime},
				{"data", JArray.FromObject(tagSets.Values)}
			};
			var httpBody = new HttpApis.JsonRequestBody(httpData);
			var httpResult = new HttpApis.SubmitTags(httpBody).Send();

			if (httpResult != null && httpResult.Success)
			{
				foreach (var tag in tags)
				{
					base.Remove(tag);
				}
			}
			else
			{
				this.startTime = startTime;
				this.endTime = endTime;
			}
		}
	}
}
