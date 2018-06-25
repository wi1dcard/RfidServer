using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using NLog;
using System.Runtime.InteropServices;

namespace RfidServer
{
	static class Program
	{
		public static DevicePool devicePool;
		public static TagQueue tagQueue;
		public static ConfigList configList;
		public static Logger logger = LogManager.GetCurrentClassLogger();

		static void Main(string[] args)
		{
			devicePool = new DevicePool();
			tagQueue = new TagQueue();
			configList = new ConfigList();

			devicePool.OnRead += (object sender, Device.ReadEventArgs e) =>
			{
				var device = (Device)sender;
				foreach(var tag in e.Tags)
				{
					tagQueue.Add(tag);
				}
				logger.Info("DeviceId: {0}, TagCount: {1}, Antennas: {2}", device.Id, e.Tags.Count, e.Tags.Count > 0 ? e.Tags[0].antennas : "");
			};

			devicePool.OnError += (object sender, Device.ErrorEventArgs e) =>
			{
				var device = (Device)sender;
				logger.Error(e.Exception, $"{e.Exception.Message}, DeviceId: {device.Id}, ErrorTimes: {e.ErrorTimes}");
			};

			DisableConsoleQuickEdit.Go();

			try
			{
				logger.Info("Init config...");
				while (!configList.RefreshAsync().Result) Thread.Sleep(1000);

				logger.Info("Init devices...");
				while (!devicePool.RefreshAsync().Result) Thread.Sleep(1000);
			}
			catch (Exception ex)
			{
				logger.Fatal(ex);
				return;
			}

			try
			{
				logger.Info("Starting new thread...");
				while (true)
				{
					Task.Run(new Action(WorkThread)).Wait();
				}
			}
			catch (Exception ex)
			{
				logger.Fatal(ex, "Main thread exception");
			}
		}

		static void WorkThread()
		{
			int times = 0;
			while (true)
			{
				logger.Info("Running...");
				if (++times == 10)
				{
					logger.Info("Reading config...");
					configList.RefreshAsync();
					logger.Info("Reading devices...");
					devicePool.RefreshAsync();
					times = 0;
				}
				foreach(var device in devicePool)
				{
					device.Value.ConnectAsync();
				}
				tagQueue.Submit();

				logger.Info("Sleeping...");
				Thread.Sleep(configList.GetInt("INTERVAL", 5000));
			}
		}
	}
}
