using System;
using System.Text;
using Warehouse.Core;
using Warehouse.DAL.Repositories;
using Warehouse.BLL.Validators;

namespace Warehouse.BLL.Service
{
    public class RackManagementService
    {
        private readonly RackMatrixRepository _repository;

        public RackManagementService(RackMatrixRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        // Đặt sản phẩm vào kệ với validation chặt chẽ
        public bool AddProduct(int floorIndex, int slotIndex, Product product)
        {
            ArgumentNullException.ThrowIfNull(product);

            if (floorIndex < 0 || floorIndex >= _repository.FloorCount)
            {
                throw new ArgumentOutOfRangeException(nameof(floorIndex), $"Tầng không hợp lệ! Kho chỉ có {_repository.FloorCount} tầng (1 - {_repository.FloorCount}).");
            }

            if (slotIndex < 0 || slotIndex >= _repository.SlotCount)
            {
                throw new ArgumentOutOfRangeException(nameof(slotIndex), $"Ô chứa không hợp lệ! Kho chỉ có {_repository.SlotCount} ô (1 - {_repository.SlotCount}).");
            }

            SkuValidator.Validate(product.Sku);

            return _repository.PlaceProduct(floorIndex, slotIndex, product);
        }

        // Kiểm tra an toàn tải trọng của 1 tầng (dùng tham số in, out và trả về bool với validator chặt chẽ)
        public bool CheckFloorSafety(in int floorIndex, out double totalWeight, out string warningMessage)
        {
            totalWeight = 0;
            warningMessage = string.Empty;

            // 1. Validator kiểm tra hợp lệ của chỉ số tầng
            if (floorIndex < 0 || floorIndex >= _repository.FloorCount)
            {
                warningMessage = $"[LỖI] Chỉ số tầng không hợp lệ (tầng {floorIndex + 1} vượt ngoài phạm vi 1 - {_repository.FloorCount}).";
                return false;
            }

            Product?[,] matrix = _repository.GetAllSlots();
            StringBuilder warnings = new StringBuilder();

            // 2. Duyệt qua mảng 2 chiều Product[,] tại tầng floorIndex để tính tổng tải trọng: ∑(WeightKg × Quantity)
            for (int slot = 0; slot < _repository.SlotCount; slot++)
            {
                Product? product = matrix[floorIndex, slot];
                if (product is null)
                {
                    continue;
                }

                // Kiểm tra tính toàn vẹn tải trọng kiện hàng
                if (double.IsNaN(product.WeightKg) || double.IsInfinity(product.WeightKg) || product.WeightKg <= 0 || product.Quantity < 0)
                {
                    warnings.AppendLine($"   - [LỖI DỮ LIỆU] Ô {slot + 1} ({product.Sku}): Trọng lượng hoặc số lượng không hợp lệ!");
                    continue;
                }

                totalWeight += product.WeightKg * product.Quantity;

                // Pattern Matching kiểm tra hàng đông lạnh & hàng nguy hiểm
                if (product is PerishableProduct perishable)
                {
                    // Phát hiện dữ liệu rác: ImportDate > ExpiryDate
                    if (perishable.ImportDate.Date > perishable.ExpiryDate.Date)                    
                    {
                        warnings.AppendLine($"   - [CẢNH BÁO RÁC] Ô {slot + 1} ({perishable.Sku}): Ngày nhập ({perishable.ImportDate:yyyy-MM-dd}) > Ngày hết hạn ({perishable.ExpiryDate:yyyy-MM-dd})!");
                    }
                    else if (DateTime.Today > perishable.ExpiryDate.Date)
                    {
                        warnings.AppendLine($"   - [HẾT HẠN] Ô {slot + 1} ({perishable.Sku}): Đã hết hạn vào {perishable.ExpiryDate:yyyy-MM-dd}!");
                    }
                    else
                    {
                        int remainingDays = (perishable.ExpiryDate.Date - DateTime.Today).Days;
                        if (remainingDays < 15)
                        {
                            warnings.AppendLine($"   - [CẬN DATE] Ô {slot + 1} ({perishable.Sku}): Còn {remainingDays} ngày hết hạn (giảm giá 50% phí lưu kho)!");
                        }
                    }
                }
                else if (product is HazardousProduct hazardous)
                {
                    // Quy tắc an toàn kho: Hóa chất dễ cháy nổ không nên để tại tầng cao nhất (Tầng 4)
                    if (hazardous.IsFlammable && floorIndex == 3)
                    {
                        warnings.AppendLine($"   - [CẢNH BÁO PCCC] Ô {slot + 1} ({hazardous.Sku}): Hàng dễ cháy nổ xếp tại Tầng 4 (tầng cao nhất)! Cần lưu ý an toàn.");
                    }
                    if (hazardous.HazardLevel >= 4)
                    {
                        warnings.AppendLine($"   - [MỨC ĐỘ NGUY HIỂM CAO] Ô {slot + 1} ({hazardous.Sku}): Hóa chất cấp {hazardous.HazardLevel}/5.");
                    }
                }
            }

            // 3. Quy định an toàn: Tầng cao nhất (Tầng 4 - index 3) chỉ chịu tải tối đa 400 kg.
            // Nếu vượt, warningMessage = "NGUY HIỂM: Tầng 4 quá tải!" và trả về false.
            if (floorIndex == 3 && totalWeight > 400.0)
            {
                StringBuilder errorSb = new StringBuilder();
                errorSb.Append($"NGUY HIỂM: Tầng 4 quá tải! (Hiện tại: {totalWeight:N2} kg / Tối đa: 400.00 kg)");
                if (warnings.Length > 0)
                {
                    errorSb.AppendLine();
                    errorSb.Append(warnings.ToString().TrimEnd());
                }
                warningMessage = errorSb.ToString();
                return false;
            }

            // 4. Nếu tải trọng đạt chuẩn nhưng có cảnh báo hàng hóa
            if (warnings.Length > 0)
            {
                warningMessage = $"Tầng {floorIndex + 1} tải trọng hợp lệ ({totalWeight:N2} kg) nhưng có cảnh báo:\n" + warnings.ToString().TrimEnd();
                return true;
            }

            // 5. An toàn tuyệt đối
            warningMessage = $"Tầng {floorIndex + 1} an toàn. Tải trọng hiện tại: {totalWeight:N2} kg.";
            return true;
        }

        // Xuất biên bản kiểm kê & phí lưu kho bằng StringBuilder (tuyệt đối không dùng toán tử + trong vòng lặp)
        public string GenerateInventoryReport()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("======================================== BIÊN BẢN KIỂM KÊ & BÁO CÁO PHÍ LƯU KHO ========================================");
            sb.AppendLine(
                $"{"Vị trí",-10} | {"Mã SKU",-16} | {"Loại hàng",-16} | {"Tên SP",-18} | {"Số ngày",-8} | {"Tải trọng (kg)",15} | {"Phí lưu kho (VNĐ)",18}");
            sb.AppendLine(new string('-', 108));

            decimal grandTotalFee = 0;
            double grandTotalWeight = 0;
            int totalItems = 0;

            Product?[,] matrix = _repository.GetAllSlots();

            for (int floor = 0; floor < _repository.FloorCount; floor++)
            {
                for (int slot = 0; slot < _repository.SlotCount; slot++)
                {
                    Product? product = matrix[floor, slot];
                    if (product is null)
                    {
                        continue;
                    }

                    totalItems++;
                    double productWeight = product.WeightKg * product.Quantity;
                    grandTotalWeight += productWeight;

                    // Tính số ngày lưu kho thực tế qua TimeSpan (DateTime.Today - product.ImportDate)
                    TimeSpan duration = DateTime.Today - product.ImportDate.Date;
                    int daysInStorage = Math.Max(0, duration.Days);

                    // Xác định loại hàng bằng Pattern Matching Switch Expression
                    string productTypeName = product switch
                    {
                        StandardProduct => "Tiêu chuẩn",
                        PerishableProduct => "Đông lạnh",
                        HazardousProduct => "Hóa chất nguy hại",
                        _ => "Khác"
                    };

                    // Kiểm tra dữ liệu rác: PerishableProduct có ImportDate > ExpiryDate
                    bool isGarbageData = product is PerishableProduct perishable && perishable.ImportDate.Date > perishable.ExpiryDate.Date;

                    decimal fee = product.CalculateStorageFee(daysInStorage);
                    if (!isGarbageData)
                    {
                        grandTotalFee += fee;
                    }

                    string location = $"T{floor + 1}-Ô{slot + 1}";
                    string feeDisplay = isGarbageData ? "LỖI (RÁC)" : fee.ToString("N0");

                    // Tối ưu hóa StringBuilder, format bảng báo cáo không dùng toán tử + chuỗi trong vòng lặp
                    sb.AppendFormat(
                        "{0,-10} | {1,-16} | {2,-16} | {3,-18} | {4,-8} | {5,15:N2} | {6,18}\n",
                        location,
                        product.Sku,
                        productTypeName,
                        product.Name,
                        daysInStorage,
                        productWeight,
                        feeDisplay);
                }
            }

            sb.AppendLine(new string('-', 108));
            sb.AppendLine($"Tổng số sản phẩm lưu kho: {totalItems}");
            sb.AppendLine($"Tổng trọng lượng lưu kho: {grandTotalWeight:N2} kg");
            sb.AppendLine($"TỔNG TIỀN PHÍ LƯU KHO   : {grandTotalFee:N0} VNĐ");
            sb.AppendLine("========================================================================================================");

            return sb.ToString();
        }

        // Tương thích tên gọi cũ
        public string GenerateStorageFeeReport()
        {
            return GenerateInventoryReport();
        }
    }
}
