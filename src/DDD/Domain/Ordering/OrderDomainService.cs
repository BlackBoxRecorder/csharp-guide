using DddOrderDemo.Domain.Common;

namespace DddOrderDemo.Domain.Ordering;

/// <summary>
/// 领域服务：承载跨越多个实体的业务规则（此处为订单折扣规则）。
/// 规则不属于单个实体，也不适合塞进 Order，故独立成领域服务。
/// </summary>
public static class OrderDomainService
{
    /// <summary>
    /// 折扣规则：满 3 件打 9 折（优惠 10%）；否则满 500 元减 50 元；都不满足则无折扣。
    /// </summary>
    public static Money CalculateDiscount(Order order)
    {
        var total = order.TotalAmount;
        var itemCount = order.Items.Sum(item => item.Quantity);

        // 规则一：满 3 件打 9 折，优惠金额为总额的 10%
        if (itemCount >= 3)
            return total * 0.1m;

        // 规则二：未满 3 件但满 500 元，直减 50 元
        if (total.Amount >= 500)
            return new Money(50m, total.Currency);

        return Money.Zero(total.Currency);
    }
}
