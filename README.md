# NCFE Code Test – LearnerService refactor

## Build & test

```
dotnet test Ncfe.CodeTest.sln
```

Targets .NET Framework 4.8 (unchanged). The project was converted to an SDK-style csproj so it builds with the
`dotnet` CLI as well as Visual Studio. Tests use **xUnit** and **Moq**.

## What was wrong with the original `GetLearner`

- **Many responsibilities in one method:** archive lookup, reading config, counting failures, choosing a data
  store and mapping the response.
- **Hard-wired dependencies:** `new`, a static call (`FailoverLearnerDataAccess`), `ConfigurationManager` and
  `DateTime.Now`. This made the method impossible to unit test.
- **Magic numbers:** the `100` failures and the `10` minute window were hard coded, and the `"true" || "True"`
  string comparison was brittle.
- **Wasted work:** the failover repository (a DB call) was queried even when failover was disabled in config.
- **No null handling:** a null response from a data store threw a `NullReferenceException`.

## Approach

The three classes whose signatures cannot change (`ArchivedDataService`, `LearnerDataAccess`,
`FailoverLearnerDataAccess`) are wrapped in **adapters**, so the rest of the code only depends on abstractions.

| Component | Responsibility |
|---|---|
| `LearnerService` | Orchestration only. It reads archived learners from the archive, otherwise it reads from the data store, falling back to the archive if the response says the learner is archived. |
| `IArchivedLearnerStore` → `ArchivedLearnerStore` | Adapter over `ArchivedDataService`. |
| `ILearnerDataStore` → `MainLearnerDataStore` | Adapter over the 3rd party `LearnerDataAccess`. |
| `ILearnerDataStore` → `FailoverLearnerDataStore` | Adapter over the static `FailoverLearnerDataAccess`. |
| `ILearnerDataStore` → `FailoverAwareLearnerDataStore` | Composite that routes each request to the main or failover store. |
| `IFailoverModeEvaluator` → `FailoverModeEvaluator` | Failover policy: enabled **and** more than *N* failures within window *W*. |
| `IFailoverSettings` → `AppSettingsFailoverSettings` | Config access. Threshold and window are now configurable, defaulting to 100 and 10 minutes. |
| `IClock` → `SystemClock` | Makes the time window deterministic in tests. |
| `IFailoverRepository` | Extracted from `FailoverRepository`. |

How this maps to SOLID:

- **S:** every class has one reason to change: the policy, the config, the routing, or the orchestration.
- **O:** a new source, such as a cache or a third store, or a new failover policy, is a new implementation of
  an existing interface. `LearnerService` does not change.
- **L:** the main, failover and failover-aware stores can stand in for each other behind `ILearnerDataStore`.
- **I:** the interfaces are small and role specific.
- **D:** `LearnerService` and `FailoverModeEvaluator` depend only on abstractions, injected through the constructor.

The parameterless `LearnerService()` constructor wires up the production defaults, so existing callers keep
working. New code should resolve `LearnerService` from a DI container.

### Behaviour changes, all deliberate and small

- `IsFailoverModeEnabled` is now parsed with `bool.TryParse`, so values like `TRUE` also count as enabled.
- The config flag is checked before the repository is queried.
- A null data store response returns `null` instead of throwing.

## Further improvements I would make

1. **Composition root / DI container:** register everything in `Microsoft.Extensions.DependencyInjection` or
   Autofac and remove the parameterless constructor ("poor man's DI").
2. **Async all the way:** add `Task<Learner> GetLearnerAsync(...)` built on async adapters. The data stores are
   I/O bound, a web service and a database.
3. **Resilience:** use Polly (retry, timeout, circuit breaker) around the 3rd party call. Failed calls should
   also *record* a `FailoverEntry`. Currently nothing in this code writes those entries, so the failover counter
   depends on some other process.
4. **Query the count in the database:** for example `CountFailuresSince(DateTime)`, instead of loading every
   `FailoverEntry` into memory on every request. Cache the result of `IsInFailoverMode` briefly, since it runs
   on every learner lookup.
5. **UTC everywhere:** `DateTime.Now` is ambiguous around DST changes. Use `DateTime.UtcNow`, or
   `TimeProvider` on a newer runtime.
6. **Typed options with validation:** use `IOptions<FailoverOptions>` once on modern .NET.
7. **Logging and metrics:** log when the system enters or leaves failover mode, and which store served each
   request.
8. **API shape:** `GetLearner(int, bool)` with a boolean flag is a code smell. Separate `GetArchivedLearner`
   and `GetLearner` methods, or a request object, would be clearer. I left it unchanged to avoid breaking
   callers.
9. **Not-found semantics:** decide whether a missing learner should be `null`, a `Try...` pattern or a
   result type, and apply it consistently.
10. **Migrate to modern .NET (8+)** and enable nullable reference types.
11. **Integration tests** for the real adapters and config, plus CI running `dotnet test`.
