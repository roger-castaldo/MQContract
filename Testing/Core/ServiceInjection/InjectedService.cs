namespace CoreTesting.ServiceInjection;

internal record InjectedService(string Name) : IInjectableService
{
}
