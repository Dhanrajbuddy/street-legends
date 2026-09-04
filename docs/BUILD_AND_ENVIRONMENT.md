# Build and Environment

## Tool versions (pin these)

| Tool                | Version                         | Notes |
|---------------------|---------------------------------|-------|
| Unity               | **6000.0.x LTS** (Unity 6 LTS)  | Latest patch of the LTS stream; record exact version in `ProjectSettings/ProjectVersion.txt` |
| Render pipeline     | URP (3D, Mobile template)       | |
| IDE                 | Rider or VS 2022 / VS Code + C# Dev Kit | |
| Git                 | 2.40+                           | |
| Git LFS             | 3.x                             | Required before first commit |
| Android             | SDK/NDK/JDK via Unity Hub module | Min API 24 (Android 7.0), target latest |
| iOS                 | Xcode 15+ on macOS              | Min iOS 13 |
| Android device      | Mid-range test device (4 GB RAM class) | Performance reference |

## Unity packages (MVP)

| Package                       | Purpose                        |
|-------------------------------|--------------------------------|
| `com.unity.render-pipelines.universal` | Rendering            |
| `com.unity.inputsystem`       | Touch + keyboard input         |
| `com.unity.textmeshpro`       | UI text                        |
| `com.unity.test-framework`    | Edit/Play mode tests           |
| `com.unity.cinemachine`       | Match camera (optional, evaluate in Phase 1) |
| `com.unity.mobile.android-logcat` | Device debugging (editor only) |

Added in later phases: `com.unity.services.*` (auth, cloudsave, economy, remoteconfig, analytics), `com.unity.purchasing`, ad mediation SDK, Addressables.

Do **not** add asset-store frameworks (DOTween, Odin, Zenject, etc.) in Phase 0/1. Evaluate only when a concrete pain exists.

## Project settings baseline

- Color space: Linear. Graphics API Android: Vulkan + OpenGLES3 fallback. iOS: Metal.
- Scripting backend: IL2CPP, ARM64 only (Android), `.NET Standard 2.1`.
- Managed stripping: Medium (raise in later phases; keep link.xml for serialization).
- Editor: Asset Serialization **Force Text**; Version Control **Visible Meta Files**.
- Orientation: **Landscape** locked (court is wider than tall). Revisit only with prototype evidence.
- Target frame rate: 60. Physics: Fixed timestep 0.02 (50 Hz) for prototype; tune in Phase 1.
- Input System: **Input System Package (New)** only.
- Quality: two levels, `Mobile-Low`, `Mobile-High`, auto-selected by device tier.

## Build configurations

| Config      | Scripting define      | Services                 | Logging  | Use |
|-------------|-----------------------|--------------------------|----------|-----|
| `Dev`       | `SL_DEV`              | Local mocks              | Verbose, on-screen console | Daily development |
| `Staging`   | `SL_STAGING`          | UGS test environment     | Info     | Internal/closed testing |
| `Release`   | (none)                | UGS production           | Errors only | Store |

Implementation: one `EnvironmentConfig` ScriptableObject per config selected by
define at boot; `Bootstrap` chooses service implementations from it. Build
scripts live in `Assets/_Project/Scripts/Editor/Build/` and are invoked with
`-executeMethod` for CI.

## Environments

- Secrets (UGS project id, store keys) are never committed. Editor reads them from `ProjectSettings/` UGS linking (safe) or from `.env.local` (git-ignored) for CI.
- Bundle identifier: `com.<company>.streetlegends` (`.dev` suffix for Dev builds so they install side-by-side).

## CI (Phase 3+, not now)

GitHub Actions + GameCI: run EditMode tests on every PR, produce Android Dev
APK on `develop`. Defined here so the structure supports it; not implemented in Phase 0.

## Developer workflow

1. Clone, `git lfs install`, open in Unity Hub with the pinned version.
2. Open `Boot` scene, press Play. Everything must run with no network.
3. Run tests via **Window > General > Test Runner** before every PR.
4. Build `Dev` Android APK and test on device before completing each phase.
