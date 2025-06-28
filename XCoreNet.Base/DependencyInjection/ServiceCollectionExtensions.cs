using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using XCoreNet.Base.Abstractions;
using XCoreNet.Base.Infrastructure;
using XCoreNet.Base.Infrastructure.Repositories;

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
                    // EfRepository<> tipini base olarak alanlar
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
    }
}
