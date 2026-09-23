using System.Net.Http.Headers;
using System.Reflection;

using Application;
using Application.Common.Interfaces.Repositorys;
using Application.Common.Interfaces.Services;
using Application.Utils.Interfaces.Mediator;
using Application.Utils.Interfaces.Transaction;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using Infrastructure.Configurations;
using Infrastructure.InternalServices;
using Infrastructure.InternalServices.NightShift;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Context;
using Infrastructure.Pipelines;
using Infrastructure.Pipelines.Guid;
using Infrastructure.Pipelines.Transaction;
using Infrastructure.Repositorys;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        AddPipeline(services);
        AddDatabase(services);
        AddRepositories(services);
        AddInternalServices(services);
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<INightShiftSessionRepository, NightShiftSessionRepository>();
    }

    private static void AddPipeline(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddDatabase(IServiceCollection services)
    {
        services.AddScoped<MigrationManager>();
        services.AddDbContext<SlaisDbContext>();
    }

    private static void AddInternalServices(IServiceCollection services)
    {
        AddMediator(services);
        services.AddSingleton(typeof(ISlaisLogger<>), typeof(SlaisLogger<>));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<INightShiftPromptBuilder, NightShiftPromptBuilder>();
        services.AddHttpClient<ILlmClient, OpenAiLlmClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<NightShiftOptions>>().Value.OpenAi;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
        });
    }

    private static void AddMediator(IServiceCollection services)
    {
        var handlerInterface = typeof(IRequestHandler<,>);

        var handlers = Assembly.GetAssembly(typeof(IApplicationAssemblyMarker))
            ?.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                                    .Where(i => i.IsGenericType &&
                                               i.GetGenericTypeDefinition() == handlerInterface),
                (type, iface) => new { type, iface });

        if (handlers == null)
        {
            return;
        }

        foreach (var handler in handlers)
        {
            services.AddTransient(handler.iface, handler.type);
        }

        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<GuidResolver>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GuidResolverPipeline<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionPipeline<,>));
    }

}
