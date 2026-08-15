# ConsoleScroller Test Infrastructure (Touchstone)

Tests are written **once** in `Test.Shared` as [Touchstone](https://www.nuget.org/packages/Touchstone)
descriptors and executed unchanged by three hosts:

| Project          | Role                              | How to run                                            |
|------------------|-----------------------------------|-------------------------------------------------------|
| `Test.Shared`    | Source of truth (test descriptors)| n/a (class library)                                   |
| `Test.Automated` | Touchstone CLI runner             | `dotnet run --project Test.Automated -f net8.0`       |
| `Test.Xunit`     | Touchstone xUnit adapter          | `dotnet test Test.Xunit`                              |
| `Test.Nunit`     | Touchstone NUnit adapter          | `dotnet test Test.Nunit`                              |

`Test.Automated` optionally takes a results path: `dotnet run --project Test.Automated -f net8.0 -- results.json`.

## Why some tests are skipped

`ConsoleScroller.Scroller` drives the terminal through `Console.CursorTop`, `Console.BufferHeight`,
`Console.WindowWidth`, and `Console.SetCursorPosition`. Those APIs throw
`IOException: The handle is invalid.` whenever standard output is a redirected pipe or file — which is
exactly the case under `dotnet test`, CI, or any captured/headless run.

To stay deterministic and green in every host, the suites split into two groups:

* **Console-independent** cases — constructor argument validation and the reflected public API surface.
  These run everywhere.
* **Console-dependent** cases — anything that constructs a live `Scroller` (construction, `WriteLine`,
  colors, scrolling/eviction, and the `Dispose`/`ObjectDisposedException` contract). These are gated on
  `ConsoleAvailability.IsInteractive`: they **execute and assert real behavior in a genuine terminal**
  and are **skipped** (never failed) when no interactive console is present.

So a headless run reports the console-independent cases as passing and the console-dependent cases as
skipped; running `Test.Automated` in a real terminal executes the full set.
