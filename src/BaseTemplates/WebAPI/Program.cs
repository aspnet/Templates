using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;

namespace $safeprojectname$
{
    public class Program
    {
        // Entry point for the application.
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseServer("Microsoft.AspNetCore.Server.Kestrel")
                .UseApplicationBasePath(Directory.GetCurrentDirectory())
                .UseDefaultConfiguration()
                .UseIISPlatformHandlerUrl()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
