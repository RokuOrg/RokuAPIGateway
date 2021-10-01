using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Ocelot.Logging;
using Ocelot.Middleware;
using Ocelot.Responder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RokuAPIGateway
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class JWTMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpResponder _httpResponder;

        public JWTMiddleware(RequestDelegate next, IHttpResponder responder, IOcelotLoggerFactory loggerFactory) : base(loggerFactory.CreateLogger<JWTMiddleware>())
        {
            _next = next;
            _httpResponder = responder;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (!httpContext.Request.Headers.ContainsKey("X-JWT-Token"))
            {
                await _next.Invoke(httpContext);
                return;
            }
            
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://157.90.164.50/");
            client.DefaultRequestHeaders.Add("X-JWT-Token", (string)httpContext.Request.Headers["X-JWT-Token"]);
            
            HttpResponseMessage responseMessage = await client.GetAsync("/api/user/validate");
            if (!responseMessage.IsSuccessStatusCode)
            {
                await ReturnStatus(httpContext, HttpStatusCode.Unauthorized, "Invalid JWT token");

                DownstreamResponse downstreamResponse = new DownstreamResponse(new HttpResponseMessage(HttpStatusCode.Unauthorized));

                await _httpResponder.SetResponseOnHttpContext(httpContext, downstreamResponse);
                return;
            }


            Message message = new Message();

            var contentStream = await responseMessage.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(contentStream);
            using var jsonReader = new JsonTextReader(streamReader);

            JsonSerializer serializer = new JsonSerializer();

            try
            {
                 message = serializer.Deserialize<Message>(jsonReader);
            }
            catch (JsonReaderException)
            {
                Console.WriteLine("Invalid JSON.");
            }
            if (!message.Succes)
            {
                await ReturnStatus(httpContext, HttpStatusCode.Unauthorized, "Invalid JWT token");
                return;
            }

            httpContext.Request.Headers.Add("X-User-Validated", (string)message.Object.Username);
            
            await _next.Invoke(httpContext);
        }

        private static async Task ReturnStatus(HttpContext context, HttpStatusCode statusCode, string msg)
        {
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(msg);

        }
    }
}
