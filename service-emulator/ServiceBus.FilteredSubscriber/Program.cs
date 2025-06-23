using Azure.Messaging.ServiceBus;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceBus.FilteredSubscriber;

class Program
{
    // Connection string to the Service Bus namespace
    // Using the local Service Bus emulator connection string
    private const string connectionString = "Endpoint=sb://localhost:5671/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=guest";

    // Name of the topic
    private const string topicName = "create-claim-topic";

    // Name of the subscription with filter
    private const string subscriptionName = "subscription.2";

    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Service Bus Filtered Topic Subscriber...");
        Console.WriteLine($"This subscriber only receives messages that match the correlation filter with property 'prop3' = 'value3'");

        // Create a ServiceBusClient that will authenticate through the connection string
        await using var client = new ServiceBusClient(connectionString);

        // Create a processor for the subscription
        ServiceBusProcessor processor = client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false
        });

        // Configure the message and error handlers
        processor.ProcessMessageAsync += MessageHandler;
        processor.ProcessErrorAsync += ErrorHandler;

        // Start processing messages
        await processor.StartProcessingAsync();

        Console.WriteLine($"Listening for filtered messages on topic '{topicName}' with subscription '{subscriptionName}'. Press Ctrl+C to stop.");

        // Set up cancellation
        using var cancellationSource = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationSource.Cancel();
            Console.WriteLine("Stopping the subscriber...");
        };

        // Keep the application running
        await Task.Delay(Timeout.Infinite, cancellationSource.Token).ContinueWith(task => { });

        // Stop processing and clean up resources
        Console.WriteLine("Stopping and cleaning up resources...");
        await processor.StopProcessingAsync();
        await processor.DisposeAsync();
        await client.DisposeAsync();
    }

    // Message handler for processing messages
    private static async Task MessageHandler(ProcessMessageEventArgs args)
    {
        try
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Received filtered message: {body}");

            // Get any user properties
            Console.WriteLine("Message Properties:");
            foreach (var prop in args.Message.ApplicationProperties)
            {
                Console.WriteLine($"  {prop.Key}: {prop.Value}");
            }

            // Complete the message to remove it from the subscription
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
            // Abandon the message to make it available for processing again
            await args.AbandonMessageAsync(args.Message);
        }
    }

    // Error handler for processing errors
    private static Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"Error occurred: {args.Exception.Message}");
        Console.WriteLine($"Entity Path: {args.EntityPath}");
        Console.WriteLine($"Fully Qualified Namespace: {args.FullyQualifiedNamespace}");
        return Task.CompletedTask;
    }
}
