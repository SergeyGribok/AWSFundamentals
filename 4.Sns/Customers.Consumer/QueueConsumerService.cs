
using Amazon.SQS;
using Amazon.SQS.Model;
using Customers.Consumer.Messages;
using MediatR;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Customers.Consumer;

public class QueueConsumerService : BackgroundService
{
    private readonly IAmazonSQS _amazonSQS;
    private readonly IOptions<QueueSettings> _queueSettings;
    private readonly IMediator _mediator;
    private readonly ILogger<QueueConsumerService> _logger;

    public QueueConsumerService(
        IAmazonSQS amazonSQS,
        IOptions<QueueSettings> queueSettings,
        IMediator mediator,
        ILogger<QueueConsumerService> logger)
    {
        _amazonSQS = amazonSQS;
        _queueSettings = queueSettings;
        _mediator = mediator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueUrlResponse = await _amazonSQS.GetQueueUrlAsync(_queueSettings.Value.Name, stoppingToken);

        var receiveMessageRequest = new ReceiveMessageRequest
        {
            QueueUrl = queueUrlResponse.QueueUrl,
            MessageSystemAttributeNames = new List<string> { "All" },
            MessageAttributeNames = new List<string> { "All" }
        };

        while (!stoppingToken.IsCancellationRequested) 
        {
            var response = await _amazonSQS.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

            foreach(var message in response.Messages)
            {
                var messageType = message.MessageAttributes["MessageType"].StringValue;

                var type = Type.GetType($"Customers.Consumer.Messages.{messageType}");

                if(type is null)
                {
                    _logger.LogWarning("Unknown message type: {MessageType}", messageType);
                    continue;
                }

                var typedMessage = (ISqsMessage)JsonSerializer.Deserialize(message.Body, type)!;

                try
                {
                    await _mediator.Send(typedMessage, stoppingToken);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Message failed duting processing");
                    continue;
                }

                await _amazonSQS.DeleteMessageAsync(
                    queueUrlResponse.QueueUrl, 
                    message.ReceiptHandle, 
                    stoppingToken);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
