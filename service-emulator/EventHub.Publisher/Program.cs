using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System.Text.Json;

namespace EventHub.Publisher
{
    class Program
    {
        // Event Hub connection settings
        // These settings would connect to a local emulator
        private const string eventHubConnectionString = "Endpoint=sb://localhost:5673;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
        private const string eventHubName = "eh1";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Event Hub Publisher Starting...");

            // Create a producer client that you can use to send events to an event hub
            await using (var producerClient = new EventHubProducerClient(eventHubConnectionString, eventHubName))
            {
                // Create a batch of events
                using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

                // Generate some sample messages
                for (int i = 1; i <= 1; i++)
                {
                    // Create a message with some sample data
                    var messageData = new
                    {
                        CorrelationId = "a82ba590-f86d-45cf-9ce3-68c4c64b6e29",
                        QuestionSetId = "2e6bc115-9b8a-424d-9786-c08d5daf5b21",
                        CreatedDate = DateTimeOffset.UtcNow
                    };

                    // Convert the message to JSON
                    string messageJson = JsonSerializer.Serialize(messageData);
                    Console.WriteLine($"Preparing message: {messageJson}");

                    // Add the event to the batch
                    if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(messageJson))))
                    {
                        // If the batch is full, throw an exception
                        throw new Exception($"Event {i} is too large for the batch and cannot be sent.");
                    }
                }

                try
                {
                    // Send the batch of events to the event hub
                    await producerClient.SendAsync(eventBatch);
                    Console.WriteLine($"A batch of {5} messages has been published.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending the batch of events: {ex.Message}");
                    Console.WriteLine(ex.ToString());
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
