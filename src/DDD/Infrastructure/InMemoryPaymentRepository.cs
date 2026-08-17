using DddOrderDemo.Domain.Payment;

namespace DddOrderDemo.Infrastructure;

/// <summary>内存支付仓储：演示用，仅存在于进程内</summary>
public class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly Dictionary<Guid, Payment> _payments = [];

    public void Add(Payment payment) => _payments[payment.Id] = payment;

    public IReadOnlyList<Payment> GetAll() => _payments.Values.ToList();
}
