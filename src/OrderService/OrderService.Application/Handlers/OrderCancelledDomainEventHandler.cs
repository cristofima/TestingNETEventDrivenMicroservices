using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Events;
using OrderService.Application.Interfaces;
using SharedKernel.Events;

namespace OrderService.Application.Handlers;

public class OrderCancelledDomainEventHandler(IEventPublisher eventPublisher, ILogger<OrderCancelledDomainEventHandler> logger) : INotificationHandler<OrderCancelledDomainEvent>
{
    public async Task Handle(OrderCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling OrderCancelledDomainEvent for OrderId: {OrderId}. Publishing integration event...", notification.Order.Id);
        var integrationEvent = new OrderCancelledIntegrationEvent(
            notification.Order.Id,
            notification.CancelledDate,
            notification.Reason
        );

        try
        {
            await eventPublisher.PublishAsync(integrationEvent, cancellationToken);
            logger.LogInformation("OrderCancelledIntegrationEvent published for OrderId: {OrderId}", notification.Order.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing OrderCancelledIntegrationEvent for OrderId: {OrderId}", notification.Order.Id);
            throw;
        }
    }
}