using DddOrderDemo.Domain.Common;

namespace DddOrderDemo.Domain.Payment;

/// <summary>
/// 支付聚合根：一次支付记录一经创建不可修改。
/// 支付金额在创建时校验，杜绝 0 元或负数支付。
/// </summary>
public class Payment
{
    private Payment(Guid id, Guid orderId, Money amount, DateTime paidAt)
    {
        Id = id;
        OrderId = orderId;
        Amount = amount;
        PaidAt = paidAt;
    }

    /// <summary>工厂方法：创建一笔支付</summary>
    public static Payment Pay(Guid orderId, Money amount)
    {
        if (amount.IsZero)
            throw new InvalidOperationException("支付金额不能为 0");

        return new Payment(Guid.NewGuid(), orderId, amount, DateTime.UtcNow);
    }

    public Guid Id { get; }
    public Guid OrderId { get; }
    public Money Amount { get; }
    public DateTime PaidAt { get; }
}
