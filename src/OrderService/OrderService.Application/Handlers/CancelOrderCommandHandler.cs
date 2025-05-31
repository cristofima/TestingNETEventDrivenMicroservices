using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands;
using OrderService.Application.Events;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;

namespace OrderService.Application.Handlers;

public class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IMediator mediator,
    ILogger<CancelOrderCommandHandler> logger) : IRequestHandler<CancelOrderCommand, bool>
{
    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling CancelOrderCommand for OrderId: {OrderId}", request.OrderId);
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            logger.LogWarning("Order {OrderId} not found for cancellation.", request.OrderId);
            return false;
        }

        switch (order.Status)
        {
            // Add domain logic: e.g., order can only be cancelled if not Shipped or Completed.
            case OrderStatus.Shipped:
            case OrderStatus.Completed:
                logger.LogWarning("Order {OrderId} is already {Status} and cannot be cancelled.", order.Id, order.Status);
                return false;
            case OrderStatus.Cancelled:
                logger.LogInformation("Order {OrderId} is already cancelled.", order.Id);
                return true; // Already in desired state
            case OrderStatus.Pending:
            case OrderStatus.Processing:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var cancelledDate = DateTimeOffset.UtcNow;
        order.SetStatus(OrderStatus.Cancelled);
        order.CancellationReason = request.Reason;

        await orderRepository.UpdateAsync(order, cancellationToken);
        logger.LogInformation("Order {OrderId} status updated to Cancelled. Reason: {Reason}", order.Id, request.Reason ?? "N/A");

        await mediator.Publish(new OrderCancelledDomainEvent(order, cancelledDate, request.Reason), cancellationToken);
        return true;
    }
}