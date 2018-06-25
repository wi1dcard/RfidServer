using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RfidServer
{
	class ConfigList : Dictionary<string, string>
	{
		public async Task<bool> RefreshAsync()
		{
			var httpResult = await Task.Run(() => new HttpApis.GetConfig().Send());
			if (httpResult == null)
			{
				return false;
			}
			foreach(var config in httpResult.data)
			{
				try
				{
					this[config["key"].ToString()] = config["value"].ToString();
				}
				catch
				{

				}
			}
			return true;
		}

		public string GetString(string key, string defaultValue = "")
		{
			return this.ContainsKey(key) ? this[key] : defaultValue;
		}

		public int GetInt(string key, int defaultValue = 0)
		{
			string val = this.GetString(key, null);
			int valInt;
			if (val != null && int.TryParse(val, out valInt))
			{
				return valInt;
			}
			return defaultValue;
		}
	}
}
