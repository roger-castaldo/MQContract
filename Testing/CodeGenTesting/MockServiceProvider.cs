using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenTesting
{
    internal static class MockServiceProvider
    {
        public static Mock<IServiceProvider> Instance
        {
            get
            {
                var result = new Mock<IServiceProvider>();
                var mockInjection = new Mock<IServiceInjection>();
                result.Setup(x=>x.GetService(typeof(IServiceInjection)))
                    .Returns(mockInjection.Object);
                return result;
            }
        }
    }
}
