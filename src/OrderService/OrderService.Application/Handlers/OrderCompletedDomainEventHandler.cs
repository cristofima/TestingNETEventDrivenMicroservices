using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Events;
using OrderService.Application.Interfaces;
using SharedKernel.Events;

namespace OrderService.Application.Handlers;

public class OrderCompletedDomainEventHandler(IEventPublisher eventPublisher, ILogger<OrderCompletedDomainEventHandler> logger) : INotificationHandler<OrderCompletedDomainEvent>
{
    public async Task Handle(OrderCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling OrderCompletedDomainEvent for OrderId: {OrderId}. Publishing integration event...", notification.Order.Id);
        var integrationEvent = new OrderCompletedIntegrationEvent(
            notification.Order.Id,
            notification.CompletedDate
        );

        try
        {
            await eventPublisher.PublishAsync(integrationEvent, cancellationToken);
            logger.LogInformation("OrderCompletedIntegrationEvent published for OrderId: {OrderId}", notification.Order.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing OrderCompletedIntegrationEvent for OrderId: {OrderId}", notification.Order.Id);
            throw;
        }
    }
}