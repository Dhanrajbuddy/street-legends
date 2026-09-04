# Street Legends

Original free-to-play mobile 1v1 street-sports game. Unity / C#. Android + iOS.

> Rookie -> Improve -> Win -> Unlock -> Upgrade -> Compete -> Reach a higher league -> Become a Legend

## Status

| Phase | Name                  | Status      |
|-------|-----------------------|-------------|
| 0     | Project Foundation    | In progress |
| 1     | Gameplay Prototype    | Not started |
| 2     | Player Progression    | Not started |
| 3     | Content & Competition | Not started |
| 4     | Retention Systems     | Not started |
| 5     | Monetization          | Not started |
| 6     | Live Content          | Not started |
| 7     | Analytics             | Not started |
| 8     | Soft Launch           | Not started |
| 9     | Multiplayer           | Not started |

## Repository layout

```
StreetLegends/            <- git root == Unity project root
  Assets/                 <- created by Unity Hub (see docs/PROJECT_STRUCTURE.md)
  Packages/
  ProjectSettings/
  docs/                   <- design + engineering documentation
  .gitignore .gitattributes .editorconfig
```

## Documentation

- `docs/MVP_SCOPE.md` - what the MVP is and is not
- `docs/ARCHITECTURE.md` - runtime architecture, modules, data flow
- `docs/PROJECT_STRUCTURE.md` - Unity folder + assembly layout
- `docs/CODING_CONVENTIONS.md` - C# / Unity conventions
- `docs/BACKEND_AND_DATA.md` - backend architecture, save schema, database structure
- `docs/BUILD_AND_ENVIRONMENT.md` - tool versions, build configs, environments
- `docs/GIT_STRATEGY.md` - branching, commits, LFS, phase tags
- `docs/PHASE_PLAN.md` - phase-by-phase plan, complexity, completion criteria
- `docs/RISKS.md` - technical and product risks

## Principles

1. Fun before retention optimization. Retention before monetization. Quality before scale.
2. No dark patterns, no gambling mechanics, no pay-to-win.
3. Build in phases. Verify every phase before starting the next.
4. Data-driven balance: no economy or balance values hard-coded in gameplay code.
5. Never claim something is tested if it has not been run.
