#region Usings

using System;

#endregion

namespace RestaurantExercise.Models
{
    public class Table
    {
        public Table(int size)
        {
            Id = Guid.NewGuid();
            Size = size;
            FreeSize = size;
        }

        public Guid Id { get; }
        public int Size { get; set; }
        public int FreeSize { get; set; }
        public bool IsFree => FreeSize == Size;
    }
}
