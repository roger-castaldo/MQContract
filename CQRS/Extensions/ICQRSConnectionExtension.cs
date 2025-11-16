using MQContract.CQRS.Interfaces;
using MQContract.CQRS.Interfaces.Command;
using MQContract.CQRS.Interfaces.Query;

namespace MQContract.CQRS.Extensions
{
    public static class ICQRSConnectionExtension
    {
        public static async ValueTask<ICQRSConnection> RegisterCommandProcessorAsync<TCommand>(this ValueTask<ICQRSConnection> cqrsConnectionTask, ICommandProcessor<TCommand> processor, string? group = null)
            where TCommand : ICommand
        {
            var result = await cqrsConnectionTask.ConfigureAwait(false);
            return await result.RegisterCommandProcessorAsync<TCommand>(processor, group)
                .ConfigureAwait(false);
        }

        public static async ValueTask<ICQRSConnection> RegisterCommandProcessorAsync<TCommand, TCommandResult>(this ValueTask<ICQRSConnection> cqrsConnectionTask, ICommandProcessor<TCommand, TCommandResult> processor, string? group = null)
            where TCommand : ICommand<TCommandResult>
        {
            var result = await cqrsConnectionTask.ConfigureAwait(false);
            return await result.RegisterCommandProcessorAsync<TCommand, TCommandResult>(processor, group)
                .ConfigureAwait(false);
        }

        public static async ValueTask<ICQRSConnection> RegisterQueryProcessorAsync<TQuery, TQueryResponse>(this ValueTask<ICQRSConnection> cqrsConnectionTask, IQueryProcessor<TQuery, TQueryResponse> processor, string? group = null)
            where TQuery : IQuery
        {
            var result = await cqrsConnectionTask.ConfigureAwait(false);
            return await result.RegisterQueryProcessorAsync<TQuery, TQueryResponse>(processor, group)
                .ConfigureAwait(false);
        }
    }
}
