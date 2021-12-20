using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace agent
{
    public class Program
    {
        private static int port;
        
        public static void Main(string[] args)
        {
            port = 12345;
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel((context, serverOptions) => {
                            
                            // TCP listener
                            serverOptions
                                .ListenAnyIP(port, listenOptions => {
                                    listenOptions.UseConnectionHandler<TcpConnectionHandler>();
                                });

                            // HTTP listener
                            serverOptions.ListenAnyIP(5000);
                        })
                        .UseStartup<Startup>();
                });
    }
}
