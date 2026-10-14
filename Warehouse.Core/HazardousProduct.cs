using System;

namespace Warehouse.Core
{
    public class HazardousProduct : Product
    {
        private int _hazardLevel;
        public int HazardLevel
        {
            get => _hazardLevel;
            set
            {
                if (value < 1 || value > 5)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Mức độ nguy hại phải nằm trong khoảng từ 1 đến 5.");
                }
                _hazardLevel = value;
            }
        }

        public bool IsFlammable { get; set; }

        public HazardousProduct() : base()
        {
            HazardLevel = 1;
            IsFlammable = false;
        }

        public HazardousProduct(int id, string sku, string name, double weightKg, int quantity, DateTime importDate, int hazardLevel, bool isFlammable)
            : base(id, sku, name, weightKg, quantity, importDate)
        {
            HazardLevel = hazardLevel;
            IsFlammable = isFlammable;
        }

        public override decimal CalculateStorageFee(int daysInStorage)
        {
            if (daysInStorage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(daysInStorage), "Số ngày lưu kho không được là số âm.");
            }
            decimal fee = 20000m * Quantity * daysInStorage;

            if (IsFlammable)
            {
                fee *= 2m;
            }
            return fee;
        }
    }
}
