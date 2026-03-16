using TiendaOnline.Abstractions;

namespace TiendaOnline.Services
{
    public class CompositeDiscount : IDiscountStrategy
    {
        private readonly List<IDiscountStrategy> _strategies;

        public CompositeDiscount(IEnumerable<IDiscountStrategy> strategies)
        {
            _strategies = strategies.ToList();
        }

        public decimal Apply(decimal price)
        {
            return _strategies.Aggregate(price, (current, strategy) => strategy.Apply(current));
        }
    }
}
