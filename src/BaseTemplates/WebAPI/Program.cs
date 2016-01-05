using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;

namespace $safeprojectname$
{
    public class Program
    {
        // Entry point for the application.
        public static void Main(string[] args)
        {
           var configuration = WebApplicationConfiguration.GetDefault(args);

            var application = new WebApplicationBuilder()
                        .UseApplicationBasePath(Directory.GetCurrentDirectory())
                        .UseConfiguration(configuration)
                        .UseStartup<Startup>()
                        .Build();

            application.Run();
        }
    }
}
