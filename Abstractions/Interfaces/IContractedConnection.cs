using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Interfaces
{
    public interface IContractedConnection : IContractConnection, IMetricContractConnection<IContractedConnection>
    {
    }
}
