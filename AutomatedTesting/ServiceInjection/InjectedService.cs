using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedTesting.ServiceInjection
{
    internal record InjectedService(string Name) : IInjectableService
    {
    }
}
