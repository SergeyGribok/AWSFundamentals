
using Amazon.SimpleNotificationService.Model;

namespace Customers.Api.Messaging;

public interface ISnsMessanger
{
    Task<PublishResponse> PublishMessageAsync<T>(T message);
}
