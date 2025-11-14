using MQContract.CQRS.Interfaces.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CQRSTesting.Messages
{
    public record BasicResponseCommand : ICommand<BasicCommandResponse>
    {
    }
}
