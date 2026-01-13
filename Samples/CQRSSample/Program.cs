using CQRSSample;
using Messages;
using MQContract;
using MQContract.CQRS;
using MQContract.CQRS.Extensions;
using MQContract.InMemory;

var serviceConnection = new Connection();

var contractConnection = ContractConnection.Instance(serviceConnection);

var cqrsConnection = contractConnection.CreateCQRSConnection("CQRSChannel");

await cqrsConnection.RegisterCommandProcessorAsync(new CreateUserProcessor(), "CommandGroup");

await cqrsConnection.RegisterQueryProcessorAsync(new GetUserProcessor(), "QueryGroup");

var context = new MQContract.CQRS.Context();

Console.WriteLine("Executing CreateUserCommand...");
await cqrsConnection.ExecuteCommandAsync(new CreateUserCommand("user123", "John Doe", "john@example.com"), context);

Console.WriteLine("Executing GetUserQuery...");
try
{
    var user = await cqrsConnection.ExecuteQueryAsync<GetUserQuery, User>(new GetUserQuery("user123"), context, TimeSpan.FromSeconds(10));
    Console.WriteLine($"Retrieved user: {user.UserName}, {user.Email}");
}catch(QueryCallException e)
{ 
    Console.WriteLine($"Query error: {e.Error.Message}"); 
}

Console.WriteLine("CQRS sample completed.");