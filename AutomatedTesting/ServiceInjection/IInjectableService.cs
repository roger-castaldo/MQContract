using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedTesting.ServiceInjection
{
    internal interface IInjectableService
    {
        string Name { get; }
    }
}
