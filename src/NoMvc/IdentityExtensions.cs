using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.AccessTokenValidation;
using IdentityServer4.Models;

namespace NoMvc
{
    public static class IdentityExtensions
    {
        public static void AddAuthenticationAndAuthorization(this IServiceCollection s)
        {
            s.AddAuthorization(options =>
            {
                // set up authorization policy for the API
                options.AddPolicy("API", policy =>
                {
                    policy.AddAuthenticationSchemes("Bearer");
                    policy.RequireAuthenticatedUser().RequireClaim("scope", "write");
                });
            })
            .AddAuthorizationPolicyEvaluator()
            .AddAuthentication("Bearer")
            .AddJwtBearer(options =>
            {
                options.Authority = "https://localhost:5001/identity";
                options.Audience = "https://localhost:5001";
            });
        }

        public static void AddEmbeddedIdentityServer(this IServiceCollection s)
        {
            // set up embedded identity server
            s.AddIdentityServer().
                AddTestClients().
                AddTestResources().
                AddDeveloperSigningCredential();
        }
    }

    public static class IdentityServerBuilderTestExtensions
    {
        public static IIdentityServerBuilder AddTestClients(this IIdentityServerBuilder builder)
        {
            return builder.AddInMemoryClients(new[] { new Client
            {
                ClientId = "client1",
                ClientSecrets =
                {
                    new Secret("secret1".Sha256())
                },
                AllowedGrantTypes = new[]
                {
                    GrantType.ClientCredentials
                },
                AllowedScopes = new[]
                {
                    "write"
                }
            }});
        }

        public static IIdentityServerBuilder AddTestResources(this IIdentityServerBuilder builder)
        {
            return builder.AddInMemoryApiResources(new[]
            {
                new ApiResource("https://localhost:5001")
                {
                    Scopes =
                    {
                        new Scope("write")
                    },
                    Enabled = true
                },
            });
        }
    }
}