using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.Configuration.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RokuAPIGateway
{
    public static class FileConfigurationExtensions
    {
        public static IServiceCollection ConfigureOcelotJson(this IServiceCollection services, IConfiguration configuration)
        {
            services.PostConfigure<FileConfiguration>(fileConfig =>
            {
                IConfigurationSection items = configuration.GetSection($"{nameof(fileConfig.GlobalConfiguration)}");
                
                foreach (FileRoute route in fileConfig.Routes)
                {
                    if (route.DownstreamPathTemplate.Contains("[") && route.DownstreamPathTemplate.Contains("]"))
                    {
                        int start = route.DownstreamPathTemplate.IndexOf("[");
                        int end = route.DownstreamPathTemplate.IndexOf("]");

                        string placeholder = route.DownstreamPathTemplate.Substring(start, end ); 
                        string Envname = placeholder.Remove(placeholder.Length-1, 1).Remove(0, 1);

                        string EnvValue = Environment.GetEnvironmentVariable(Envname);

                        if(EnvValue != null)
                        {
                            route.DownstreamPathTemplate = route.DownstreamPathTemplate.Replace(placeholder, EnvValue);
                        }
                    }
                }
            });

            return services;
        }
    }
}
