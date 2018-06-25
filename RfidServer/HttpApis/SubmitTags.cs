using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RfidServer.HttpApis
{
	class SubmitTags : BaseRequest
	{
		public SubmitTags(JsonRequestBody httpBody) : base (httpBody)
		{
			this.Method = HcHttp.Method.POST;
			this.Uri = "insert-record";
		}
	}
}
