**Source Generator**

MQContract includes an Incremental Source Generator that produces a strongly-typed Message Context at compile time. The generated context improves runtime performance, helps with AOT scenarios, and centralizes message-specific encoders, converters, and encryptors similar to System.Text.Json source-generated contexts.

## How it works

- Define a partial class that inherits `MQContractMessageContext`.
- Decorate that class with one or more `UseMqContractAttribute` attributes to register the message (contract) types you want included.
- Optionally decorate the context class with `MQContractMessageContextAttribute` to enable automatic scanning for encoders, converters, and encryptors when those are not explicitly supplied.

The generator will implement the virtual helpers on `MQContractMessageContext` (try-get encoder, decoding callback, type definitions, converters, encryptors, and query execution helpers) for the types you register.

## Minimal example

```csharp
using MQContract;
using MQContract.Attributes;

[UseMqContractAttribute(typeof(Announcement))]
[UseMqContractAttribute(typeof(MyQuery), encoderType: typeof(MyCustomEncoder))]
[MQContractMessageContextAttribute(locateEncoders: true, locateConverters: true, locateEncryptors: false)]
public partial class MyMessageContext : MQContractMessageContext { }
```

Notes:
- `UseMqContractAttribute` registers a message type to be included in the generated context.
- The `MQContractMessageContextAttribute` constructor parameters (`locateEncoders`, `locateConverters`, `locateEncryptors`) enable automatic scanning when per-message settings are not provided.

## `UseMqContractAttribute` options

Each `UseMqContractAttribute` can include optional parameters to control how the generator wires up support for the message:
- `contractType` (required) — the message/contract type to include.
- `encoderType` (optional) — a type that implements `IMessageTypeEncoder<T>` for this contract.
- `converters` (optional) — an array of converter types that can be used to convert other message types into this one.
- `messageEncryptor` (optional) — a type that implements the required encryptor interface for this contract.

If `encoderType`, `converters`, or `messageEncryptor` are not set and the context has auto-locate enabled, the generator will scan referenced assemblies for a matching implementation.

## `MQContractMessageContextAttribute` options

- `LocateEncoders` — if `true`, the generator will attempt to locate a single matching encoder when a `UseMqContractAttribute` does not specify one. If multiple encoders exist for the same contract type, none will be selected and a compiler warning is emitted.
- `LocateConverters` — if `true`, the generator will discover conversion paths to the registered contract type and include converters where possible.
- `LocateEncryptors` — if `true`, the generator will attempt to find a single matching encryptor; if multiple are found a warning is emitted and none will be used.

## Generated APIs and usage

The source generator implements the virtual methods on `MQContractMessageContext` so you can call them at runtime. Examples:

- `TryGetMessageEncoder<TMessage>(...)` — returns the encoder instance (or null) for the given message type.
- `TryGetDecodingCallback(messageID, ...)` — returns a decode callback for incoming messages.
- `TryGetMessageType(Type)` — obtains the message type definition (channel, type name, version, response settings).
- `TryGetMessageConverter<TMessage>(messageID, ...)` — obtains a converter callback for converting an incoming message to `TMessage`.

The generated context is registered with the connection via `RegisterMessageContext(...)` so connection-level operations (publish, query, subscribe) can use generated encoders/converters/encryptors automatically.

## Best practices and tips

- Prefer explicit `encoderType` or `messageEncryptor` when you have a clear implementation to avoid ambiguous auto-discovery.
- Keep the context class internal or appropriately scoped to match your application's visibility requirements; the generator respects declared accessibility when emitting the partial implementation.
- Use the generated `TryExecuteQuery` helpers to get better performance for dynamic query dispatch scenarios.

## Warnings & troubleshooting

- If auto-locate finds multiple encoders or encryptors for a contract, the generator will skip selecting one and emit a compilation warning. In that case explicitly specify which implementation to use in the `UseMqContractAttribute`.
- If a converter path cannot be found, you will not get a converter callback — ensure your converters implement expected interfaces and are discoverable by the generator.