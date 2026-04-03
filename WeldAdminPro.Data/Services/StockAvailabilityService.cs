using System;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    /// <summary>
    /// Single source of truth for stock availability.
    /// </summary>
    public class StockAvailabilityService
    {
        private readonly StockRepository _stockRepository;

        public StockAvailabilityService()
        {
            _stockRepository = new StockRepository();
        }

        public int GetAvailableQuantity(Guid stockItemId)
        {
            if (stockItemId == Guid.Empty)
                return 0;

            return _stockRepository.GetAvailableQuantity(stockItemId);
        }

        public bool CanIssue(Guid stockItemId, decimal quantity)
        {
            if (quantity <= 0)
                return false;

            return quantity <= GetAvailableQuantity(stockItemId);
        }
    }
}
