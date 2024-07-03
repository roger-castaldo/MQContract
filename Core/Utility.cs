namespace MQContract
{
    internal static class Utility
    {
        internal static string TypeName<T>()
            => TypeName(typeof(T));

        internal static string TypeName(Type type)
        {
            var result = type.Name;
            if (result.Contains('`'))
                result=result[..result.IndexOf('`')];
            return result;
        }
    }
}
