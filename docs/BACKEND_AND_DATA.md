# Backend and Data

## Principle

MVP is **offline-first**. The client owns a local save; every backend feature is
an interface with a local implementation. Backend is introduced per phase,
never all at once.

## Backend: Unity Gaming Services (UGS) - confirmed 2026-09-04

| Need              | UGS product        | Phase |
|-------------------|--------------------|-------|
| Authentication    | Authentication (anonymous -> link Google/Apple) | 5-6 |
| Player save       | Cloud Save         | 6     |
| Currencies/inventory (server-authoritative) | Economy | 5-6 |
| Remote balance    | Remote Config      | 6     |
| Server logic (reward validation, receipts) | Cloud Code | 5-6 |
| Analytics         | Unity Analytics    | 7     |
| Leaderboards      | Leaderboards       | 9     |
| Ads               | Unity LevelPlay / AdMob | 5 |
| IAP               | Unity IAP + Cloud Code receipt validation | 5 |

Why UGS: single SDK family, C#-native, free tier covers soft launch, server-side
economy and receipt validation without running our own servers.

Alternative: Firebase (Auth, Firestore, Remote Config, Analytics) + Cloud
Functions. Better analytics tooling, more code to write for economy. Switching
cost is contained because all access goes through `Services` interfaces.

Own backend (Node/.NET + Postgres) is only justified for real-time PvP (Phase 9).

## Service interfaces (defined progressively)

```csharp
ISaveService        Task<SaveData> LoadAsync(); Task SaveAsync(SaveData);          // Phase 2
ITimeService        DateTime UtcNow;                                                // Phase 2 (mockable for missions)
IAuthService        Task<string> SignInAsync();                                      // Phase 5/6
IAdService          bool IsRewardedReady(placement); Task<AdResult> ShowRewardedAsync(placement); // Phase 5
IPurchaseService    Task<PurchaseResult> PurchaseAsync(productId); RestoreAsync();  // Phase 5
IRemoteConfigService T Get<T>(key, fallback);                                       // Phase 6
IAnalyticsService   void Track(string eventName, IDictionary<string, object> props); // Phase 7
```

Each gets `Local*` (JSON file / in-memory / mock) and `Ugs*` implementations.

## Save schema (local now, cloud later)

```json
{
  "schemaVersion": 1,
  "playerId": "local-guid",
  "createdUtc": "2026-09-04T10:00:00Z",
  "lastSavedUtc": "...",
  "profile":     { "displayName": "Rookie", "level": 1, "xp": 0, "avatarId": "default" },
  "wallet":      { "coins": 0, "gems": 0 },
  "characters":  { "rookie": { "unlocked": true, "upgrades": { "speed": 0, "power": 0, "skill": 0, "defense": 0, "stamina": 0 } } },
  "stats":       { "matchesPlayed": 0, "wins": 0, "losses": 0, "draws": 0, "goalsScored": 0 },
  "settings":    { "sfx": true, "music": true, "haptics": true },

  "league":      null,   // Phase 3: { "tier": "bronze", "points": 0, "promotionWindow": [...] }
  "missions":    null,   // Phase 4: { "dailyResetUtc": "...", "daily": [...], "weekly": [...] }
  "collection":  null,   // Phase 4
  "season":      null,   // Phase 6
  "grants":      { "recentGrantIds": [] }  // idempotency ring buffer
}
```

Rules:
- Add fields only; never rename or repurpose. Bump `schemaVersion` and add a migration step.
- `null` sub-documents mean "system not yet initialized", not "empty".
- Balances are stored as `long`.

## Database structure

### UGS mapping (Phases 5-7)

- Cloud Save keys: `profile`, `wallet` (read-only for client once Economy is on), `characters`, `stats`, `league`, `missions`, `collection`, `season`.
- Economy: currencies `COINS`, `GEMS`; inventory items for cosmetics/characters; virtual purchases for upgrades so cost validation is server-side.
- Remote Config keys mirror ScriptableObject config names: `xp_curve`, `match_rewards`, `upgrade_costs`, `ad_frequency`, `league_thresholds`.

### Relational reference schema (only if/when we run our own backend)

```
players(id PK, auth_provider, auth_uid, created_at, last_seen_at, display_name, country, app_version)
wallets(player_id PK/FK, coins BIGINT, gems BIGINT, updated_at)
wallet_transactions(id PK, player_id FK, currency, delta, balance_after, reason, ref_id UNIQUE, created_at)
player_characters(player_id FK, character_id, unlocked_at, PK(player_id, character_id))
player_upgrades(player_id FK, character_id, stat, level, PK(player_id, character_id, stat))
player_progress(player_id PK/FK, level, xp, league_tier, league_points)
matches(id PK, player_id FK, opponent_id, arena_id, result, player_score, ai_score, duration_s, ended_at, client_version)
missions(player_id FK, mission_id, period_start, progress, claimed_at, PK(player_id, mission_id, period_start))
inventory(player_id FK, item_id, acquired_at, source, PK(player_id, item_id))
purchases(id PK, player_id FK, store, product_id, receipt_hash UNIQUE, status, verified_at)
ad_events(id PK, player_id FK, placement, network, status, reward_grant_id, created_at)
remote_config(key PK, value JSONB, segment, updated_at)
seasons(id PK, name, starts_at, ends_at, config JSONB)
```

`wallet_transactions.ref_id UNIQUE` and `purchases.receipt_hash UNIQUE` are the
anti-duplicate guarantees.

## Security stance by phase

| Phase | Trust model |
|-------|-------------|
| 1-4   | Client-only. Local save is plain JSON. Acceptable: no real money involved. |
| 5     | Receipts validated server-side (Cloud Code). Rewarded ad grants use server-side callbacks (SSV) where the network supports it. Gems become server-authoritative. |
| 6+    | Coins and progression move to server-authoritative Economy; client sends match results with a signed summary; server applies rewards. |
| 9     | Leaderboard writes only from server. |
