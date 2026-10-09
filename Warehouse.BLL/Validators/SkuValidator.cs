using System;
using System.Collections.Generic;
using System.Text;

namespace Warehouse.BLL.Validators
{
    internal class SkuValidator
    {
        //^SKU-[A-Z]{3}-\d{4}$
        public static bool IsValidSku(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                return false;
            }
            // Check if the SKU matches the pattern "SKU-XXX-YYYY"
            var parts = sku.Split('-');
            if (parts.Length != 3 || parts[0] != "SKU")
            {
                return false;
            }
            // Check if the second part is three uppercase letters
            if (parts[1].Length != 3 || !IsAllUppercaseLetters(parts[1]))
            {
                return false;
            }
            // Check if the third part is four digits
            if (parts[2].Length != 4 || !IsAllDigits(parts[2]))
            {
                return false;
            }
            return true;
        }
    }
}
