#region Usings

using System;

#endregion

namespace RestaurantExercise.Models
{
    public class ClientsGroup
    {
        public ClientsGroup(int size)
        {
            Id = Guid.NewGuid();
            TableId = Guid.Empty;
            Size = size;
        }

        public Guid Id { get; }
        public Guid TableId { get; set; }
        public int Size { get; set; }
        public int WaitCount { get; set; }
    }
}
