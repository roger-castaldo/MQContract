﻿using MQContract.Interfaces.Encrypting;
using MQContract.Messages;

namespace MQContract.Defaults
{
    internal class NonEncryptor<T> : IMessageTypeEncryptor<T>
    {
        public Stream Decrypt(Stream stream, MessageHeader headers) => stream;

        public byte[] Encrypt(byte[] data, out Dictionary<string, string?> headers)
        {
            headers = [];
            return data;
        }
    }
}
