#nullable enable
using AutoMapper;
using StackExchange.Redis;

namespace RedisSortedSet.Test
{
    public class RedisWeightedQueue(IConnectionMultiplexer connectionMultiplexer, IMapper mapper) : IWeightedQueue
    {
        private const string QueueKey = "squidward:weightedqueue";
        private readonly IDatabase _db = connectionMultiplexer.GetDatabase();

        private static string GetQueueName(RedisWeightedQueueType queue)
        {
            return $"{QueueKey}:{queue.ToString().ToLower().Replace("_", ":")}";
        }

        public async Task EnqueueAsync(WeightedQueueDto data, RedisWeightedQueueType queue)
        {
            await _db.SortedSetAddAsync(GetQueueName(queue), (RedisValue)data.Value, data.Weight);
        }

        public async Task<WeightedQueueDto[]> DequeueAsync(RedisWeightedQueueType queue, int count = 1)
        {
            var result = await _db.SortedSetPopAsync(GetQueueName(queue), count);
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.ffff}\tDequeueAsync\t{result.Length}");

            return result.Length == 0 ? [] : mapper.Map<WeightedQueueDto[]>(result);
        }

        public void GetEntries(RedisWeightedQueueType queue)
        {
            var keys = _db.SortedSetScan(GetQueueName(queue));
            foreach (var key in keys)
            {
                Console.WriteLine($"{key.Element}\t{key.Score}");
            }
        }
    }
}
