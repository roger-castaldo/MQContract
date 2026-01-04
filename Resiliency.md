# Resiliency

MQContract includes a built-in resiliency layer that allows message transmissions to gracefully handle transient failures such as temporary connectivity issues, service throttling, or intermittent downstream outages. Resiliency is applied at the **contract connection level** and is fully configurable using policies backed by **Polly**.

Resiliency in MQContract is **opt-in** and is enabled by registering one or more resilience policies using `RegisterResiliencePolicy`.

---

## Enabling Resiliency

To enable resiliency for a connection, call one of the `RegisterResiliencePolicy` overloads and provide the desired configuration. Policies can be registered at varying levels of specificity, allowing fine-grained control over how retries, circuit breakers, and other resilience behaviors are applied.

Policies may be registered for:

- A specific **message channel**
- A specific **message type**
- A **default** (fallback) policy
- An optional **service connection name** when using mapped or multi-connection configurations

---

## Policy Resolution and Priority

When a transmission occurs, MQContract determines which resiliency policy to apply by evaluating the available policies in a defined priority order.

### Resolution Order

Policies are selected using the following precedence:

1. Channel-specific policy  
2. Message-type-specific policy  
3. Default policy  

When using **mapped or multi-service connections**, this resolution is performed in two passes:

1. Policies registered for the **specific service connection name**
2. Policies registered without a connection name (global fallback)

This enables:

- Strict policies for critical channels or message types
- Relaxed policies for less critical traffic
- Per-service overrides when multiple downstream connections are in use

---

## What Resiliency Applies To

Resiliency is applied **only when a transmission returns an error that is considered recoverable**.

### Recoverable Errors

Recoverable errors typically include:

- Temporary connectivity failures
- Timeouts
- Service unavailability
- Transient infrastructure issues

These errors may trigger:

- Retries
- Circuit breaking
- Other Polly-based resilience behaviors, depending on the configured policy

### Fatal Errors

Errors explicitly marked as **Fatal** are **not retried** and do **not** participate in circuit-breaking logic.

Fatal errors generally represent:

- Invalid parameters
- Contract or encoding violations
- Critical library or configuration errors
- Conditions where retrying would never succeed

Once a fatal error is encountered, it is immediately returned to the caller.

---

## How Resiliency Operates Internally

1. A transmission is attempted against the underlying service connection.
2. If the transmission succeeds, the result is returned immediately.
3. If the transmission fails:
   - MQContract evaluates whether the error is fatal.
   - Fatal errors are returned immediately.
   - Non-fatal errors are processed using the resolved Polly policy.
4. The configured policy governs retries, delays, circuit breaking, and failure escalation.

This behavior is consistent across all supported transports, ensuring predictable resiliency regardless of the underlying messaging system.

---

## Underlying Technology

MQContract’s resiliency layer is implemented using **Polly**, a well-established .NET resilience and transient-fault-handling library. This provides:

- Retry policies
- Circuit breakers
- Timeouts
- Policy composition
- Deterministic execution behavior

By leveraging Polly, MQContract provides robust resiliency primitives while maintaining a clean, contract-aware abstraction.
