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

        public static Task WriteObjectResult(this HttpResponse response, object result)
        {
            var executor = response.HttpContext.RequestServices.GetService<IActionResultExecutor<ObjectResult>>();

            if (executor == null)
            {
                throw new InvalidOperationException($"No action result executor for {typeof(ObjectResult)} registered.");
            }

            var routeData = response.HttpContext.GetRouteData() ?? new RouteData();
            var actionContext = new ActionContext(response.HttpContext, routeData, new ActionDescriptor());

            return executor.ExecuteAsync(actionContext, new ObjectResult(result));
        }

        private static readonly JsonSerializer Serializer = new JsonSerializer();

        public static void WriteJson<T>(this HttpResponse response, T obj)
        {
            response.ContentType = "application/json";
            using (var writer = new HttpResponseStreamWriter(response.Body, Encoding.UTF8))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    jsonWriter.CloseOutput = false;
                    jsonWriter.AutoCompleteOnClose = false;

                    Serializer.Serialize(jsonWriter, obj);
                }
            }
        }

        public static T ReadFromJson<T>(this HttpContext httpContext)
        {
            using (var streamReader = new StreamReader(httpContext.Request.Body))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                var obj = Serializer.Deserialize<T>(jsonTextReader);

                var results = new List<ValidationResult>();
                if (Validator.TryValidateObject(obj, new ValidationContext(obj), results))
                {
                    return obj;
                }

                httpContext.Response.StatusCode = 400;
                httpContext.Response.WriteJson(results);

                return default(T);
            }
        }
    }
}