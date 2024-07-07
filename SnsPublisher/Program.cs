using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using SnsPublisher;
using System.Text.Json;

var customer = new CustomerCreated
{
    Id = Guid.NewGuid(),
    Email = "nick@nickchapsas.com",
    FullName = "Nick Chapsas",
    DateOfBirth = new DateTime(1993, 1, 1),
    GitHubUsername = "nickchapsas"
};

var snsClient = new AmazonSimpleNotificationServiceClient();

var topicARNResponse = await snsClient.FindTopicAsync("customers");

var publishRequest = new PublishRequest
{
    TopicArn = topicARNResponse.TopicArn,
    Message = JsonSerializer.Serialize(customer),
    MessageAttributes = new Dictionary<string, MessageAttributeValue>
    {
        {
            "MessageType", new MessageAttributeValue
            {
                DataType = "String",
                StringValue = nameof(CustomerCreated)
            }
        }
    },
};

var response = await snsClient.PublishAsync(publishRequest);