# Git Strategy

## Repository

- Git root == Unity project root (`StreetLegends/`).
- Git LFS for all binary assets (see `.gitattributes`). Run `git lfs install` before the first commit.
- Unity `.gitignore` excludes `Library/`, `Temp/`, `Logs/`, `Build/`, IDE files.

## Branches

| Branch            | Purpose                                              |
|-------------------|------------------------------------------------------|
| `main`            | Always buildable, tagged phase checkpoints and releases |
| `develop`         | Integration branch for the current phase             |
| `feature/<name>`  | Short-lived work branches, merged into `develop` via PR |
| `fix/<name>`      | Bug fixes                                            |
| `release/<x.y.z>` | Soft-launch and later store release stabilization (Phase 8+) |

Solo-developer simplification: `develop` may be skipped and features merged
directly into `main`, as long as `main` stays green and phase tags are made.

## Phase checkpoints

At the end of each phase, on `main`:

```
git tag -a phase-0-foundation           -m "Phase 0: project foundation"
git tag -a phase-1-gameplay-prototype   -m "Phase 1: gameplay prototype"
git tag -a phase-2-progression          -m "..."
git tag -a phase-3-content
git tag -a phase-4-retention
git tag -a phase-5-monetization
git tag -a phase-6-live-content
git tag -a phase-7-analytics
git tag -a phase-8-soft-launch
git tag -a phase-9-multiplayer
```

Requirements before tagging: tests pass, device build verified, docs updated,
`README.md` phase table updated.

## Commits

Conventional Commits:

```
feat(gameplay): add ball kick impulse scaled by power stat
fix(save): migrate v1 -> v2 wallet field
data(economy): tune xp curve levels 1-10
docs(phase-1): add controls testing checklist
test(meta): cover WalletService negative balance rejection
chore(build): pin Unity 6000.0.x
```

Scopes: `core`, `data`, `services`, `meta`, `gameplay`, `ui`, `audio`, `build`, `save`, `phase-N`.

## Unity-specific rules

- Never commit `Library/`. Never commit a scene with unsaved prefab overrides you did not intend.
- One scene per feature branch where possible to avoid scene merge conflicts. If a conflict happens, use Unity **Smart Merge** (`UnityYAMLMerge`) configured in `.gitconfig`:
  ```
  [merge]
      tool = unityyamlmerge
  [mergetool "unityyamlmerge"]
      trustExitCode = false
      cmd = '<UnityInstall>/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"
  ```
- Commit `.meta` files always. Missing meta = broken references.
- Squash-merge feature branches; keep `main` history readable.
