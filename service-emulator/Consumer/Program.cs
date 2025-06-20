using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    private const string connectionString = "Endpoint=sb://localhost:5673;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
    private const string eventHubName = "eh1";
    private const string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

    static async Task Main(string[] args)
    {
        await ReceiveEventsAsync();
    }

    static async Task ReceiveEventsAsync()
    {
        await using var consumer = new EventHubConsumerClient(consumerGroup, connectionString, eventHubName);

        Console.WriteLine("Listening for events. Press Ctrl+C to stop.");

        using var cancellationSource = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationSource.Cancel();
            Console.WriteLine("Exiting...");
        };

        await foreach (PartitionEvent partitionEvent in consumer.ReadEventsAsync(cancellationSource.Token))
        {
            string data = partitionEvent.Data?.EventBody.ToString();
            Console.WriteLine($"Received event from partition {partitionEvent.Partition.PartitionId}: {data}");
        }
    }
}
