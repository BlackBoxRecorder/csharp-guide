namespace OrderDomain;

/// <summary>
/// 折扣规则：满 3 件打 9 折（优惠 10%）；否则满 500 元减 50 元；都不满足则无折扣。
/// 规则独立成类，方便单独测试。
/// </summary>
public static class DiscountRules
{
    public static Money Calculate(Money total, int itemCount)
    {
        // 规则一：满 3 件打 9 折，优惠金额为总额的 10%
        if (itemCount >= 3)
            return total * 0.1m;

        // 规则二：未满 3 件但满 500 元，直减 50 元
        if (total.Amount >= 500)
            return new Money(50m, total.Currency);

        return Money.Zero(total.Currency);
    }
}
