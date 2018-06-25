using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace RfidServer
{
	public class DevicePool : Dictionary<int, Device>
	{
		public EventHandler<Device.ReadEventArgs> OnRead;
		public EventHandler<Device.ConnectedEventArgs> OnConnected;
		public EventHandler<Device.ErrorEventArgs> OnError;

		private const int ReconnectErrorTimes = 5;

		public async Task<bool> PullAsync()
		{
			var httpResult = await Task.Run(() => new HttpApis.GetDevices().Send());
			if(httpResult == null)
			{
				return false;
			}
			var deviceList = httpResult.data.ToObject<List<Device>>();
			foreach(var device in deviceList)
			{
				if (base.ContainsKey(device.Id))
				{
					continue;
				}
				device.OnRead += this.Reader_OnRead;
				device.OnConnected += this.Reader_OnConnected;
				device.OnError += this.Reader_OnError;

				device.OnRead += this.OnRead;
				device.OnConnected += this.OnConnected;
				device.OnError += this.OnError;
				base.Add(device.Id, device);
			}
			return true;
		}

		public async Task<bool> PushAsync()
		{
			var httpData = JArray.FromObject(this.Values);
			var httpBody = new HttpApis.JsonRequestBody(httpData);
			var httpResult = await Task.Run(() => new HttpApis.SubmitDevices(httpBody).Send());
			return httpResult == null ? false : httpResult.Success;
		}

		public async Task<bool> RefreshAsync()
		{
			return await this.PullAsync() && await this.PushAsync();
		}

		private void Reader_OnRead(object sender, Device.ReadEventArgs e)
		{
			var device = (Device)sender;
			device.ReadAsync();
		}

		private void Reader_OnConnected(object sender, Device.ConnectedEventArgs e)
		{
			var device = (Device)sender;
			if (device.IsConnected)
			{
				device.ReadAsync();
			}
			else
			{
				device.ConnectAsync();
			}
		}

		private void Reader_OnError(object sender, Device.ErrorEventArgs e)
		{
			var device = (Device)sender;
			if (device.IsConnected)
			{
				if (e.ErrorTimes >= ReconnectErrorTimes)
				{
					device.Disconnect();
					device.ConnectAsync();
				}
				else
				{
					device.ReadAsync();
				}
			}
			else
			{
				device.ConnectAsync();
			}
		}
	}
}
