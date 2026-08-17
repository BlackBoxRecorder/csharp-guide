using OrderDomain;
using Xunit;

namespace OrderDomain.Tests;

/// <summary>订单聚合的测试：先写失败测试，再实现功能（红-绿循环）</summary>
public class OrderTests
{
    [Fact]
    public void 单件商品_总额等于单价乘数量()
    {
        var order = new Order();
        order.AddItem("机械键盘", new Money(899m), 2);

        Assert.Equal(new Money(1798m), order.TotalAmount);
    }

    [Fact]
    public void 多件商品_总额等于各行小计之和()
    {
        var order = new Order();
        order.AddItem("机械键盘", new Money(899m), 2);
        order.AddItem("显示器", new Money(1299m), 1);

        Assert.Equal(new Money(3097m), order.TotalAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void 商品数量必须大于零(int quantity)
    {
        var order = new Order();

        Assert.Throws<ArgumentException>(() => order.AddItem("机械键盘", new Money(899m), quantity));
    }

    [Fact]
    public void 已支付订单_不能再添加商品()
    {
        var order = new Order();
        order.AddItem("机械键盘", new Money(899m), 1);
        order.Pay();

        Assert.Throws<InvalidOperationException>(() => order.AddItem("显示器", new Money(1299m), 1));
    }

    [Fact]
    public void 重复支付_抛出异常()
    {
        var order = new Order();
        order.AddItem("机械键盘", new Money(899m), 1);
        order.Pay();

        Assert.Throws<InvalidOperationException>(() => order.Pay());
    }

    [Fact]
    public void 取消后_状态变为已取消()
    {
        var order = new Order();
        order.AddItem("机械键盘", new Money(899m), 1);
        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }
}
