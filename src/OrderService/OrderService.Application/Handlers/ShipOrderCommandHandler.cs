using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands;
using OrderService.Application.Events;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;

namespace OrderService.Application.Handlers;

public class ShipOrderCommandHandler(
    IOrderRepository orderRepository,
    IMediator mediator,
    ILogger<ShipOrderCommandHandler> logger) : IRequestHandler<ShipOrderCommand, bool>
{
    public async Task<bool> Handle(ShipOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ShipOrderCommand for OrderId: {OrderId}", request.OrderId);
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            logger.LogWarning("Order {OrderId} not found for shipping.", request.OrderId);
            return false;
        }

        // Add domain logic: e.g., order must be in 'Processing' state to be shipped.
        if (order.Status != OrderStatus.Processing)
        {
            logger.LogWarning("Order {OrderId} is not in Processing state. Current state: {Status}. Cannot ship.", order.Id, order.Status);
            return false;
        }

        var shippedDate = DateTimeOffset.UtcNow;
        order.SetStatus(OrderStatus.Shipped);
        order.TrackingNumber = request.TrackingNumber;

        await orderRepository.UpdateAsync(order, cancellationToken);
        logger.LogInformation("Order {OrderId} status updated to Shipped. Tracking: {TrackingNumber}", order.Id, request.TrackingNumber ?? "N/A");

        await mediator.Publish(new OrderShippedDomainEvent(order, shippedDate, request.TrackingNumber), cancellationToken);
        return true;
    }
}