using System;
using System.Collections.Generic;
using System.Text;

namespace Warehouse.BLL.Service
{
    internal class WarehouseService
    {
        public bool CheckFloorSafety(
            in int floorIndex,
            out double totalWeight,
            out string warningMessage)
        {
            totalWeight = 0;
            warningMessage = string.Empty;
            // Giả sử có một phương thức GetProductsOnFloor để lấy danh sách sản phẩm trên tầng
            var productsOnFloor = GetProductsOnFloor(floorIndex);
            foreach (var product in productsOnFloor)
            {
                totalWeight += product.WeightKg * product.Quantity;
            }
            if (floorIndex == 3 && totalWeight > 400)
            {
                warningMessage = "NGUY HIỂM: Tầng 4 quá tải!";
                return false;
            }
            return true;
        }
        public int CalculateStorageDays(DateTime importDate, DateTime expiryDate)
        {
            if (importDate > expiryDate)
            {
                throw new ArgumentException("Import date cannot be after expiry date.");
            }
            var daysInStorage = (DateTime.Today - importDate.Date).Days;
            return Math.Max(daysInStorage, 0); // Không tính ngày lưu kho âm
        }
        public string GenerateInventoryReport()
        {
            var reportBuilder = new StringBuilder();
            reportBuilder.AppendLine("SKU\tName\tType\tPosition\tQuantity\tWeight\tStorage Days\tExpiry Status\tStorage Fee");
            // Giả sử có một phương thức GetAllProducts để lấy danh sách tất cả sản phẩm
            var allProducts = GetAllProducts();
            foreach (var product in allProducts)
            {
                if (product == null) continue; // Bỏ qua ô trống
                int storageDays = CalculateStorageDays(product.ImportDate, product.ExpiryDate);
                string expiryStatus = GetExpiryStatus(product.ExpiryDate);
                decimal storageFee = product.CalculateStorageFee(storageDays);
                reportBuilder.AppendLine($"{product.SKU}\t{product.Name}\t{product.Type}\t{product.Position}\t{product.Quantity}\t{product.WeightKg * product.Quantity}\t{storageDays}\t{expiryStatus}\t{storageFee.ToString("C0", new System.Globalization.CultureInfo("vi-VN"))}");
            }
            return reportBuilder.ToString();
        }
        public string GetExpiryStatus(DateTime expiryDate)
        {
            if (expiryDate < DateTime.Today)
            {
                return "Hết hạn";
            }
            else if ((expiryDate - DateTime.Today).Days < 15)
            {
                return "Cận date";
            }
            else
            {
                return "Hợp lệ";
            }
        }
        //Tính phí lưu kho
        public decimal CalculateTotalStorageFee()
        {
            decimal totalFee = 0;
            var allProducts = GetAllProducts();
            foreach (var product in allProducts)
            {
                if (product == null) continue; // Bỏ qua ô trống
                int storageDays = CalculateStorageDays(product.ImportDate, product.ExpiryDate);
                totalFee += product.CalculateStorageFee(storageDays);
            }
            return totalFee;
        }
    }
}
