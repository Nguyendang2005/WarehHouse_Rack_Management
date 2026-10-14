using System;

namespace Warehouse.Core
{
    public abstract class Product
    {
        private readonly int _id;
        public int Id
        {
            get => _id;
            init
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Mã số sản phẩm (ID) phải là số nguyên dương (> 0).");
                }
                _id = value;
            }
        }

        private string _sku = string.Empty;
        public string Sku
        {
            get => _sku;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Mã SKU không được để trống.", nameof(value));
                }
                _sku = value.Trim().ToUpperInvariant();
            }
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Tên sản phẩm không được để trống.", nameof(value));
                }
                _name = value.Trim();
            }
        }

        private double _weightKg;
        public double WeightKg
        {
            get => _weightKg;
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Trọng lượng phải là số dương hợp lệ (> 0 kg).");
                }
                _weightKg = value;
            }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Số lượng kiện không được là số âm (>= 0).");
                }
                _quantity = value;
            }
        }

        private DateTime _importDate;
        public DateTime ImportDate
        {
            get => _importDate;
            set
            {
                if (value.Date > DateTime.Today)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Ngày nhập kho không được vượt quá ngày hiện tại.");
                }
                _importDate = value;
            }
        }

        protected Product()
        {
            ImportDate = DateTime.Today;
            WeightKg = 1;
            Quantity = 0;
        }

        protected Product(int id, string sku, string name, double weightKg, int quantity, DateTime importDate) : this()
        {
            Id = id;
            Sku = sku;
            Name = name;
            WeightKg = weightKg;
            Quantity = quantity;
            ImportDate = importDate;
        }

        public abstract decimal CalculateStorageFee(int daysInStorage);
    }
}



