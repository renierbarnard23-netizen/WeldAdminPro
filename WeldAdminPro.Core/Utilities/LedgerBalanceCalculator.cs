using System;

namespace WeldAdminPro.Core.Utilities
{
    public static class LedgerBalanceCalculator
    {
        public static int CalculateNext(
            int previousBalance,
            int quantityIn,
            int quantityOut)
        {
            var next = previousBalance + quantityIn - quantityOut;

            if (next < 0)
                throw new InvalidOperationException(
                    "Stock balance cannot go below zero.");

            return next;
        }
    }
}
