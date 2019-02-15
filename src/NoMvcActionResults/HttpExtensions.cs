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
        public static Task WriteActionResult<TResult>(this HttpResponse response, TResult result)
            where TResult : IActionResult
        {
            var executor = response.HttpContext.RequestServices.GetService<IActionResultExecutor<TResult>>();

            if (executor == null)
            {
                throw new InvalidOperationException($"No action result executor for {typeof(TResult).FullName} registered.");
            }

            var routeData = response.HttpContext.GetRouteData() ?? new RouteData();
            var actionContext = new ActionContext(response.HttpContext, routeData, new ActionDescriptor());

            return executor.ExecuteAsync(actionContext, result);
        }
    }
}