using Microsoft.Extensions.DependencyInjection;
using Platform.Core.Events;

namespace Platform.Infrastructure.Events;

public class InProcessEventBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;

    public InProcessEventBus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();

        foreach (var handler in handlers)
        {
            await handler.HandleAsync(@event, cancellationToken);
        }
    }
}
