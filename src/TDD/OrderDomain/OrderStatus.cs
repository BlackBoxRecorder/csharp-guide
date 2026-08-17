namespace OrderDomain;

/// <summary>订单状态：状态流转由聚合根内部控制</summary>
public enum OrderStatus
{
    Pending,   // 待支付
    Paid,      // 已支付
    Cancelled, // 已取消
}
