using OrderDomain;
using Xunit;

namespace OrderDomain.Tests;

/// <summary>折扣规则的测试：覆盖规则的两个分支与优先级</summary>
public class DiscountRulesTests
{
    [Fact]
    public void 不足3件且不满500元_无折扣()
    {
        var total = new Money(200m);

        var discount = DiscountRules.Calculate(total, itemCount: 2);

        Assert.Equal(Money.Zero(), discount);
    }

    [Fact]
    public void 满3件_打9折()
    {
        var total = new Money(1000m);

        var discount = DiscountRules.Calculate(total, itemCount: 3);

        Assert.Equal(new Money(100m), discount);
    }

    [Fact]
    public void 不满3件但满500元_减50元()
    {
        var total = new Money(600m);

        var discount = DiscountRules.Calculate(total, itemCount: 1);

        Assert.Equal(new Money(50m), discount);
    }

    [Fact]
    public void 满3件且满500元_按9折优先()
    {
        var total = new Money(600m);

        var discount = DiscountRules.Calculate(total, itemCount: 3);

        Assert.Equal(new Money(60m), discount);
    }
}
