# Risks and Technical Challenges

| # | Risk | Impact | Likelihood | Mitigation |
|---|------|--------|------------|------------|
| 1 | **Core controls are not fun on touch.** Joystick + buttons for a ball sport often feel mushy on phones. | Fatal | High | Phase 1 is gated on feel, not features. Prototype 2-3 control schemes (joystick+kick, tap-to-move, swipe-to-shoot). Tune ball "stickiness"/dribble assist early. Test on device, not in editor, from day one. |
| 2 | **AI feels either dumb or cheating.** | High | Medium | Utility AI with readable intent (telegraphed shots, reaction delay parameter). Difficulty via reaction time and error rate, never hidden stat boosts. |
| 3 | **Physics ball is unpredictable on mobile frame drops.** | High | Medium | Fixed timestep, interpolation, cap on ball velocity, tune in Phase 1 on reference device. |
| 4 | **Economy inflation/deflation.** Hard-coded numbers drift out of balance. | High | High | All values in configs; economy spreadsheet from Phase 2; sinks and sources modeled; remote config in Phase 6. |
| 5 | **Save corruption / lost progress.** | High | Medium | Atomic writes, schema versioning, backup slot, migration tests. |
| 6 | **Feature creep before the loop is validated.** | High | High | MVP scope doc; phase gates; "DO NOT BUILD YET" list enforced in reviews. |
| 7 | **Client-side economy exploited once ads/IAP exist.** | Medium | Medium | Server-authoritative gems from Phase 5, coins from Phase 6, idempotent grants, receipt validation. |
| 8 | **Ad SDK bloat and crashes on low-end Android.** | Medium | Medium | Single mediation SDK, add only in Phase 5, measure APK size and crash rate before/after. |
| 9 | **Performance on mid-range devices** (URP overdraw, UI rebuilds). | Medium | Medium | Reference device from Phase 1; profiler pass at each phase end; UI canvases split static/dynamic. |
| 10 | **Unity version/package churn.** | Low | Medium | Pin LTS; upgrade only between phases. |
| 11 | **Legal: resemblance to existing IP** (names, kits, logos, likenesses). | High | Low | Original names/characters/arenas; no real clubs, players, or brands; art review checklist before soft launch. |
| 12 | **Store compliance** (privacy, consent, age rating, loot-box disclosure laws). | Medium | Medium | No randomized paid loot; consent flow and privacy policy in Phase 8; deterministic unlocks preferred. |
| 13 | **Real-time multiplayer expectation.** | Medium | Low | Explicitly deferred; architecture keeps simulation separate from presentation so a deterministic path is possible later. |
| 14 | **Solo/small-team bandwidth.** | Medium | High | Phased scope, placeholder art, no custom backend until needed. |

## Product decisions

| Decision          | Status                     | Choice                              |
|-------------------|----------------------------|-------------------------------------|
| Sport             | Confirmed 2026-09-04       | 1v1 street football, caged court    |
| Camera            | Confirmed 2026-09-04       | Fixed 3D three-quarter              |
| Orientation       | Confirmed 2026-09-04       | Landscape                           |
| Backend           | Confirmed 2026-09-04       | Unity Gaming Services               |
| Engine            | Confirmed 2026-09-04       | Unity 6000.0 LTS + URP              |
| Bundle-id prefix  | **Open**                   | `com.<company>.streetlegends`       |
