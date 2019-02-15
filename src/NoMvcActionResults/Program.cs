using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NoMvcActionResults.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace NoMvcActionResults
{
    public class Program
    {
        public static async Task Main(string[] args) =>
            await WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(s =>
                {
                    s.AddRouting();
                    s.AddMvcCore().AddJsonFormatters().AddXmlSerializerFormatters();
                })
                .Configure(app =>
                {
                    app.UseRouter(r =>
                    {
                        r.MapGet("contacts", async (request, response, routeData) =>
                        {
                            var contacts = new[]
                            {
                                new Contact { Name = "Filip", City = "Zurich" },
                                new Contact { Name = "No Filip", City = "Not Zurich" }
                            };
                            await response.WriteActionResult(new ObjectResult(contacts));
                        });


                        r.MapGet("download", async (request, response, routeData) =>
                        {
                            var file = Path.GetFullPath(Path.Combine("Files", "powershell.pdf"));
                            await response.WriteActionResult(new PhysicalFileResult(file, "application/pdf"));
                        });
                    });
                })
                .Build().RunAsync();
    }
}