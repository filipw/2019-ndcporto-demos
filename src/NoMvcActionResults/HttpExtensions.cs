using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NoMvcActionResults
{
    public static class HttpExtensions
    {
        public static Task WriteActionResult<TActionResult>(this HttpResponse response, TActionResult result)
            where TActionResult : IActionResult
        {
            var executor = response.HttpContext.RequestServices.GetService<IActionResultExecutor<TActionResult>>();

            if (executor == null)
            {
                throw new InvalidOperationException($"No action result executor for {typeof(TActionResult).FullName} registered.");
            }

            var routeData = response.HttpContext.GetRouteData() ?? new RouteData();
            var actionContext = new ActionContext(response.HttpContext, routeData, new ActionDescriptor());

            return executor.ExecuteAsync(actionContext, result);
        }
    }
}