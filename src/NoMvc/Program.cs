using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NoMvc.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http.Endpoints;

namespace NoMvc
{
    public class Program
    {
        public static async Task Main(string[] args) =>
            await WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(s =>
                {
                    // set up JWT authentication and API policy
                    s.AddAuthenticationAndAuthorization();

                    // set up embedded Identity Server
                    s.AddEmbeddedIdentityServer();

                    s.AddSingleton<InMemoryContactRepository>();
                    s.AddRouting();
                })
                .Configure(app =>
                {
                    app.UseRouting(r => // define all API endpoints
                    {
                        var contactRepo = r.ServiceProvider.GetRequiredService<InMemoryContactRepository>();

                        r.MapGet("contacts", async context =>
                        {
                            var contacts = await contactRepo.GetAll();
                            context.Response.WriteJson(contacts);
                        });

                        r.MapGet("contacts/{id:int}", async context =>
                        {
                            var contact = await contactRepo.Get(Convert.ToInt32(context.GetRouteData().Values["id"]));
                            if (contact == null)
                            {
                                context.Response.StatusCode = 404;
                                return;
                            }

                            context.Response.WriteJson(contact);
                        });

                        r.MapPost("contacts", async context =>
                        {
                            var newContact = context.ReadFromJson<Contact>();
                            if (newContact == null) return;

                            await contactRepo.Add(newContact);

                            context.Response.StatusCode = 201;
                            context.Response.WriteJson(newContact);
                        })
                        .RequireAuthorization(new AuthorizeAttribute() { Policy = "API" });

                        r.MapPut("contacts/{id:int}", async context =>
                        {
                            var updatedContact = context.ReadFromJson<Contact>();
                            if (updatedContact == null) return;

                            updatedContact.ContactId = Convert.ToInt32(context.GetRouteData().Values["id"]);
                            await contactRepo.Update(updatedContact);

                            context.Response.StatusCode = 204;
                        })
                        .RequireAuthorization(new AuthorizeAttribute() { Policy = "API" });

                        r.MapDelete("contacts/{id:int}", async context =>
                        {
                            await contactRepo.Delete(Convert.ToInt32(context.GetRouteData().Values["id"]));
                            context.Response.StatusCode = 204;
                        })
                        .RequireAuthorization("API");
                    });

                    app.UseAuthentication();
                    app.UseAuthorization();

                    app.Map("/identity", id =>
                    {
                        // use embedded identity server to issue tokens
                        id.UseIdentityServer();
                    });
                })
                .Build().RunAsync();
    }
}