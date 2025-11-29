using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Interfaces.Encrypting
{
    public readonly record struct EncryptionResult(Dictionary<string,string?>? Headers, byte[] Data);
}
