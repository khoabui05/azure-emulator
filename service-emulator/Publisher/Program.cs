// See https://aka.ms/new-console-template for more information
using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Publisher
{
    class Program
    {
        // Connection string to the Service Bus namespace
        // Using the local Service Bus emulator connection string
        private static string connectionString = "Endpoint=sb://localhost:5671/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=guest";

        // Name of the queue or topic
        private static string queueName = "messages";

        // Number of messages to be sent
        private static int numOfMessages = 10;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Service Bus Publisher...");

            // Create a ServiceBusClient that will authenticate through the connection string
            await using var client = new ServiceBusClient(connectionString);

            // Create a sender for the queue
            ServiceBusSender sender = client.CreateSender(queueName);

            // Create a batch of messages
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            for (int i = 1; i <= numOfMessages; i++)
            {
                // Create a message to send
                string messageBody = $"Message {i} from Publisher at {DateTime.Now}";

                // Try to add the message to the batch
                if (!messageBatch.TryAddMessage(new ServiceBusMessage(messageBody)))
                {
                    // If the batch is full, send it and create a new one
                    throw new Exception($"Message {i} is too large to fit in the batch.");
                }

                Console.WriteLine($"Added message: {messageBody}");
            }

            try
            {
                // Send the batch of messages to the queue
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"A batch of {numOfMessages} messages has been sent to the queue.");
            }
            finally
            {
                // Close the sender
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
