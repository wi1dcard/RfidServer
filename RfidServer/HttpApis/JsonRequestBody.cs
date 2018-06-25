using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace RfidServer.HttpApis
{
	class JsonRequestBody : HcHttp.RequestBody.Binary
	{
		public JsonRequestBody(JToken json) 
			: base(json.ToString(Newtonsoft.Json.Formatting.None))
		{
			
			this.ContentType = "application/json";
		}
	}
}
