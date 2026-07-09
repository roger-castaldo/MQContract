using Messages;
using MQContract.CQRS.Interfaces.Query;

namespace CQRSSample;

public class GetUserProcessor : IQueryProcessor<GetUserQuery, User>
{
    private static readonly Dictionary<string, User> _users = new();

    public async ValueTask<User> ProcessQueryAsync(IQueryInvocationContext<GetUserQuery> context, CancellationToken cancellationToken)
    {
        var query = context.Query;
        if (_users.TryGetValue(query.UserId, out var user))
        {
            Console.WriteLine($"User found: {user.UserName}");
            return user;
        }
        throw new KeyNotFoundException($"User {query.UserId} not found");
    }

    public void ErrorRecieved(Exception error)
    {
        Console.WriteLine($"GetUserProcessor error: {error.Message}");
    }
}