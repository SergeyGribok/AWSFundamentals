using MediatR;

namespace Customers.Consumer.Messages;

public class CustomerDeleted : IRequest, ISqsMessage
{
    public required Guid Id { get; set; }
}
