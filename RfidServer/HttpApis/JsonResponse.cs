using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RfidServer.HttpApis
{
	class JsonResponse
	{
		public bool Success
		{
			get
			{
				return this.status == 0;
			}
		}

		public int status { get; set; }

		public string message { get; set; }

		public JToken data { get; set; }
	}
}
