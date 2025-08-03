#nullable enable
namespace RedisSortedSet.Test
{
    public interface IWeightedQueue
    {
        Task EnqueueAsync(WeightedQueueDto data, RedisWeightedQueueType queue);

        Task<WeightedQueueDto[]> DequeueAsync(RedisWeightedQueueType queue, int count = 1);
        void GetEntries(RedisWeightedQueueType queue);
    }

    public record WeightedQueueDto
    {
        public string Value { get; init; }
        public double Weight { get; init; }
    }

    public enum RedisWeightedQueueType : byte
    {
        Order_ProcessWms_Confirmed = 1,
        Mehdi_Data = 2
    }
}
