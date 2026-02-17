using TiendaOnline.Abstractions;

namespace TiendaOnline.Services
{
    public class PercentageDiscount : IDiscountStrategy
    {
        private readonly decimal _percentage;

        public PercentageDiscount(decimal percentage)
        {
            _percentage = percentage;
        }

        public decimal Apply(decimal price)
        {
            return price - (price * _percentage / 100m);
        }
    }
}
