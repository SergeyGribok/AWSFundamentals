using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Customers.Api.Messaging;

public class SnsMessanger : ISnsMessanger
{
    private readonly IAmazonSimpleNotificationService _sns;
    private readonly IOptions<TopicSettings> _topicSettings;
    private string? _topicArn;


    public SnsMessanger(
        IAmazonSimpleNotificationService sns,
        IOptions<TopicSettings> topicSettings)
    {
        _sns = sns;
        _topicSettings = topicSettings;
    }

    public async Task<PublishResponse> PublishMessageAsync<T>(T message)
    {
        var topicArn = await GetTopicArnAsync();

        var publishRequest = new PublishRequest
        {
            TopicArn = topicArn,
            Message = JsonSerializer.Serialize(message),
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

        return await _sns.PublishAsync(publishRequest);
    }

    private async ValueTask<string> GetTopicArnAsync()
    {
        if(_topicArn is not null)
        {
            return _topicArn;
        }

        var topicARNResponse = await _sns.FindTopicAsync(_topicSettings.Value.Name);
        _topicArn = topicARNResponse.TopicArn;
        return _topicArn;
    }
}
