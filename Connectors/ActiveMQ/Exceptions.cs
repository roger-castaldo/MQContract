namespace MQContract.ActiveMQ
{
    internal class QueryAsyncReponseException : Exception
    {
        internal QueryAsyncReponseException(string error)
            : base(error) { }
    }
    internal class QueryExecutionFailedException : Exception
    {
        internal QueryExecutionFailedException()
            : base("Failed to execute query") { }
    }
    internal class QueryResultMissingException : Exception
    {
        internal QueryResultMissingException()
            : base("Query result not found") { }
    }
}
