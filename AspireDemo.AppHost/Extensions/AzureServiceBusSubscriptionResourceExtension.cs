using Aspire.Hosting.Azure;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AspireDemo.AppHost.Extensions;

public static class AzureServiceBusSubscriptionResourceExtension
{
    public static IResourceBuilder<AzureServiceBusSubscriptionResource> WithPeekMessagesCommand(
       this IResourceBuilder<AzureServiceBusSubscriptionResource> builder)
    {
        builder.WithCommand(
            name: "peek-messages",
            displayName: "Peek Messages",
            executeCommand: context => OnPeekMessagesCommandAsync(builder, context),
            updateState: OnUpdateResourceState,
            iconName: "AnimalRabbitOff",
            iconVariant: IconVariant.Filled);

        return builder;
    }

    private static async Task<ExecuteCommandResult> OnPeekMessagesCommandAsync(
        IResourceBuilder<AzureServiceBusSubscriptionResource> builder,
        ExecuteCommandContext context)
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();

        var connectionString = await builder.Resource.Parent.Parent.ConnectionStringExpression.GetValueAsync(CancellationToken.None);
        var sbClient = new ServiceBusClient(connectionString);
        var receiver = sbClient.CreateReceiver(builder.Resource.Parent.TopicName, builder.Resource.SubscriptionName);

        var messages = await receiver.PeekMessagesAsync(100);

        logger.LogInformation("###### Peek Message Command ######");
        logger.LogInformation("Peeked {MessageCount} messages from {TopicName}/{SubscriptionName}:",
            messages.Count, builder.Resource.Parent.TopicName, builder.Resource.SubscriptionName);

        foreach (var message in messages)
        {
            logger.LogInformation("Message ID: {MessageId}", message.MessageId);
            logger.LogInformation("Message Body: {Body}", Encoding.UTF8.GetString(message.Body));
        }

        return CommandResults.Success();
    }

    private static ResourceCommandState OnUpdateResourceState(
        UpdateCommandStateContext context)
    {
        return context.ResourceSnapshot.HealthStatus is HealthStatus.Healthy
            ? ResourceCommandState.Enabled
            : ResourceCommandState.Disabled;
    }
}
