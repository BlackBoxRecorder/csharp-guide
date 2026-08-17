namespace DddOrderDemo.Domain.Common;

/// <summary>
/// 金额值对象：金额与币种绑定，一经创建不可修改。
/// 两个金额运算时校验币种一致，避免"元"与"美元"直接相加。
/// </summary>
public readonly record struct Money(decimal Amount, string Currency = "CNY")
{
    public static Money Zero(string currency = "CNY") => new(0, currency);

    public bool IsZero => Amount == 0;

    public static Money operator +(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator *(Money a, int factor) => new(a.Amount * factor, a.Currency);

    public static Money operator *(Money a, decimal rate) => new(Math.Round(a.Amount * rate, 2), a.Currency);

    private static void EnsureSameCurrency(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException($"币种不一致：{a.Currency} 与 {b.Currency}");
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
