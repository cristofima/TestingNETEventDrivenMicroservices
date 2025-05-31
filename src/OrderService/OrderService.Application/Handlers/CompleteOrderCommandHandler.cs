using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands;
using OrderService.Application.Events;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;

namespace OrderService.Application.Handlers;

public class CompleteOrderCommandHandler(
    IOrderRepository orderRepository,
    IMediator mediator,
    ILogger<CompleteOrderCommandHandler> logger) : IRequestHandler<CompleteOrderCommand, bool>
{
    public async Task<bool> Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling CompleteOrderCommand for OrderId: {OrderId}", request.OrderId);
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            logger.LogWarning("Order {OrderId} not found for completion.", request.OrderId);
            return false;
        }

        // Add domain logic: e.g., order must be in 'Shipped' state to be completed.
        if (order.Status != OrderStatus.Shipped)
        {
            logger.LogWarning("Order {OrderId} is not in Shipped state. Current state: {Status}. Cannot complete.", order.Id, order.Status);
            return false;
        }

        var completedDate = DateTimeOffset.UtcNow;
        order.SetStatus(OrderStatus.Completed);

        await orderRepository.UpdateAsync(order, cancellationToken);
        logger.LogInformation("Order {OrderId} status updated to Completed.", order.Id);

        await mediator.Publish(new OrderCompletedDomainEvent(order, completedDate), cancellationToken);
        return true;
    }
}