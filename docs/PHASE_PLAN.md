# Phase Plan

Complexity scale: **S** (1-3 dev-days), **M** (1-2 weeks), **L** (2-4 weeks), **XL** (1-2 months). Estimates assume one experienced Unity developer plus placeholder art.

Every phase must produce: objective, requirements, architecture, implementation, tests, verification on device, completion criteria met, git tag.

---

## Phase 0 - Project Foundation  (S)  - current

Deliver: this documentation set, repo scaffolding, Unity project created and configured, empty assembly structure compiling, one passing EditMode smoke test, tag `phase-0-foundation`.

Completion criteria:
- [ ] Unity project opens on pinned version with URP + Input System, no errors.
- [ ] Folder + asmdef structure from `PROJECT_STRUCTURE.md` exists and compiles.
- [ ] `.gitignore`, `.gitattributes` (LFS), `.editorconfig` in place, first commit made.
- [ ] Empty `Boot`, `MainMenu`, `Match` scenes in Build Settings.
- [ ] Test Runner shows one green EditMode test.
- [ ] Dev Android build installs and shows the Boot scene.

## Phase 1 - Gameplay Prototype  (L)

Build: court, ball physics, character motor, touch controls (joystick + 1-2 buttons), one AI opponent, match state machine, timer, scoring, win/lose, restart.

Sub-steps with gates:
1. Character movement + ball dribbling feel (gate: "is it fun to move with the ball?").
2. Kick/shoot + goals + score.
3. Tackle/steal + AI opponent.
4. Match flow, HUD, restart.
5. Device test on mid-range Android.

Tests: EditMode for `MatchRules`, `MatchClock`, AI decision scoring; PlayMode for goal detection and match state transitions.

Completion: tester plays without explanation; controls judged fun by at least 3 testers; 60 FPS on reference device.

## Phase 2 - Player Progression  (M)

Build: `SaveData` + local save, `ProgressionService`, `WalletService`, `UpgradeService`, `RewardService`, post-match reward screen, profile panel, upgrade screen, config SOs (XP curve, rewards, upgrade costs), first economy model spreadsheet (`docs/economy/`).

Tests: XP curve monotonic, level-up boundaries, wallet never negative, idempotent grants, save round-trip and migration, "zero coins" and "insufficient XP" paths.

Completion: after a match the player sees exactly what they earned and what it unlocks next; progress survives app kill.

## Phase 3 - Content & Competition  (L)

Build: 3-5 AI personalities + difficulty scaling, league tiers with promotion rules (config-driven), league screen, 2-3 arenas, simple knockout tournament mode.

Tests: promotion/relegation rule engine, difficulty selection, tournament bracket logic.

Completion: player can name their medium-term goal ("2 wins to Silver").

## Phase 4 - Retention Systems  (M-L)

Build: daily/weekly mission engine (config-driven, `ITimeService`), achievements, login streak, deterministic unlocks, collection screen.

Tests: mission reset at UTC boundary, clock manipulation guard, double-claim prevention.

Completion: returning player has a visible, non-pressuring reason to play.

## Phase 5 - Monetization (sandbox)  (M-L)

Build: `IAdService` (mock + mediation SDK in sandbox), rewarded placements (double rewards, bonus coins), interstitial policy engine (config-driven frequency, never right after every match), `IPurchaseService` with sandbox products (Remove Ads, gem packs, starter bundle, cosmetic bundle), receipt validation via Cloud Code, gems server-authoritative.

Tests: ad fail path, ad callback twice, purchase fail/cancel/restore, offline purchase queue.

Completion: game fully playable with all monetization declined; no dark patterns per review checklist.

## Phase 6 - Live Content  (L)

Build: seasons framework, season pass (free/premium tracks), limited events, Remote Config overriding SO values, event scheduling, Cloud Save sync with conflict policy, Addressables for new content.

Completion: a new season is deployed with zero client rebuild.

## Phase 7 - Analytics  (M)

Build: `IAnalyticsService`, event taxonomy (session, match, progression, economy, ads, IAP), funnels and dashboards for DAU/MAU, D1/D7/D30, session metrics, ARPU/ARPPU, churn.

Completion: every KPI in the master prompt has a dashboard.

## Phase 8 - Soft Launch  (L, calendar-bound)

Internal -> closed testing -> 1-2 test countries. Iterate on onboarding, economy, ads with data. Store listings, privacy policy, age gating, consent (GDPR/COPPA/ATT).

## Phase 9 - Multiplayer  (XL)

Ghost/async challenges -> leaderboards -> async tournaments -> real-time PvP (requires dedicated server or relay + deterministic simulation; decide then).

---

## Cross-phase tracked TODOs

(none yet)
