using System;
using System.Threading;
using System.Threading.Tasks;

using ElectricPowerMonitorService.ElectricPower;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ElectricPowerMonitorService {
	public class Program {
		public static async Task Main() {
			var builder = new ConfigurationBuilder()
				.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
				.AddJsonFile("appsettings.json", false, true);
			var configuration = builder.Build();

			var serviceProvider = new ServiceCollection()
				.AddLogging(x => {
					x.AddConfiguration(configuration.GetSection("Logging"));
					x.AddConsole();
				})
				.AddScoped<IConfiguration>(_ => configuration)
				.AddScoped<ElectricPowerMonitor>()
				.BuildServiceProvider();

			var epm = serviceProvider.GetService<ElectricPowerMonitor>();
			var cts = new CancellationTokenSource();
			Console.CancelKeyPress += (_, e) => {
				e.Cancel = true;
				cts.Cancel();
			};
			AppDomain.CurrentDomain.ProcessExit += (_, _) => {
				cts.Cancel();
			};

			// 監視開始
			await epm!.StartAsync();

			// 永久ループ（キャンセルトークンまたは終了シグナルまで待機）
			while (!cts.IsCancellationRequested) {
				var line = Console.ReadLine();
				if (line == null) {
					// stdin が EOF の場合（systemd サービス実行時等）は非同期で待機し、CPU空回りを防止
					try {
						await Task.Delay(Timeout.Infinite, cts.Token);
					} catch (OperationCanceledException) {
						break;
					}
				}
			}

			epm?.Dispose();
		}
	}
}
