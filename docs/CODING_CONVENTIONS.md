# Coding Conventions

## Language and style

- C# 9 features available in Unity 6 (records where useful for DTOs, pattern matching, target-typed `new`).
- `.editorconfig` at repo root is authoritative for formatting (4 spaces, CRLF-agnostic, braces on new lines).
- Nullable reference types are **off** (Unity serialization interop); guard with explicit null checks.

## Naming

| Element                    | Style              | Example                      |
|----------------------------|--------------------|------------------------------|
| Namespace                  | `StreetLegends.<Module>` | `StreetLegends.Gameplay.Ball` |
| Class / struct / enum      | PascalCase         | `MatchController`            |
| Interface                  | `I` + PascalCase   | `ISaveService`               |
| Method / property / event  | PascalCase         | `ApplyKick()`, `GoalScored`  |
| Private field              | `_camelCase`       | `_currentScore`              |
| Serialized private field   | `[SerializeField] private` + `_camelCase` | `_moveSpeed` |
| Local / parameter          | camelCase          | `deltaTime`                  |
| Constant                   | PascalCase         | `MaxPlayers`                 |
| ScriptableObject config    | `*Config` / `*Definition` | `XpCurveConfig`, `CharacterDefinition` |
| Save DTO                   | `*Data`            | `WalletData`                 |
| Async method               | `*Async`           | `LoadAsync()`                |

No Hungarian prefixes, no `m_`, no public fields on MonoBehaviours.

## MonoBehaviour discipline

- MonoBehaviours are thin adapters: input, physics callbacks, lifecycle, rendering. Logic goes in plain C# classes that can be unit tested.
- Keep any MonoBehaviour under ~200 lines; split otherwise.
- Never `FindObjectOfType`, `GameObject.Find`, or `Camera.main` in hot paths. Inject references via `[SerializeField]` or the registry at bootstrap.
- Cache component lookups in `Awake`.
- `Update` only where necessary; prefer events.
- No `Invoke("MethodName")` string calls; no `SendMessage`.

## Data-driven rule

Any number that a designer may want to change (durations, costs, rewards, speeds, cooldowns, thresholds) belongs in a ScriptableObject config. Code may contain only structural constants (array sizes, physics layer names, schema version).

## Errors and logging

- Use `Log.Info/Warn/Error(category, message)` wrapper (in Core), not raw `Debug.Log`, so logs can be stripped in release and routed to analytics later.
- Fail loudly in editor (`Debug.Assert`, exceptions); fail safe on device (fallback to defaults, log error).

## Async

- Use `async`/`await` with `Awaitable` (Unity 6) or UniTask if adopted; no coroutine for anything that can fail or needs cancellation.
- Every awaited service call has a timeout and a failure path.

## Economy safety

- Currency changes go through exactly one method (`WalletService.Apply(Transaction)`), which validates non-negative balances and records a reason code.
- Rewards are idempotent: every grant has a `grantId`; duplicates are rejected.

## Tests

- Pure logic -> EditMode tests (`[Test]`). Scene-dependent -> PlayMode tests.
- Test names: `MethodName_Condition_ExpectedResult`.
- Every phase adds tests for its new logic before the phase is declared complete.

## Comments

- Public API of `Services` and `Meta` gets XML doc comments. Otherwise comment *why*, not *what*.
- `// TODO(phaseN):` is the only allowed TODO format, and each one is tracked in `docs/PHASE_PLAN.md`.
