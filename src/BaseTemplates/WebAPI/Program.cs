using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Builder;

namespace $safeprojectname$
{
    public class Program
    {
        // Entry point for the application.
        public static void Main(string[] args)
        {
            var hostingConfiguration = WebApplicationConfiguration.GetDefault(args);

            var application = new WebApplicationBuilder()
                .UseServerFactory("Microsoft.AspNet.Server.Kestrel")
                .UseConfiguration(hostingConfiguration)
                .UseStartup<Startup>()
                .Build();

            application.Run();
        }
    }
}
