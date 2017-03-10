using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Logging;

namespace AspNetServer
{
	class Program
	{
		static void Main(string[] args)
		{
			var host = new WebHostBuilder()
				.UseStartup<Startup>()
				.UseSetting("server.urls", "https://*:5001")
				.UseKestrel(options =>
				{
					options.UseHttps(new HttpsConnectionFilterOptions()
					{
						ServerCertificate = new X509Certificate2("server.pfx", "test"),
						SslProtocols = SslProtocols.Tls12
					});
				})
				.Build();


			using (var cts = new CancellationTokenSource())
			{
				Console.CancelKeyPress += (sender, eventArgs) =>
				{
					cts.Cancel();
					eventArgs.Cancel = true;
				};

				host.Run(cts.Token);
			}
		}
	}
}