using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Events;
using OrderService.Application.Interfaces;
using SharedKernel.Events;

namespace OrderService.Application.Handlers;

public class OrderShippedDomainEventHandler(IEventPublisher eventPublisher, ILogger<OrderShippedDomainEventHandler> logger) : INotificationHandler<OrderShippedDomainEvent>
{
    public async Task Handle(OrderShippedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling OrderShippedDomainEvent for OrderId: {OrderId}. Publishing integration event...", notification.Order.Id);
        var integrationEvent = new OrderShippedIntegrationEvent(
            notification.Order.Id,
            notification.ShippedDate,
            notification.TrackingNumber
        );

        try
        {
            await eventPublisher.PublishAsync(integrationEvent, cancellationToken);
            logger.LogInformation("OrderShippedIntegrationEvent published for OrderId: {OrderId}", notification.Order.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing OrderShippedIntegrationEvent for OrderId: {OrderId}", notification.Order.Id);
            throw;
        }
    }
}