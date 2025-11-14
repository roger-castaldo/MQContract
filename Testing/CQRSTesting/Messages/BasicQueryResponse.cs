using MQContract.CQRS.Interfaces.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRSTesting.Messages
{
    public record BasicQueryResponse : IQueryResponse
    {
    }
}
