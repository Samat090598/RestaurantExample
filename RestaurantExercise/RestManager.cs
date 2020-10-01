#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantExercise.Models;

#endregion

namespace RestaurantExercise
{
    public class RestManager
    {

        private readonly List<ClientsGroup> _clientQueue;
        private readonly List<Table> _tables;

        public RestManager(IEnumerable<Table> tables)
        {
            _tables = new List<Table>(tables);
            _clientQueue = new List<ClientsGroup>();
            TableClients = new Dictionary<Guid, List<ClientsGroup>>();
        }

        // Проще было бы сделать Dictionary<Table, List<ClientsGroup>> но Reference Type в качестве ключа не есть правильно, поэтому ключ - Table.Id 
        public Dictionary<Guid, List<ClientsGroup>> TableClients { get; }
        public int TotalClients => _clientQueue.Count + TableClients.Sum(t => t.Value.Count);

        // new client(s) show up
        public void OnArrive(ClientsGroup group)
        {
            // Получаем столики по размеру равным или больше чем количество людей
            List<Table> freeTables = _tables.Where(t => t.FreeSize >= group.Size).ToList();
            if (!freeTables.Any())
            {
                _clientQueue.Add(group);
                return;
            }
            // Сортируем по размеру пустых столиков. Это нужно для того чтобы получить столик который меньше размером. Например чтобы не посадить 2 людей на столик 6-х
            freeTables = freeTables.OrderBy(t => t.FreeSize).ToList();
            // Получаем столик который свободен или в который уже сидят люди но есть свободные места
            Table freeTable = freeTables.FirstOrDefault(t => t.IsFree) 
                              ?? freeTables.FirstOrDefault(t => !t.IsFree);
            PutTheTable(freeTable, group);
            // Если есть клиенты в очереди, проверяем их состояние
            if (_clientQueue.Count > 0)
            {
                ClientStatusInTheQueue(new List<ClientsGroup>());   
            }
        }

        // client(s) leave, either served or simply abandoning the queue
        public void OnLeave(ClientsGroup group)
        {
            // за каким столиком сидит клиент
            Table table = Lookup(group);

            if (table != null)
            {
                // удаляем клиента
                TableClients[group.TableId].RemoveAll(client => client.Id == group.Id);

                // увеличиваем кол-во свободных мест столика
                table.FreeSize += group.Size;

                // ищем подходящего клиента из очереди
                ClientsGroup fromQueueClient = _clientQueue.FirstOrDefault(client => table.FreeSize >= client.Size);
                
                if (fromQueueClient == null)
                {
                    return;
                }
                
                // получаем групп стоящих перед fromQueueClient
                List<ClientsGroup> clientsGroupAheadOfQueue = _clientQueue.Where(clientsGroup =>
                    _clientQueue.IndexOf(clientsGroup) < _clientQueue.IndexOf(fromQueueClient)).ToList();

                
                PutTheTable(table, fromQueueClient);
                
                if (clientsGroupAheadOfQueue.Count != 0)
                {
                    ClientStatusInTheQueue(clientsGroupAheadOfQueue);
                }

                // удаляем из очереди
                _clientQueue.RemoveAll(client => client.Id == fromQueueClient.Id);
            }
        }

        // return table where a given client group is seated, 
        // or null if it is still queuing or has already left
        public Table Lookup(ClientsGroup group)
        {
            return _tables.FirstOrDefault(table => table.Id == group.TableId);
        }
        
        public void PutTheTable(Table table, ClientsGroup group)
        {
            // устанавливаем id столика для клиента
            group.TableId = table.Id;

            // уменьшаем кол-во свободных мест за столиком
            table.FreeSize -= group.Size;

            // усаживаем за столик
            if (TableClients.ContainsKey(table.Id))
            {
                TableClients[table.Id].Add(group);
            }
            else
            {
                TableClients.Add(table.Id, new List<ClientsGroup> {group});
            }
        }

        public void ClientStatusInTheQueue(List<ClientsGroup> clientsGroupAheadOfQueue)
        {
            Random random = new Random();
            if (!clientsGroupAheadOfQueue.Any())
            {
                clientsGroupAheadOfQueue = new List<ClientsGroup>(_clientQueue);
            }
            foreach (var clientsGroup in clientsGroupAheadOfQueue)
            {
                if (random.Next(1, 11) <= 4)
                {
                    Console.WriteLine($"Client with Id = '{clientsGroup.Id}' leave queue!");
                    _clientQueue.Remove(clientsGroup);
                }
            }
        }
    }
}
