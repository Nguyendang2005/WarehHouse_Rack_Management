using System;
using System.Linq;
using Warehouse.BLL.Service;
using Warehouse.BLL.Validators;
using Warehouse.Core;
using Warehouse.DAL.Repositories;

namespace WarehHouse_Rack_Management
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            try
            {
                RackMatrixRepository repository = new RackMatrixRepository();
                RackManagementService service = new RackManagementService(repository);
                SeedProducts(repository);

                bool running = true;
                while (running)
                {
                    Console.WriteLine("\n======= QUẢN LÝ KỆ KHO HÀNG THÔNG MINH (SMART RACK) =======");
                    Console.WriteLine("1. Xem sơ đồ ma trận kệ kho (4x5 Grid)");
                    Console.WriteLine("2. Nhập sản phẩm vào ô kệ");
                    Console.WriteLine("3. Kiểm tra an toàn tải trọng các tầng");
                    Console.WriteLine("4. Xuất biên bản kiểm kê & phí lưu kho");
                    Console.WriteLine("0. Thoát");
                    Console.Write("Chọn chức năng: ");

                    string? choice = Console.ReadLine();
                    try
                    {
                        switch (choice)
                        {
                            case "1":
                                DisplayRack(repository);
                                break;
                            case "2":
                                AddProduct(service, repository);
                                break;
                            case "3":
                                CheckSafety(service, repository);
                                break;
                            case "4":
                                Console.WriteLine(service.GenerateInventoryReport());
                                break;
                            case "0":
                                running = false;
                                Console.WriteLine("Đã thoát chương trình. Tạm biệt!");
                                break;
                            default:
                                Console.WriteLine("Lựa chọn không hợp lệ. Vui lòng chọn lại.");
                                break;
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine($"[LỖI] {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[LỖI KHÔNG MONG MUỐN] {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khởi động ứng dụng: {ex.Message}");
            }
        }

        private static void SeedProducts(RackMatrixRepository repository)
        {
            Product[] products =
            {
                new StandardProduct(
                    1, "SKU-STD-0001", "Keyboard", 1.2, 10,
                    DateTime.Today.AddDays(-5)),

                new StandardProduct(
                    2, "SKU-STD-0002", "Mouse", 0.3, 20,
                    DateTime.Today.AddDays(-10)),

                new PerishableProduct(
                    3, "SKU-PRH-0003", "Milk", 1.0, 30,
                    DateTime.Today.AddDays(-3),
                    DateTime.Today.AddDays(10)),

                new PerishableProduct(
                    4, "SKU-PRH-0004", "Yogurt", 0.5, 15,
                    DateTime.Today.AddDays(-2),
                    DateTime.Today.AddDays(20)),

                new HazardousProduct(
                    5, "SKU-HAZ-0005", "Paint", 5.0, 10,
                    DateTime.Today.AddDays(-7), 3, true),

                new HazardousProduct(
                    6, "SKU-HAZ-0006", "Cleaner", 2.0, 10,
                    DateTime.Today.AddDays(-4), 2, false)
            };

            for (int i = 0; i < products.Length; i++)
            {
                int floor = i / repository.SlotCount;
                int slot = i % repository.SlotCount;
                repository.PlaceProduct(floor, slot, products[i]);
            }
        }

        // Chức năng 1: In lưới 4x5, ô có hàng hiển thị SKU màu Green, ô trống [ EMPTY ] màu xám
        private static void DisplayRack(RackMatrixRepository repository)
        {
            Console.WriteLine("\n=============================== SƠ ĐỒ MA TRẬN KỆ KHO (4x5) ===============================");
            Console.WriteLine("          " + string.Join("        ", Enumerable.Range(1, repository.SlotCount).Select(s => $"Ô {s}")));
            Console.WriteLine(new string('-', 90));

            for (int floor = repository.FloorCount - 1; floor >= 0; floor--)
            {
                Console.Write($"Tầng {floor + 1} | ");
                for (int slot = 0; slot < repository.SlotCount; slot++)
                {
                    Product? prod = repository.GetProduct(floor, slot);
                    if (prod != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write($"[{prod.Sku,-13}] ");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("[   EMPTY   ] ");
                        Console.ResetColor();
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine(new string('-', 90));
        }

        // Chức năng 2: Nhập hàng vào ô kệ - Tận dụng trực tiếp Validator của Domain & BLL
        private static void AddProduct(RackManagementService service, RackMatrixRepository repository)
        {
            Console.WriteLine("\n--- THÊM SẢN PHẨM MỚI VÀO KỆ ---");

            // 1. Chọn vị trí và kiểm tra ô trống
            int floor;
            int slot;
            while (true)
            {
                floor = ReadGridIndex($"Chọn Tầng (1-{repository.FloorCount}): ", repository.FloorCount);
                slot = ReadGridIndex($"Chọn Ô chứa (1-{repository.SlotCount}): ", repository.SlotCount);

                Product? existing = repository.GetProduct(floor, slot);
                if (existing != null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[CẢNH BÁO] Vị trí Tầng {floor + 1}, Ô {slot + 1} ĐÃ CÓ HÀNG (SKU: {existing.Sku}, Tên: {existing.Name})!");
                    Console.WriteLine("Vui lòng chọn vị trí khác còn trống!");
                    Console.ResetColor();
                    continue;
                }
                break;
            }

            // 2. Nhập loại sản phẩm
            Console.WriteLine("\nChọn loại sản phẩm:");
            Console.WriteLine("1. Hàng tiêu chuẩn (Standard)");
            Console.WriteLine("2. Hàng đông lạnh (Perishable)");
            Console.WriteLine("3. Hàng hóa chất nguy hại (Hazardous)");
            int type = ReadGridIndex("Chọn loại hàng (1-3): ", 3) + 1;

            // 3. Nhập thông tin chung (Validate trực tiếp qua Domain Model & Validator)
            int id = ReadAndValidate(
                () => ReadInt("Mã số sản phẩm (ID): "),
                val => new StandardProduct { Id = val });

            string sku = ReadAndValidate(
                () => ReadString("Mã SKU (VD: SKU-ABC-1234): "),
                SkuValidator.Validate);

            string name = ReadAndValidate(
                () => ReadString("Tên sản phẩm: "),
                val => new StandardProduct { Name = val });

            double weight = ReadAndValidate(
                () => ReadDouble("Trọng lượng 1 kiện (kg): "),
                val => new StandardProduct { WeightKg = val });

            int quantity = ReadAndValidate(
                () => ReadInt("Số lượng kiện: "),
                val => new StandardProduct { Quantity = val });

            DateTime importDate = ReadAndValidate(
                () => ReadDate("Ngày nhập kho (yyyy-MM-dd): "),
                val => new StandardProduct { ImportDate = val });

            // 4. Khởi tạo theo loại sản phẩm
            Product product;
            switch (type)
            {
                case 1:
                    product = new StandardProduct(id, sku, name, weight, quantity, importDate);
                    break;
                case 2:
                    DateTime expiryDate = ReadAndValidate(
                        () => ReadDate($"Ngày hết hạn (yyyy-MM-dd, sau {importDate:yyyy-MM-dd}): "),
                        val => new PerishableProduct { ImportDate = importDate, ExpiryDate = val });
                    product = new PerishableProduct(id, sku, name, weight, quantity, importDate, expiryDate);
                    break;
                case 3:
                    int hazardLevel = ReadAndValidate(
                        () => ReadInt("Mức độ nguy hại (1-5): "),
                        val => new HazardousProduct { HazardLevel = val });
                    bool isFlammable = ReadBool("Hàng có dễ cháy nổ không? (y/n): ");
                    product = new HazardousProduct(id, sku, name, weight, quantity, importDate, hazardLevel, isFlammable);
                    break;
                default:
                    Console.WriteLine("Loại hàng không hợp lệ.");
                    return;
            }

            // 5. Lưu vào kệ
            bool success = service.AddProduct(floor, slot, product);
            if (success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[THÀNH CÔNG] Đã xếp sản phẩm [{product.Sku}] {product.Name} vào Tầng {floor + 1}, Ô {slot + 1}!");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[THẤT BẠI] Không thể đặt sản phẩm vào vị trí đã chọn!");
                Console.ResetColor();
            }
        }

        // Chức năng 3: Kiểm tra an toàn tải trọng các tầng - Báo đỏ rực nếu quá tải
        private static void CheckSafety(RackManagementService service, RackMatrixRepository repository)
        {
            Console.WriteLine("\n================ KIỂM TRA AN TOÀN TẢI TRỌNG CÁC TẦNG ================");
            for (int floor = 0; floor < repository.FloorCount; floor++)
            {
                bool isSafe = service.CheckFloorSafety(in floor, out double totalWeight, out string message);

                Console.Write($"Tầng {floor + 1} | Tổng tải trọng: {totalWeight,7:N2} kg | ");
                if (!isSafe)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[NGUY HIỂM] {message.Trim()}");
                    Console.ResetColor();
                }
                else if (message.Contains("cảnh báo"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[CẢNH BÁO] {message.Trim()}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[AN TOÀN] {message.Trim()}");
                    Console.ResetColor();
                }
            }
            Console.WriteLine("---------------------------------------------------------------------");
        }

        // Helper validate thông qua action/setter của Domain Model & Validator
        private static T ReadAndValidate<T>(Func<T> readFunc, Action<T> validateAction)
        {
            while (true)
            {
                try
                {
                    T value = readFunc();
                    validateAction(value);
                    return value;
                }
                catch (ArgumentException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[LỖI] {ex.Message} Vui lòng nhập lại!");
                    Console.ResetColor();
                }
            }
        }

        // Đọc chỉ số lưới kệ (1-based to 0-based)
        private static int ReadGridIndex(string prompt, int max)
        {
            while (true)
            {
                int val = ReadInt(prompt);
                if (val >= 1 && val <= max)
                {
                    return val - 1;
                }
                Console.WriteLine($"[LỖI] Vui lòng nhập số trong khoảng từ 1 đến {max}!");
            }
        }

        // Các hàm hỗ trợ parse kiểu dữ liệu cơ bản từ Console
        private static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int value))
                {
                    return value;
                }
                Console.WriteLine("[LỖI] Vui lòng nhập số nguyên hợp lệ!");
            }
        }

        private static double ReadDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (double.TryParse(Console.ReadLine(), out double value) && !double.IsNaN(value) && !double.IsInfinity(value))
                {
                    return value;
                }
                Console.WriteLine("[LỖI] Vui lòng nhập số thực hợp lệ!");
            }
        }

        private static string ReadString(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = (Console.ReadLine() ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    return input;
                }
                Console.WriteLine("[LỖI] Không được để trống nội dung. Vui lòng nhập lại!");
            }
        }

        private static DateTime ReadDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (DateTime.TryParse(Console.ReadLine(), out DateTime value))
                {
                    return value;
                }
                Console.WriteLine("[LỖI] Định dạng ngày tháng không hợp lệ (chuẩn: yyyy-MM-dd)!");
            }
        }

        private static bool ReadBool(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();
                if (input is "y" or "yes" or "true" or "1" or "co" or "có")
                {
                    return true;
                }
                if (input is "n" or "no" or "false" or "0" or "khong" or "không")
                {
                    return false;
                }
                Console.WriteLine("[LỖI] Vui lòng nhập 'y' (Có) hoặc 'n' (Không)!");
            }
        }
    }
}
