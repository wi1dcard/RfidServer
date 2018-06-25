using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RfidServer.HttpApis
{
	class GetConfig : BaseRequest
	{
		public GetConfig() : base()
		{
			this.Method = HcHttp.Method.GET;
			this.Uri = "get-rparameter";
		}
	}
}
