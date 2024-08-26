using EF.Core.Commons.Messages;
using EF.Infra.Commons.EventBus;
using EF.Pagamentos.Application.Events;
using EF.Pagamentos.Application.Events.Messages;

namespace EF.Api.Commons.Config;

public static class EventBusConfig
{
    public static IServiceCollection AddEventBusConfig(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        // Pagamentos
        services.AddScoped<IEventHandler<PagamentoAutorizadoEvent>, PagamentoEventHandler>();
        services.AddScoped<IEventHandler<PagamentoRecusadoEvent>, PagamentoEventHandler>();

        return services;
    }

    public static WebApplication SubscribeEventHandlers(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var bus = services.GetRequiredService<IEventBus>();
        
        services.GetRequiredService<IEnumerable<IEventHandler<PagamentoAutorizadoEvent>>>().ToList()
            .ForEach(e => bus.Subscribe(e));

        services.GetRequiredService<IEnumerable<IEventHandler<PagamentoRecusadoEvent>>>().ToList()
            .ForEach(e => bus.Subscribe(e));

        return app;
    }
}