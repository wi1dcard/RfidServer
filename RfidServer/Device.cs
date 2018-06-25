using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using SuperSocket.ClientEngine;
using SuperSocket.ProtoBase;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

namespace RfidServer
{
	public class Device
	{
		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "ip")]
		public string IP { get; set; }

		[JsonProperty(PropertyName = "port")]
		public int Port { get; set; }

		[JsonIgnore]
		public Reader Reader { get; private set; }

		//------------------------------------------------

		[JsonIgnore]
		public bool IsConnecting { get; private set; }

		[JsonProperty(PropertyName = "is_activated")]
		public bool IsConnected
		{
			get
			{
				return Reader.IsConnected;
			}
		}

		[JsonProperty(PropertyName = "error_times")]
		public int ErrorTimes { get; private set; }

		[JsonProperty(PropertyName = "timeout")]
		public int Timeout { get; set; }

		private ManualResetEvent Waiting = new ManualResetEvent(false);

		public class ConnectedEventArgs : EventArgs
		{

		}

		public class ReadEventArgs : EventArgs
		{
			public List<Tag> Tags;
		}

		public class ErrorEventArgs : EventArgs
		{
			public Exception Exception { get; set; }

			public int ErrorTimes { get; set; }

			public bool IsBreak { get; set; }
		}

		public event EventHandler<ConnectedEventArgs> OnConnected;

		public event EventHandler<ReadEventArgs> OnRead;

		public event EventHandler<ErrorEventArgs> OnError;

		//------------------------------------------------

		public Device()
		{
			this.Reader = new Reader();
			this.Reader.NewPackageReceived += Reader_NewPackageReceived;
			this.Timeout = 1500;
		}

		private void Reader_NewPackageReceived(object sender, PackageEventArgs<BufferedPackageInfo> e)
		{
			Waiting.Set();
			var tagList = new List<Tag>();
			try
			{
				byte status;
				var epcBytes = this.Reader.VerifyResponse(e.Package.Data, 0x01, out status);
				if(epcBytes.Count() != 0)
				{
					using (var epcStream = new MemoryStream(epcBytes.ToArray()))
					{
                        var antennaBits = (byte)epcStream.ReadByte(); // 兼容分体设备
                        var epcCount = (byte)epcStream.ReadByte();
                        for (int i = 0; i < epcCount; i++)
						{
							var len = epcStream.ReadByte();
							var epc = new byte[len];
							epcStream.Read(epc, 0, len);
                            var tag = new Tag()
                            {
                                antennaBits = antennaBits,
                                epcBytes = epc,
                                device = this,
							};
							tagList.Add(tag);
						}
						if (epcStream.Position != epcStream.Length)
						{
							throw new Exceptions.ReaderException($"Tag count doesn't match ({epcStream.Position} / {epcStream.Length})");
						}
					}
				}
				this.ErrorTimes = 0;
				this.OnRead?.Invoke(this, new ReadEventArgs()
				{
					Tags = tagList
				});
			}
			catch(Exception ex)
			{
				var errorEventArgs = new ErrorEventArgs()
				{
					Exception = ex,
					ErrorTimes = ++this.ErrorTimes
				};
				this.OnError?.Invoke(this, errorEventArgs);
			}
		}

		public async Task ConnectAsync()
		{
			if (this.IsConnecting || this.IsConnected)
			{
				return;
			}

			this.IsConnecting = true;

			try
			{
				var endPoint = new IPEndPoint(IPAddress.Parse(this.IP), this.Port);
				var isConnected = await this.Reader.ConnectAsync(endPoint);
			}
			catch (Exception ex)
			{
				
			}
			
			this.IsConnecting = false;
			this.OnConnected?.Invoke(this, new ConnectedEventArgs());
		}

		public async Task ReadAsync()
		{
			Waiting.Reset();
			try
			{
				await this.Reader.Send(new byte[] { 0x01 });
			}
			catch (Exception ex)
			{
				var errorEventArgs = new ErrorEventArgs()
				{
					Exception = new Exceptions.ReaderException($"Sending error: {ex.Message}")
				};
				this.OnError?.Invoke(this, errorEventArgs);
			}
			//等待读取
			bool Result = true;
			if(this.Timeout != 0)
			{
				Result = await Task.Run(() => Waiting.WaitOne(this.Timeout));
			}
			if (!Result)
			{
				var errorEventArgs = new ErrorEventArgs()
				{
					ErrorTimes = ++this.ErrorTimes,
					Exception = new Exceptions.ReaderException("Reading tags timeout")
				};
				this.OnError?.Invoke(this, errorEventArgs);
			}
		}

		public bool Disconnect()
		{
			return this.Reader.Close().Result;
		}
	}
}
