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

            app.UseOcelot((ocelotBuilder, ocelotConfiguration) =>
            {
                //app.UseDownstreamContextMiddleware();
                
                ocelotBuilder.UseResponderMiddleware();

                ocelotBuilder.UseMiddleware<JWTMiddleware>();


                //ocelotBuilder.UseDownstreamRouteFinderMiddleware();
                //ocelotBuilder.UseMultiplexingMiddleware();
                //ocelotBuilder.UseDownstreamRequestInitialiser();
                //ocelotBuilder.UseRequestIdMiddleware();
 
                ocelotBuilder.Use((ctx, next) =>
                {
                    //ctx.Items.SetError(new UnauthorizedError("unauthorized", OcelotErrorCode.UnauthorizedError, 400));
                    return Task.CompletedTask;
                });

                ocelotBuilder.UseLoadBalancingMiddleware();
                //ocelotBuilder.UseDownstreamUrlCreatorMiddleware();
                //ocelotBuilder.UseHttpRequesterMiddleware();
            }).Wait();
        }
    }
}
