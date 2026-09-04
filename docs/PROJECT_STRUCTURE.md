# Unity Project Structure

Everything we author lives under `Assets/_Project/`. Third-party packages and
imported assets go in `Assets/ThirdParty/` (or come via Package Manager).
The underscore keeps our folder at the top of the Project window.

```
Assets/
  _Project/
    Art/                    (placeholder primitives in prototype)
      Materials/
      Models/
      Textures/
      Animations/
    Audio/
      Music/
      Sfx/
    Data/                   ScriptableObject instances (the "balance sheet")
      Characters/           CharacterDefinition assets
      Abilities/
      Economy/              XpCurveConfig, UpgradeCostConfig, MatchRewardConfig
      Match/                MatchConfig (duration, rules), AiDifficultyConfig
      Leagues/              (Phase 3)
      Missions/             (Phase 4)
    Prefabs/
      Gameplay/             Ball, Court, Character, Goals
      UI/                   Screens, widgets, HUD
      Systems/              Persistent systems prefab
    Scenes/
      Boot.unity
      MainMenu.unity
      Match.unity
    Scripts/
      Core/                 StreetLegends.Core.asmdef
      Data/                 StreetLegends.Data.asmdef
      Services/             StreetLegends.Services.asmdef (interfaces only)
      Services.Local/       StreetLegends.Services.Local.asmdef
      Meta/                 StreetLegends.Meta.asmdef
      Gameplay/             StreetLegends.Gameplay.asmdef
        Match/
        Court/
        Ball/
        Character/
        Ai/
        Input/
      Presentation/         StreetLegends.Presentation.asmdef
        Hud/
        Screens/
        Audio/
        Vfx/
      Bootstrap/            StreetLegends.Bootstrap.asmdef
      Editor/               StreetLegends.Editor.asmdef (tools, validators)
    Settings/
      Input/                InputActions asset
      Rendering/            URP assets, quality levels
    Tests/
      EditMode/             StreetLegends.Tests.EditMode.asmdef
      PlayMode/             StreetLegends.Tests.PlayMode.asmdef
  ThirdParty/
```

## Rules

- One class per file, filename == class name.
- Scripts folder mirrors assembly boundaries; a script's folder decides its assembly.
- Prefab variants over duplicated prefabs.
- Scenes contain only scene-specific objects; systems live in the persistent `Systems` prefab spawned by `Boot`.
- `Resources/` is not used (Addressables later if needed for live content).
- Scene and prefab files are always saved as **Force Text** (set in Editor settings) so they diff and merge.
