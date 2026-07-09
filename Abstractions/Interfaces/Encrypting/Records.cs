namespace MQContract.Interfaces.Encrypting;

/// <summary>
/// Houses the returned results from a message encryption call
/// </summary>
/// <param name="Headers">Any additional headers to add to the message</param>
/// <param name="Data">The resulting encrypted data</param>
public readonly record struct EncryptionResult(Dictionary<string, string?>? Headers, byte[] Data);
