namespace DddOrderDemo.Domain.Payment;

/// <summary>支付仓储接口：领域层定义抽象，基础设施层实现</summary>
public interface IPaymentRepository
{
    void Add(Payment payment);
    IReadOnlyList<Payment> GetAll();
}
