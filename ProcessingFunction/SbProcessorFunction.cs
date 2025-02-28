using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ProcessingFunction;

public class SbProcessorFunction(ILogger<SbProcessorFunction> logger)
{
    [Function(nameof(SbProcessorFunction))]
    public async Task Run(
        [ServiceBusTrigger("user-topic", "sub-processing-func", Connection = "sb")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        logger.LogInformation("Message ID: {Id}", message.MessageId);
        logger.LogInformation("Message Body: {Body}", message.Body);

        await messageActions.CompleteMessageAsync(message);
    }
}
