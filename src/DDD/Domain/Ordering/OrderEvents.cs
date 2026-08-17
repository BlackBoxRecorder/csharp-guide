using DddOrderDemo.Domain.Common;

namespace DddOrderDemo.Domain.Ordering;

/// <summary>领域事件基类：聚合内部发生的事实，聚合根负责发布</summary>
public abstract record DomainEvent(Guid AggregateId, DateTime OccurredAt);

/// <summary>订单已创建事件</summary>
public sealed record OrderCreatedEvent(Guid OrderId, Guid CustomerId)
    : DomainEvent(OrderId, DateTime.UtcNow);

/// <summary>订单已支付事件：携带折扣与实付金额</summary>
public sealed record OrderPaidEvent(Guid OrderId, Money TotalAmount, Money Discount, Money PaidAmount)
    : DomainEvent(OrderId, DateTime.UtcNow);
