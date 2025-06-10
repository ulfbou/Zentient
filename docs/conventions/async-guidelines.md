Certainly. Below is a highly detailed, formal guideline document for async vs sync usage across the Zentient Framework. It is structured to fit within a /docs/conventions/ folder and provides justification, patterns, exceptions, and code-level guidance.


---

# Zentient Framework Async vs Sync Conventions

📁 Location: `/docs/conventions/async-guidelines.md`  
📅 Last Updated: 2025-06-09  
📄 Status: ✅ Active Guideline  

## 🎯 Purpose

This document establishes a unified convention across all **Zentient Framework** libraries regarding asynchronous vs synchronous API design. It ensures:
- Performance and scalability under I/O-bound workloads.
- Consistency across protocols and transport layers.
- Minimized blocking and thread starvation.
- Clear developer expectations for consuming and contributing to Zentient libraries.

---

## 🔰 Async-First Philosophy

### Principle
> **Async is the default.** All framework components are designed to operate in asynchronous, non-blocking execution flows using `Task<T>` or `ValueTask<T>`.

### Justification
- .NET's async model is essential for scalable server applications.
- HTTP, gRPC, and messaging transports are async by nature.
- Blocking APIs (`.Result`, `.Wait()`) introduce deadlocks and thread exhaustion.
- Async aligns with OpenTelemetry (`Activity`, `Meter`, `LogRecord`) which expect context-aware, non-blocking pipelines.

---

## ✅ Core Guidelines

### ✅ Always Use Async for:
| Use Case | Reason |
|----------|--------|
| Endpoint handlers | Network I/O, request latency |
| Transport adapters | Awaiting I/O operations |
| Observability enrichers | Tracing/logging systems are async |
| Result transformers and enrichers | May call async services |
| Policy evaluators | Rate limiters, cache probes |
| Configuration loaders | May read from remote secrets or config |
| Validation / Contract checks | Often involve I/O or background tasks |

Use `async Task<T>` or `async ValueTask<T>` consistently.

---

## ⚠️ Synchronous Use: Narrow Scope Only

### 🧩 When Sync is Acceptable
| Area | Justification |
|------|---------------|
| **Roslyn Analyzers** | Roslyn APIs are synchronous |
| **Pure value transformations** | If fully in-memory and CPU-bound |
| **Unit tests / test doubles** | For simplicity and control |
| **CLI tooling in DevKit** | Scripting efficiency and determinism |
| **Internal sync helpers** | For internal composition or error conversion only |

All sync methods must be **opt-in** and **clearly documented**.

---

## 🛑 Anti-Patterns

### ❌ Disallowed:
- Blocking async code in sync context:
  ```csharp
  var result = GetResultAsync().Result; // ❌

Defining sync overloads "just in case":

public Result<T> Process() { ... } // ❌ if an async variant exists

Mixing sync/async logic in shared infrastructure

Using Task.Run(...) to wrap sync methods for faux async



---

🧱 API Design Rules

Async Method Naming

Never append Async to methods in async-only contexts.

Use Async suffix only when both async/sync exist:

// Valid:
public Task<Result<T>> BindAsync(...) { ... }

// Invalid:
public Task<Result<T>> Bind(...) { ... } // if async-only


Cancellation Token Convention

Always support CancellationToken if:

The operation takes user input

It performs I/O

It may be long-running



public Task<IEndpointResult> ExecuteAsync(RequestContext context, CancellationToken cancellationToken);


---

🛠 Library-Specific Directives

Zentient.Results

Bind, Map, Match must have BindAsync, MapAsync, MatchAsync variants.

Prefer ValueTask<Result<T>> for transformers and policies.

Optional sync helpers: .ToSyncResult() for test or migration code.


Zentient.Endpoints

All pipeline stages (IResultTransformer, IResultObserver, IEndpointHandler) are async.

Sync handlers must be wrapped using adapters (not direct overloads).


Zentient.Pipeline

Middleware hooks are async-first.

Sync enrichers only allowed for pure CPU transformations.


Zentient.Telemetry

Always async. OTel APIs propagate async context (e.g., Activity.Current).

All hooks must support CancellationToken.


Zentient.Analyzers

Entirely synchronous, per Roslyn API requirements.

Never await in diagnostic analyzers or code fix providers.


Zentient.DevKit

Mix allowed:

CLI and schema loaders may be sync

Contract generation for live apps is async


Flag sync CLI commands for review.


Zentient.Observability.Extensions

Must be async to support OTLP, Sentry, Raygun, Datadog exporters.

No sync fallbacks for log shipping or telemetry dispatch.



---

🧪 Testing Conventions

Use async Task unit tests as standard.

Prefer ValueTask in library internals but convert to Task in public APIs.

Avoid Task.Run(...) in tests unless isolating thread-bound behavior.



---

🧰 Utilities & Helpers

Extension Pattern

public static class ResultExtensions
{
    public static Result<T> ToSync<T>(this Task<Result<T>> task) =>
        task.ConfigureAwait(false).GetAwaiter().GetResult(); // internal use only
}

[EditorBrowsable(Never)] for sync overloads:

[EditorBrowsable(EditorBrowsableState.Never)]
public Result<T> ExecuteSync(...) { ... }


---

📌 Future Proposals

Idea	Status

Linter to flag sync usage in I/O-heavy code	✅ Tracked
Async analyzer for .Result, .Wait()	✅ In Zentient.Analyzers v0.1
Diagnostic for missing CancellationToken in async methods	🚧 Planned
Optional compile-time switch to disable sync API surface	🔍 Under review



---

✅ Summary

Rule	Status

* Async is the default across all Zentient libraries	✅ Enforced
* Sync only allowed where explicitly justified	✅ Allowed (scoped)
* No blocking on Task or ValueTask allowed	✅ Mandatory
* CancellationToken required in async pipelines	✅ Required
* Separate packages for sync-only APIs	❌ Not recommended



---

🧭 Contributors

Name	Role

Uffe	Lead Architect

---

📄 Related Documents

/docs/architecture/transport-pipeline.md

/docs/analyzers/rules.md

/docs/telemetry/otel-integration.md