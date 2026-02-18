using TiendaOnline.Abstractions;

namespace TiendaOnline.Services
{
    public class FixedAmountDiscount : IDiscountStrategy
    {
        private readonly decimal _amount;

        public FixedAmountDiscount(decimal amount)
        {
            _amount = amount;
        }

        public decimal Apply(decimal price)
        {
            var result = price - _amount;
            return result < 0 ? 0 : result;
        }
    }
}
