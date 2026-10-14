using System;
using Warehouse.Core;

namespace Warehouse.DAL.Repositories
{
    public class RackMatrixRepository
    {
        private const int FLOOR_COUNT = 4;
        private const int SLOT_COUNT = 5;

        private readonly Product?[,] _matrix;

        public RackMatrixRepository()
        {
            _matrix = new Product?[FLOOR_COUNT, SLOT_COUNT];
        }

        public int FloorCount => FLOOR_COUNT;
        public int SlotCount => SLOT_COUNT;

        public bool PlaceProduct(int floor, int slot, Product product)
        {
            ArgumentNullException.ThrowIfNull(product);

            if (floor < 0 || floor >= FLOOR_COUNT || slot < 0 || slot >= SLOT_COUNT)
            {
                return false;
            }

            if (_matrix[floor, slot] is not null)
            {
                return false;
            }

            _matrix[floor, slot] = product;
            return true;
        }

        public Product? GetProduct(int floor, int slot)
        {
            if (floor < 0 || floor >= FLOOR_COUNT || slot < 0 || slot >= SLOT_COUNT)
            {
                return null;
            }

            return _matrix[floor, slot];
        }

        public Product?[,] GetAllSlots()
        {
            return _matrix;
        }
    }
}