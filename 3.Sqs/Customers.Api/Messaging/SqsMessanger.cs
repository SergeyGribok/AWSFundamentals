using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Customers.Api.Messaging;

public class SqsMessanger : ISqsMessanger
{
    private readonly IAmazonSQS _amazonSQS;
    private readonly IOptions<QueueSettings> _queueSettings;
    private string? _queueUrl;


    public SqsMessanger(IAmazonSQS amazonSQS, IOptions<QueueSettings> queueSettings)
    {
        _amazonSQS = amazonSQS;
        _queueSettings = queueSettings;
    }

    public async Task<SendMessageResponse> SendMessageAsync<T>(T message)
    {
        var queueUrl = await GetQueueUrlAsync();

        var sendMessageRequest = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = JsonSerializer.Serialize(message),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "MessageType", new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = typeof(T).Name
                    }
                }
            }
        };

        return await _amazonSQS.SendMessageAsync(sendMessageRequest);
    }

    private async Task<string> GetQueueUrlAsync()
    {
        if(_queueUrl is not null)
        {
            return _queueUrl;
        }

        var queueUrlResponse = await _amazonSQS.GetQueueUrlAsync(_queueSettings.Value.Name);
        _queueUrl = queueUrlResponse.QueueUrl;
        return _queueUrl;
    }
}
