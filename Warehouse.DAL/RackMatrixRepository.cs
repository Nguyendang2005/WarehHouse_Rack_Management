
using Warehouse.Core.Entities;

namespace Warehouse.DAL.Repositories
{
    public class RackMatrixRepository
    {
        private readonly Product?[,] _matrix;

        public int FloorCount { get; }
        public int SlotCount { get; }

        public RackMatrixRepository()
        {
            FloorCount = 4;
            SlotCount = 5;
            _matrix = new Product?[FloorCount, SlotCount];
        }

        public bool PlaceProduct(int floor, int slot, Product product)
        {
            if (product == null)
                return false;

            if (floor < 0 || floor >= FloorCount ||
                slot < 0 || slot >= SlotCount)
                return false;

            if (_matrix[floor, slot] != null)
                return false;

            _matrix[floor, slot] = product;
            return true;
        }

        public Product? GetProduct(int floor, int slot)
        {
            if (floor < 0 || floor >= FloorCount ||
                slot < 0 || slot >= SlotCount)
                return null;

            return _matrix[floor, slot];
        }

        public Product?[,] GetAllSlots()
        {
            return _matrix;
        }
    }
}