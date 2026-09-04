# Architecture

## Goals

- Gameplay code knows nothing about menus, saves, ads, or backend.
- Every economy/balance number lives in a ScriptableObject or remote config, not in code.
- Every external dependency (save, auth, ads, IAP, analytics, remote config) sits behind an interface with a local/mock implementation so the game runs fully offline in the editor.
- Pure C# logic (XP curves, rewards, match rules, league promotion) is testable without a scene.

## Layered modules

```
+--------------------------------------------------------------+
|  Presentation      UI screens, HUD, VFX, audio triggers       |
+--------------------------------------------------------------+
|  Gameplay          Match, Court, Ball, Character, AI, Input   |
+--------------------------------------------------------------+
|  Meta              Progression, Economy, Upgrades, Leagues,   |
|                    Missions, Collection, Seasons (later)      |
+--------------------------------------------------------------+
|  Services (interfaces)  ISaveService, IAuthService, IAdService,|
|                    IPurchaseService, IAnalyticsService,        |
|                    IRemoteConfigService, ITimeService          |
+--------------------------------------------------------------+
|  Services.Local / Services.Ugs   concrete implementations     |
+--------------------------------------------------------------+
|  Data              ScriptableObject configs + save DTOs       |
+--------------------------------------------------------------+
|  Core              Bootstrap, ServiceRegistry, EventBus,      |
|                    SceneLoader, Logging, utilities            |
+--------------------------------------------------------------+
```

Dependency direction is strictly downward. `Gameplay` and `Meta` may depend on
`Services` interfaces and `Data`, never on `Services.Local`/`Services.Ugs`
or on `Presentation`.

## Assemblies (asmdef)

| Assembly                       | Depends on                                  |
|--------------------------------|---------------------------------------------|
| `StreetLegends.Core`           | -                                           |
| `StreetLegends.Data`           | Core                                        |
| `StreetLegends.Services`       | Core, Data                                  |
| `StreetLegends.Services.Local` | Services                                    |
| `StreetLegends.Meta`           | Core, Data, Services                        |
| `StreetLegends.Gameplay`       | Core, Data, Services, Meta (read-only stats)|
| `StreetLegends.Presentation`   | Core, Data, Services, Meta, Gameplay        |
| `StreetLegends.Bootstrap`      | everything (composition root)               |
| `StreetLegends.Tests.EditMode` | Core, Data, Meta, Services                  |
| `StreetLegends.Tests.PlayMode` | + Gameplay, Presentation                    |

Assembly definitions enforce the dependency direction at compile time and cut iteration time.

## Composition and wiring

- **Bootstrap scene** (`Boot`) is the only entry point. `GameBootstrapper` builds concrete services, registers them in a small `ServiceRegistry`, loads the save, then loads `MainMenu`.
- No DI framework in MVP. A hand-written registry (`ServiceRegistry.Get<ISaveService>()`) is enough; swap to VContainer later only if registration grows unwieldy.
- No `FindObjectOfType`, no singletons other than the registry.

## Events

- Domain events are plain C# `event`/`Action<T>` on the owning system (e.g. `MatchController.GoalScored`), or a lightweight typed `EventBus` for cross-module fan-out (`PlayerLeveledUp`, `CurrencyChanged`).
- UI subscribes; systems never reference UI.

## Gameplay runtime (Phase 1 shape)

```
MatchController (state machine: Countdown -> Playing -> GoalScored -> Playing -> Finished)
  |- MatchClock
  |- ScoreBoard
  |- Court (bounds, goals with trigger volumes)
  |- Ball (Rigidbody, IKickable)
  |- PlayerCharacter (CharacterMotor + CharacterActions) <- IInputSource (touch / keyboard)
  |- AiCharacter     (CharacterMotor + CharacterActions) <- AiBrain (utility-based decisions)
  |- MatchRules (pure C#: win/lose/draw, overtime policy from MatchConfig)
```

`CharacterMotor` and `CharacterActions` are shared; only the controller differs
(input vs AI). Stats come from `CharacterRuntimeStats` built from
`CharacterDefinition` (SO) + player upgrades (Meta).

## Meta runtime (Phase 2 shape)

```
PlayerProfile (in-memory model)  <-> ISaveService (JSON, versioned, migrations)
ProgressionService  (XP -> level via XpCurveConfig)
WalletService       (coins/gems; single mutation point, emits CurrencyChanged)
UpgradeService      (spend coins -> stat level via UpgradeCostConfig)
RewardService       (MatchResult -> RewardBundle via MatchRewardConfig)
```

All numeric rules are in `*Config` ScriptableObjects under `Assets/_Project/Data/`.
Later, `IRemoteConfigService` overrides selected config values at boot.

## Save system

- Single `SaveData` root with `schemaVersion`, sub-documents per domain.
- Written atomically (temp file + rename), autosaved on meaningful state changes and `OnApplicationPause`.
- Migration chain `v1 -> v2 -> ...` in `SaveMigrator`.
- Same DTO shape is later uploaded to cloud save; local remains the cache.

## Scenes

| Scene       | Purpose                                             |
|-------------|-----------------------------------------------------|
| `Boot`      | Composition root, loading                           |
| `MainMenu`  | Menu, profile, upgrades, (later) leagues/missions   |
| `Match`     | Court + gameplay; receives a `MatchSetup` payload    |

`Match` is loaded additively over a persistent `Systems` object living from `Boot`.

## Anti-goals

- No giant `GameManager`.
- No static mutable state except the registry.
- No balance numbers in code (`const int XP_PER_WIN = 50` is forbidden).
- No feature flags for systems that do not exist yet; add the interface when the phase arrives.
