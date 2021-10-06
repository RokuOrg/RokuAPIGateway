using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Ocelot.Responder.Middleware;
using Ocelot.DownstreamRouteFinder.Middleware;
using Ocelot.Multiplexer;
using Ocelot.Request.Middleware;
using Ocelot.RequestId.Middleware;
using Ocelot.Errors;
using Ocelot.LoadBalancer.Middleware;
using Ocelot.DownstreamUrlCreator.Middleware;
using Ocelot.Requester.Middleware;
using System.Net.Http;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace RokuAPIGateway
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOcelot();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            var config = new OcelotPipelineConfiguration {
                AuthenticationMiddleware = async (ctx, next) =>
                {
                    if (!ctx.Request.Headers.ContainsKey("X-JWT-Token"))
                    {
                        await next.Invoke();
                        return;
                    }

                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:7000/");
                    client.DefaultRequestHeaders.Add("X-JWT-Token", (string)ctx.Request.Headers["X-JWT-Token"]);

                    HttpResponseMessage responseMessage = await client.GetAsync("25a57dae7cca4fcfb52327966462310e/user/verify");
                    if(responseMessage == null)
                    {                       
                        ctx.Items.SetError(new UnauthorizedError("Response null", OcelotErrorCode.UnauthenticatedError, 401));
                        return;
                    }
                    
                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        ctx.Items.SetError(new UnauthorizedError($"Response failed: {responseMessage.StatusCode}", OcelotErrorCode.UnauthenticatedError, 401));
                        
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
                      
                        ctx.Items.SetError(new UnauthorizedError("Response false", OcelotErrorCode.UnauthenticatedError, 200));
                        
                        return;
                    }

                    ctx.Items.DownstreamRequest().Headers.Add("X-User-Validated", (string)message.Object.UserId);

                    await next.Invoke();
                }
            };

            app.UseOcelot(config).Wait();

        }
    }


}
