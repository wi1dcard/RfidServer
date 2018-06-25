using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Configuration;

namespace RfidServer.HttpApis
{
	class BaseRequest : HcHttp.Request
	{
		public static string appKey = ConfigurationManager.AppSettings["AppKey"];
		public static NLog.Logger logger = Program.logger;

		public BaseRequest(JsonRequestBody httpBody = null) : base()
		{
			base.BaseUri = ConfigurationManager.AppSettings["BaseUri"];
			// var Host = ConfigurationManager.AppSettings["Host"];
			// if (!string.IsNullOrWhiteSpace(Host)) base.Headers["Host"] = Host;
			base.Content = httpBody;
		}

		protected JsonResponse SendWithoutLog()
		{
			base.Uri += "?app_key=" + appKey;
			var response = base.Send();
			var result= JsonConvert.DeserializeObject<JsonResponse>(response.Text);
			if (response.StatusCode != HttpStatusCode.OK)
			{
				throw new Exceptions.HttpException("Http status error");
			}
			if (result == null)
			{
				throw new Exceptions.HttpException("Json parse error");
			}
			if (!result.Success)
			{
				throw new Exceptions.HttpException($"Json shows an error: {result.message}");
			}
			return result;
		}

		public new JsonResponse Send()
		{
			try
			{
				var result = this.SendWithoutLog();
				return result;
			}
			catch (Exception ex)
			{
				logger.Warn(ex);
				return null;
			}
		}
	}
}
