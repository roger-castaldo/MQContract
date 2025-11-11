namespace AutomatedTesting.ServiceInjection
{
    internal record InjectedService(string Name) : IInjectableService
    {
    }
}
