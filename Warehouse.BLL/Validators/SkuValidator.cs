using System;
using System.Text.RegularExpressions;

namespace Warehouse.BLL.Validators
{
    public static class SkuValidator
    {
        private static readonly Regex SkuPattern =
            new Regex(@"^SKU-[A-Z]{3}-\d{4}$", RegexOptions.Compiled);

        public static bool IsValid(string? sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                return false;
            }
            return SkuPattern.IsMatch(sku.Trim().ToUpperInvariant());
        }

        public static void Validate(string? sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                throw new ArgumentException("Mã SKU không được để trống.", nameof(sku));
            }

            if (!SkuPattern.IsMatch(sku.Trim().ToUpperInvariant()))
            {
                throw new ArgumentException("Mã SKU không đúng định dạng chuẩn! Mẫu: SKU-[3 chữ cái]-[4 chữ số] (Ví dụ: SKU-ABC-1234, SKU-STD-0001).", nameof(sku));
            }
        }
    }
}
