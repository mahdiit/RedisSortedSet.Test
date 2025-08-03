using System.Collections.Concurrent;

namespace RedisSortedSet.Test;

public class InMemoryWeightedQueue : IWeightedQueue
{
    private readonly ConcurrentDictionary<RedisWeightedQueueType, List<WeightedQueueDto>> _queues = new();
    private readonly Lock _lock = new();

    public Task EnqueueAsync(WeightedQueueDto data, RedisWeightedQueueType queue)
    {
        lock (_lock)
        {
            if (!_queues.ContainsKey(queue))
                _queues[queue] = new List<WeightedQueueDto>();

            // Insert item while keeping list sorted by weight (lowest first)
            var list = _queues[queue];
            int index = list.BinarySearch(data, new WeightComparer());
            if (index < 0) index = ~index;
            list.Insert(index, data);
        }

        return Task.CompletedTask;
    }

    public Task<WeightedQueueDto[]> DequeueAsync(RedisWeightedQueueType queue, int count = 1)
    {
        List<WeightedQueueDto> result = new();

        lock (_lock)
        {
            if (_queues.TryGetValue(queue, out var list))
            {
                int take = Math.Min(count, list.Count);
                result = list.Take(take).ToList();
                list.RemoveRange(0, take);
            }
        }

        return Task.FromResult(result.ToArray());
    }

    public void GetEntries(RedisWeightedQueueType queue)
    {
        throw new NotImplementedException();
    }

    private class WeightComparer : IComparer<WeightedQueueDto>
    {
        public int Compare(WeightedQueueDto? x, WeightedQueueDto? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;
            return x.Weight.CompareTo(y.Weight);
        }
    }
}