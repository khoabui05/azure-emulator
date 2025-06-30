using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;

namespace ServiceBus.TopicPublisher;

class Program
{
    // Connection string to the Service Bus namespace
    // Using the local Service Bus emulator connection string
    private const string connectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

    // Name of the topic
    private const string topicName = "create-claim-topic";

    // Number of messages to be sent
    private const int numOfMessages = 5;

    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Service Bus Topic Publisher...");

        // Create a ServiceBusClient that will authenticate through the connection string
        await using var client = new ServiceBusClient(connectionString);

        // Create a sender for the topic
        ServiceBusSender sender = client.CreateSender(topicName);

        try
        {
            // Send messages one by one with different properties
            for (int i = 1; i <= numOfMessages; i++)
            {
                // Create a message to send
                string messageBody = $"Topic message {i} from Publisher at {DateTime.Now}";
                var message = new ServiceBusMessage(messageBody);

                // Add custom properties based on the message number
                // These properties can be used for filtering in subscriptions
                message.ApplicationProperties.Add("MessageId", i);
                message.ApplicationProperties.Add("MessageType", i % 2 == 0 ? "Even" : "Odd");

                // Every third message gets a special property
                if (i % 3 == 0)
                {
                    message.ApplicationProperties.Add("prop3", "value3");
                }

                // Send the message
                await sender.SendMessageAsync(message);
                Console.WriteLine($"Sent message: {messageBody}");
            }

            Console.WriteLine($"{numOfMessages} messages have been sent to the topic.");
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
