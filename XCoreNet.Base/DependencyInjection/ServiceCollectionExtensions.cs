using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.Reflection;
using XCoreNet.Base.Abstractions;
using XCoreNet.Base.Infrastructure;
using XCoreNet.Base.Infrastructure.Repositories;
using XCoreNet.Base.Infrastructure.Security;
using XCoreNet.Base.Utilities;

namespace XCoreNet.Base.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEfRepositories(this IServiceCollection services, Assembly? assembly = null)
        {
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

            var baseType = typeof(EfRepository<>);

            var targetAssembly = assembly ?? Assembly.GetExecutingAssembly();

            var types = targetAssembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Where(t =>
                    t.BaseType != null &&
                    t.BaseType.IsGenericType &&
                    t.BaseType.GetGenericTypeDefinition() == baseType)
                .ToList();

            foreach (var implType in types)
            {
                var interfaceType = implType.GetInterfaces().FirstOrDefault(i => i.Name == $"I{implType.Name}");

                if (interfaceType != null)
                {
                    services.AddScoped(interfaceType, implType);
                }
                else
                {
                    services.AddScoped(implType);
                }
            }

            return services;
        }

        public static IServiceCollection AddEfUnitOfWork<TContext>(this IServiceCollection services)
        where TContext : DbContext
        {
            services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();

            return services;
        }

        public static IServiceCollection AddHelpers(this IServiceCollection services)
        {
            services.AddScoped<IHttpHelper, HttpHelper>();

            return services;
        }

        public static IServiceCollection AddSecurityServices(this IServiceCollection services)
        {
            services.AddScoped<IHashingService, HashingService>();

            return services;
        }

        public static IHostBuilder AddSeriLogServices(this IHostBuilder configureHost)
        {
            configureHost.UseSerilog((ctx, lc) =>
            {
                var config = ctx.Configuration;
                var logDirectory = config["Serilog:LogDirectory"] ?? "Logs";
                var minimumLevel = config["Serilog:MinimumLevel"] ?? "Information";

                lc.MinimumLevel.Is(ParseLogLevel(minimumLevel));

                var overrides = config.GetSection("Serilog:Override").GetChildren();
                foreach (var section in overrides)
                {
                    lc.MinimumLevel.Override(section.Key, ParseLogLevel(section.Value));
                }

                lc.Enrich.FromLogContext()
                  .WriteTo.Console()
                  .WriteTo.File(
                      path: $"{logDirectory}/{Assembly.GetExecutingAssembly().GetName().Name}.txt",
                      rollingInterval: RollingInterval.Day
                  )
                  .ReadFrom.Configuration(config);
            });

            return configureHost;
        }

        private static LogEventLevel ParseLogLevel(string level)
        {
            return Enum.TryParse<LogEventLevel>(level, true, out var parsedLevel)
                ? parsedLevel
                : LogEventLevel.Information;
        }
    }
}
