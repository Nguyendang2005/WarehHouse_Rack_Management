using System;

namespace Warehouse.Core
{
    public class PerishableProduct : Product
    {
        private DateTime _expiryDate;
        public DateTime ExpiryDate
        {
            get => _expiryDate;
            set
            {
                if (value.Date <= ImportDate.Date || value.Date < DateTime.Today)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Ngày hết hạn phải sau ngày nhập kho và không được ở trong quá khứ.");
                }
                _expiryDate = value;
            }
        }

        public PerishableProduct() : base()
        {
            ExpiryDate = ImportDate.AddDays(30);
        }

        public PerishableProduct(int id, string sku, string name, double weightKg, int quantity, DateTime importDate, DateTime expiryDate)
            : base(id, sku, name, weightKg, quantity, importDate)
        {
            ExpiryDate = expiryDate;
        }

        public override decimal CalculateStorageFee(int daysInStorage)
        {
            if (daysInStorage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(daysInStorage), "Số ngày lưu kho không được là số âm.");
            }
            decimal fee = 8000m * Quantity * daysInStorage;
            int daysUntilExpiry = (ExpiryDate.Date - DateTime.Today).Days;
            if (daysUntilExpiry < 15)
            {
                fee *= 0.5m;
            }
            return fee;
        }
    }
}
