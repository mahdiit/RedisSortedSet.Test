// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RedisSortedSet.Test;
using StackExchange.Redis;

Console.WriteLine("Start");

var services = new ServiceCollection();
services.AddAutoMapper(x => x.AddMaps(typeof(Program).Assembly));
services.AddLogging(configure => configure.AddConsole());
services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(new ConfigurationOptions
    {
        EndPoints = { "localhost:6379" },
        ClientName = "squidward",
        AbortOnConnectFail = false,
    }));

services.AddKeyedScoped<IWeightedQueue, RedisWeightedQueue>("Redis");
services.AddKeyedScoped<IWeightedQueue, InMemoryWeightedQueue>("Memory");

var app = services.BuildServiceProvider();

var queue = app.GetRequiredKeyedService<IWeightedQueue>("Redis");
for (var i = 1; i <= 10; i++)
{
    await queue.EnqueueAsync(new WeightedQueueDto
    {
        Value = Guid.CreateVersion7().ToString(),
        Weight = DateTimeOffset.Now.AddSeconds(10).ToUnixTimeMilliseconds(),
    }, RedisWeightedQueueType.Mehdi_Data);

    await Task.Delay(TimeSpan.FromMilliseconds(10));
}

queue.GetEntries(RedisWeightedQueueType.Mehdi_Data);

Console.WriteLine("DequeueAsync");
Console.ReadKey();

var items = await queue.DequeueAsync(RedisWeightedQueueType.Mehdi_Data, 10);
byte counter = 1;
while (items.Length > 0)
{
    var currentWeight = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    Console.WriteLine($"Wait {counter} second....");
    counter++;

    await Task.Delay(1000);
    foreach (var item in items)
    {
        var diffWeight = item.Weight - currentWeight;
        if (diffWeight > 5000)
        {
            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}]\tIde time");
            await queue.EnqueueAsync(item, RedisWeightedQueueType.Mehdi_Data);
        }
        else
        {
            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}]\tItem done");
        }
    }

    var task1 = queue.DequeueAsync(RedisWeightedQueueType.Mehdi_Data, 2);
    var task2 = queue.DequeueAsync(RedisWeightedQueueType.Mehdi_Data, 2);
    var task3 = queue.DequeueAsync(RedisWeightedQueueType.Mehdi_Data, 2);
    var task4 = queue.DequeueAsync(RedisWeightedQueueType.Mehdi_Data, 2);
    var task5 = queue.DequeueAsync(RedisWeightedQueueType.Mehdi_Data, 2);

    var result = await Task.WhenAll([task1, task2, task3, task4, task5]);
    items = result.SelectMany(x => x).ToArray();
}

Console.WriteLine("Done");
Console.ReadKey();




