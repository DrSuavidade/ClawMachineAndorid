# Phase 4: Non-P2W Economy & Rewarded Ad System Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Establish fair non-P2W balance: add rarity spawn weighting in `PrizeSpawner.cs` (Secret 5%, Rare 25%, Normal 70%), starter entry cost on Toy Box, and build `IAdvertisementService` mockable architecture offering rewarded ad refills ("Watch ad to get 5 free claw plays").

**Architecture:**
- Create `IAdvertisementService.cs` in `Assets/Scripts/Core/Services/`.
- Implement `MockAdvertisementService.cs` hooked into `ServiceLocator`.
- Update `PrizeSpawner.cs` to filter definitions by rarity and roll weighted random spawns.
- Add "Free Tries (Watch Ad)" button hook into `ArcadeConsoleUI.cs` when player coins are insufficient.

---

### Task 1: Rarity Weighted Spawns

**Files:**
- Modify: `Assets/Scripts/Gameplay/PrizeSpawner.cs`

- [ ] **Step 1: Weighted roll by rarity**
In `GetRandomDefinition()`:
Instead of uniform random across all 9 prizes, roll rarity tier:
- Normal: 70% chance (picks random Normal toy)
- Rare: 25% chance (picks random Rare toy)
- Secret: 5% chance (picks the Secret toy)
Guarantees authentic jackpot excitement when a shiny Secret appears in the pile.

---

### Task 2: Rewarded Ad Service & Refill Flow (No P2W)

**Files:**
- Create: `Assets/Scripts/Core/Services/IAdvertisementService.cs`
- Create: `Assets/Scripts/Gameplay/MockAdvertisementService.cs`
- Modify: `Assets/Scripts/Gameplay/CollectionManager.cs`
- Modify: `Assets/Scripts/UI/ArcadeConsoleUI.cs`

- [ ] **Step 1: Create IAdvertisementService interface**
Define `ShowRewardedVideo(Action onRewardSuccess, Action onFailed)`.

- [ ] **Step 2: Implement MockAdvertisementService**
Register in ServiceLocator at runtime, simulating or invoking rewarded ads (ready for AdMob/Unity Ads SDK).

- [ ] **Step 3: Connect "Watch Ad for 5 Free Plays" UI**
When coins are 0 or below entry cost, display a rewarded ad prompt button to award coins/tickets.

- [ ] **Step 4: Commit Phase 4 changes**
Commit changes with clean conventional commit message.
