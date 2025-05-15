namespace MQContract.Kafka
{
    internal class PersistenceFailedException : Exception
    {
        internal PersistenceFailedException()
            : base("Persistence Failed") { }
    }
    internal class QueryAsyncReponseException : Exception
    {
        internal QueryAsyncReponseException(string error)
            : base(error) { }
    }

    internal class QueryLockFailedException : Exception
    {
        internal QueryLockFailedException()
            : base("Failed to produce query lock") { }
    }

    internal class QueryResultMissingException : Exception
    {
        internal QueryResultMissingException()
            : base("Query result not found") { }
    }

    internal class QueryExecutionFailedException : Exception
    {
        internal QueryExecutionFailedException()
            : base("Failed to execute query") { }
    }
}
