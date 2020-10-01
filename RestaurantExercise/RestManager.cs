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
        // критерий максимального ожидания
        private const int MAX_QUEUE_COUNT = 5;

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
            // если есть свободные столики определяем сразу туда, если нет пытаемся подсадить
            Table freeTable = _tables.FirstOrDefault(t => t.IsFree && t.Size >= group.Size) ?? _tables.FirstOrDefault(t => t.FreeSize >= group.Size);
            if (freeTable == null)
            {
                // столиков нет, значит добавляем в очередь
                _clientQueue.Add(group);
                return;
            }

            // устанавливаем id столика для клиента
            group.TableId = freeTable.Id;

            // уменьшаем кол-во свободных мест за столиком
            freeTable.FreeSize -= group.Size;

            // усаживаем за столик
            if (TableClients.ContainsKey(freeTable.Id))
            {
                TableClients[freeTable.Id].Add(group);
            }
            else
            {
                TableClients.Add(freeTable.Id, new List<ClientsGroup> {group});
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
                ClientsGroup fromQueueClient = _clientQueue.FirstOrDefault(client => _tables.Any(t => t.FreeSize >= client.Size));

                if (fromQueueClient == null)
                {
                    return;
                }

                // есть такой, пытаемся усаживать за столик
                Table freeTable = _tables.FirstOrDefault(t => t.IsFree && t.Size >= fromQueueClient.Size) ?? _tables.FirstOrDefault(t => t.FreeSize >= fromQueueClient.Size);
                if (freeTable != null)
                {
                    // предыдущим клиентам из очереди повышаем уровень ожидания, здесь вопрос - повышать всем впереди стоящим или только первому из очереди? сейчас повышаем всем впереди стоящим
                    foreach (ClientsGroup clientsGroup in _clientQueue.Where(clientsGroup => _clientQueue.IndexOf(clientsGroup) < _clientQueue.IndexOf(fromQueueClient)))
                    {
                        clientsGroup.WaitCount++;
                    }

                    // устанавливаем id столика для клиента
                    fromQueueClient.TableId = freeTable.Id;

                    // уменьшаем кол-во свободных мест за столиком
                    freeTable.FreeSize -= fromQueueClient.Size;

                    // усаживаем за столик
                    if (TableClients.ContainsKey(freeTable.Id))
                    {
                        TableClients[freeTable.Id].Add(fromQueueClient);
                    }
                    else
                    {
                        TableClients.Add(freeTable.Id, new List<ClientsGroup> {fromQueueClient});
                    }

                    // удаляем из очереди
                    _clientQueue.RemoveAll(client => client.Id == fromQueueClient.Id);

                    // сообщаем системе что скоро клиент устанет ждать и покинет нашу очередь, если до порога ожидания остлось 1
                    foreach (ClientsGroup clientsGroup in _clientQueue.Where(client => client.WaitCount == MAX_QUEUE_COUNT - 1))
                    {
                        Console.WriteLine($"Client with Id = '{clientsGroup.Id}' will soon leave queue!");
                    }

                    // сообщаем системе что клиент устал ждать и покидает нашу очередь
                    foreach (ClientsGroup clientsGroup in _clientQueue.Where(client => client.WaitCount == MAX_QUEUE_COUNT))
                    {
                        Console.WriteLine($"Client with Id = '{clientsGroup.Id}' leave queue!");
                    }

                    // удаляем уставших ждать
                    _clientQueue.RemoveAll(client => client.WaitCount == MAX_QUEUE_COUNT);
                }
            }
        }

        // return table where a given client group is seated, 
        // or null if it is still queuing or has already left
        public Table Lookup(ClientsGroup group)
        {
            return _tables.FirstOrDefault(table => table.Id == group.TableId);
        }
    }
}
