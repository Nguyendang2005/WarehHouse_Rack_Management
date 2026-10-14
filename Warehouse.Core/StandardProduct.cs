using System;

namespace Warehouse.Core
{
    public class StandardProduct : Product
    {
        public StandardProduct() : base()
        {
        }

        public StandardProduct(int id, string sku, string name, double weightKg, int quantity, DateTime importDate)
            : base(id, sku, name, weightKg, quantity, importDate)
        {
        }

        public override decimal CalculateStorageFee(int daysInStorage)
        {
            if (daysInStorage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(daysInStorage), "Số ngày lưu kho không được là số âm.");
            }

            return 3000m * Quantity * daysInStorage;
        }
    }
}
