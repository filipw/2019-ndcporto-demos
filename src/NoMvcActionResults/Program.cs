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
using WebApiContrib.Core.Results;

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

                            var objectResult = new ObjectResult(contacts);
                            await response.WriteActionResult(objectResult);
                        });


                        r.MapGet("download", async (request, response, routeData) =>
                        {
                            var path = Path.GetFullPath(Path.Combine("Files", "powershell.pdf"));
                            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 
                                64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);

                            await response.WriteActionResult(new FileStreamResult(fileStream, "application/pdf"));
                        });

                        #region simpler
                        r.MapGet("simpler/contacts", async (request, response, routeData) =>
                        {
                            var contacts = new[]
                            {
                                new Contact { Name = "Filip", City = "Zurich" },
                                new Contact { Name = "No Filip", City = "Not Zurich" }
                            };

                            // from WebApiContrib.Core
                            await response.HttpContext.Ok(contacts);
                        });


                        r.MapGet("simpler/download", async (request, response, routeData) =>
                        {
                            var path = Path.GetFullPath(Path.Combine("Files", "powershell.pdf"));

                            // from WebApiContrib.Core
                            await response.HttpContext.PhysicalFile(path, "application/pdf");
                        });

                        #endregion
                    });
                })
                .Build().RunAsync();
    }
}