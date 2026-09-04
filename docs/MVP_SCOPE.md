# MVP Scope

## Product definition

**Street Legends** - fast-paced 1v1 street-sports match, ~2-3 minutes, player vs AI, on a small walled court.

### Sport (confirmed 2026-09-04)

**1v1 street football (soccer) on a caged court, one goal per side.**

Rationale:
- One ball, two goals: instantly readable, no tutorial needed.
- Walls keep the ball in play -> no dead time, fits 2-3 minute matches.
- Naturally supports the planned ability set (Power Shot, Curve, Dash, Tackle, Shield).
- Broadest global audience for a street-sports theme.

Alternative considered: 1v1 street basketball (single hoop). Deferred - shot mechanics on touch are harder to make satisfying quickly.

### Camera / perspective (confirmed 2026-09-04)

3D, fixed elevated three-quarter camera looking down the length of the court, court fully visible. Character and ball read clearly at phone size. URP, stylized low-poly art later; primitives during prototype.

## MVP contents (and nothing else)

| Area        | In MVP                                                        |
|-------------|---------------------------------------------------------------|
| Menu        | Main menu, Play button, Profile panel, Settings (sound on/off) |
| Characters  | 1 playable character ("Rookie")                               |
| Arena       | 1 court                                                       |
| Gameplay    | Movement, one primary action (kick/shoot), one secondary (tackle/steal), ball physics |
| Opponent    | 1 AI opponent with 2-3 difficulty levels                      |
| Match       | Countdown, timer, scoring, win/lose/draw, restart, quit       |
| Progression | XP, levels, coins, stat upgrades, post-match reward screen    |
| Persistence | Local save/load of player profile (versioned JSON)            |
| Audio       | Basic UI, kick, goal, whistle, win/lose stingers              |
| UI          | Functional, readable, touch-sized                             |

## Explicitly out of MVP

Real-time multiplayer, clans, chat, friends, matchmaking, large roster, season pass,
loot systems, real IAP, production ad SDKs, backend infrastructure beyond interfaces,
leagues (Phase 3), missions (Phase 4), analytics SDK (Phase 7).

## MVP success criteria

1. A new tester launches and plays a match with no verbal explanation within 30 seconds.
2. Tester says the character is fun to control (Phase 1 gate).
3. After a match, tester can state what they get for playing another one (Phase 2 gate).
4. 60 FPS on a mid-range Android device (e.g. Snapdragon 6xx class, 4 GB RAM).
5. Progress survives app kill and relaunch.
