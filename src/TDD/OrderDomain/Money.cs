namespace OrderDomain;

/// <summary>金额值对象：金额与币种绑定，不可变（TDD 演示版）</summary>
public readonly record struct Money(decimal Amount, string Currency = "CNY")
{
    public static Money Zero(string currency = "CNY") => new(0, currency);

    public static Money operator +(Money a, Money b) => new(a.Amount + b.Amount, a.Currency);

    public static Money operator -(Money a, Money b) => new(a.Amount - b.Amount, a.Currency);

    public static Money operator *(Money a, int factor) => new(a.Amount * factor, a.Currency);

    public static Money operator *(Money a, decimal rate) => new(Math.Round(a.Amount * rate, 2), a.Currency);

    public override string ToString() => $"{Amount:F2} {Currency}";
}
