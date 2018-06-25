using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RfidServer.HttpApis
{
	class SubmitDevices : BaseRequest
	{
		public SubmitDevices(JsonRequestBody httpBody) : base(httpBody)
		{
			this.Method = HcHttp.Method.POST;
			this.Uri = "set-equipment";
		}
	}
}
