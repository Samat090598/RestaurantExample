#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantExercise.Models;

#endregion

namespace RestaurantExercise
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // создали 32 столика
            RestManager manager = new RestManager(CreateTables(32));

            // пришло 1000 клиентов
            foreach (ClientsGroup clientsGroup in CreateClientsGroups(1000))
            {
                manager.OnArrive(clientsGroup);
            }

            do
            {
                if (manager.TableClients.Count <= 0)
                {
                    continue;
                }

                for (int i = 0; i < manager.TableClients.Count; i++)
                {
                    KeyValuePair<Guid, List<ClientsGroup>> kvp = manager.TableClients.ElementAt(i);
                    if (kvp.Value.Count <= 0)
                    {
                        continue;
                    }

                    for (int j = 0; j < kvp.Value.Count; j++)
                    {
                        ClientsGroup group = kvp.Value[j];
                        // клиент уходит
                        manager.OnLeave(group);
                    }
                }
            }
            while (manager.TotalClients > 0);

            Console.WriteLine($"Total Clients: {manager.TotalClients}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static IEnumerable<Table> CreateTables(int count)
        {
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                // столики от 2 до 6 мест
                yield return new Table(random.Next(2, 7));
            }
        }

        private static IEnumerable<ClientsGroup> CreateClientsGroups(int count)
        {
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                // клиенты от 1 до 6
                yield return new ClientsGroup(random.Next(1, 7));
            }
        }
    }
}
