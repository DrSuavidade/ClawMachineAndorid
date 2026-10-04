# Graph Report - ClawMachine  (2026-10-01)

## Corpus Check
- 54 files · ~47,766 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1068 nodes · 1771 edges · 59 communities (56 shown, 3 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS · INFERRED: 7 edges (avg confidence: 0.89)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `247bd5db`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- [[_COMMUNITY_Economy & Data Architecture|Economy & Data Architecture]]
- [[_COMMUNITY_Arcade Controls & Console|Arcade Controls & Console]]
- [[_COMMUNITY_Claw Mechanics & Physics|Claw Mechanics & Physics]]
- [[_COMMUNITY_Cabinet & Prize Lifecycle|Cabinet & Prize Lifecycle]]
- [[_COMMUNITY_Juice & Audio Effects|Juice & Audio Effects]]
- [[_COMMUNITY_Module 5 (Collider)|Module 5 (Collider)]]
- [[_COMMUNITY_Claw Mechanics & Physics|Claw Mechanics & Physics]]
- [[_COMMUNITY_Claw Mechanics & Physics|Claw Mechanics & Physics]]
- [[_COMMUNITY_Module 8 (manifest.json)|Module 8 (manifest.json)]]
- [[_COMMUNITY_Claw Mechanics & Physics|Claw Mechanics & Physics]]
- [[_COMMUNITY_Cabinet & Prize Lifecycle|Cabinet & Prize Lifecycle]]
- [[_COMMUNITY_Cabinet & Prize Lifecycle|Cabinet & Prize Lifecycle]]
- [[_COMMUNITY_Cabinet & Prize Lifecycle|Cabinet & Prize Lifecycle]]
- [[_COMMUNITY_Cabinet & Prize Lifecycle|Cabinet & Prize Lifecycle]]
- [[_COMMUNITY_Module 14 (dependencies)|Module 14 (dependencies)]]
- [[_COMMUNITY_Module 15 (dependencies)|Module 15 (dependencies)]]
- [[_COMMUNITY_Cabinet & Prize Lifecycle|Cabinet & Prize Lifecycle]]
- [[_COMMUNITY_Progression & Retention UI|Progression & Retention UI]]
- [[_COMMUNITY_Claw Mechanics & Physics|Claw Mechanics & Physics]]
- [[_COMMUNITY_Module 19 (dependencies)|Module 19 (dependencies)]]
- [[_COMMUNITY_Community 20|Community 20]]
- [[_COMMUNITY_Claw Mechanics & Physics|Claw Mechanics & Physics]]
- [[_COMMUNITY_Module 22 (dependencies)|Module 22 (dependencies)]]
- [[_COMMUNITY_Cabinet & Prize Lifecycle|Cabinet & Prize Lifecycle]]
- [[_COMMUNITY_Module 24 (Func)|Module 24 (Func)]]
- [[_COMMUNITY_Claw Mechanics & Physics|Claw Mechanics & Physics]]
- [[_COMMUNITY_Module 26 (dependencies)|Module 26 (dependencies)]]
- [[_COMMUNITY_Claw Mechanics & Physics|Claw Mechanics & Physics]]
- [[_COMMUNITY_Module 28 (dependencies)|Module 28 (dependencies)]]
- [[_COMMUNITY_Cabinet & Prize Lifecycle|Cabinet & Prize Lifecycle]]
- [[_COMMUNITY_Progression & Retention UI|Progression & Retention UI]]
- [[_COMMUNITY_Module 31 (depth)|Module 31 (depth)]]
- [[_COMMUNITY_Claw Mechanics & Physics|Claw Mechanics & Physics]]
- [[_COMMUNITY_Claw Mechanics & Physics|Claw Mechanics & Physics]]
- [[_COMMUNITY_Claw Mechanics & Physics|Claw Mechanics & Physics]]
- [[_COMMUNITY_Community 35|Community 35]]
- [[_COMMUNITY_Module 36 (packages-lock.json)|Module 36 (packages-lock.json)]]
- [[_COMMUNITY_Claw Mechanics & Physics|Claw Mechanics & Physics]]
- [[_COMMUNITY_Claw Mechanics & Physics|Claw Mechanics & Physics]]
- [[_COMMUNITY_Community 39|Community 39]]
- [[_COMMUNITY_Module 40 (dependencies)|Module 40 (dependencies)]]
- [[_COMMUNITY_Module 41 (dependencies)|Module 41 (dependencies)]]
- [[_COMMUNITY_Community 42|Community 42]]
- [[_COMMUNITY_Community 43|Community 43]]
- [[_COMMUNITY_Community 44|Community 44]]
- [[_COMMUNITY_Community 45|Community 45]]
- [[_COMMUNITY_Claw Mechanics & Physics|Claw Mechanics & Physics]]
- [[_COMMUNITY_Claw Mechanics & Physics|Claw Mechanics & Physics]]
- [[_COMMUNITY_Module 48 (float)|Module 48 (float)]]
- [[_COMMUNITY_Community 49|Community 49]]
- [[_COMMUNITY_Community 50|Community 50]]
- [[_COMMUNITY_Community 51|Community 51]]
- [[_COMMUNITY_Community 52|Community 52]]
- [[_COMMUNITY_Claw Mechanics & Physics|Claw Mechanics & Physics]]
- [[_COMMUNITY_Module 56 (dependencies)|Module 56 (dependencies)]]
- [[_COMMUNITY_Module 57 (dependencies)|Module 57 (dependencies)]]
- [[_COMMUNITY_Claw Mechanics & Physics|Claw Mechanics & Physics]]
- [[_COMMUNITY_Module 60 (ProjectVersion.txt)|Module 60 (ProjectVersion.txt)]]
- [[_COMMUNITY_Community 65|Community 65]]

## God Nodes (most connected - your core abstractions)
1. `CollectionManager` - 50 edges
2. `ClawController` - 37 edges
3. `ProgressModalUI` - 36 edges
4. `ClawAudio` - 34 edges
5. `ArcadeConsoleUI` - 33 edges
6. `MachineController` - 30 edges
7. `Phase0SceneSetup` - 24 edges
8. `PrizeSpawner` - 24 edges
9. `CollectionUI` - 24 edges
10. `PrizeWinModalUI` - 24 edges

## Surprising Connections (you probably didn't know these)
- `CollectionManager` --implements--> `ICollectionService`  [EXTRACTED]
  Assets/Scripts/Gameplay/CollectionManager.cs → Assets/Scripts/Core/Services/ICollectionService.cs
- `CollectionManager` --implements--> `IEconomyService`  [EXTRACTED]
  Assets/Scripts/Gameplay/CollectionManager.cs → Assets/Scripts/Core/Services/IEconomyService.cs
- `ClawAudio` --implements--> `IAudioService`  [EXTRACTED]
  Assets/Scripts/Gameplay/ClawAudio.cs → Assets/Scripts/Core/Services/IAudioService.cs

## Import Cycles
- None detected.

## Communities (59 total, 3 thin omitted)

### Community 0 - "Economy & Data Architecture"
Cohesion: 0.08
Nodes (16): float, int, ISaveService, List, long, MachineCatalog, MachineDefinition, Prize (+8 more)

### Community 1 - "Arcade Controls & Console"
Cohesion: 0.08
Nodes (18): Action, Button, float, GameObject, Image, int, IRetentionService, PointerEventData (+10 more)

### Community 2 - "Claw Mechanics & Physics"
Cohesion: 0.10
Nodes (16): bool, float, Quaternion, Transform, Vector3, ClawMachine.Gameplay, ClawGripAnchor, ClawMachine.Gameplay (+8 more)

### Community 3 - "Cabinet & Prize Lifecycle"
Cohesion: 0.06
Nodes (26): bool, Button, Coroutine, float, GameObject, IEnumerator, int, MachineCatalog (+18 more)

### Community 4 - "Juice & Audio Effects"
Cohesion: 0.07
Nodes (12): AudioClip, AudioClip, bool, Camera, float, string, Vector3, AudioSource (+4 more)

### Community 5 - "Module 5 (Collider)"
Cohesion: 0.23
Nodes (6): Color, Material, Vector3, ClawJuiceEffects, ClawMachine.Gameplay, ParticleSystem

### Community 6 - "Claw Mechanics & Physics"
Cohesion: 0.05
Nodes (27): bool, ClawState, Color, Coroutine, float, IEnumerator, Image, MachineController (+19 more)

### Community 7 - "Claw Mechanics & Physics"
Cohesion: 0.11
Nodes (22): Button, ChuteDetector, ClawConfiguration, ClawController, GameObject, Light, MachineCatalog, MachineController (+14 more)

### Community 8 - "Module 8 (manifest.json)"
Cohesion: 0.09
Nodes (22): dependencies, com.unity.modules.accessibility, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.assetbundle, com.unity.modules.audio, com.unity.modules.imageconversion (+14 more)

### Community 9 - "Claw Mechanics & Physics"
Cohesion: 0.10
Nodes (12): ClawConfiguration, ClawState, float, Prize, Transform, Vector2, Vector3, Bounds (+4 more)

### Community 10 - "Cabinet & Prize Lifecycle"
Cohesion: 0.26
Nodes (11): Color, GameObject, MachineDefinition, Material, MenuItem, PhysicsMaterial, PrizeDefinition, PrizeRarity (+3 more)

### Community 11 - "Cabinet & Prize Lifecycle"
Cohesion: 0.17
Nodes (11): ChuteDetector, float, int, List, MachineDefinition, PhysicsMaterial, PrizeDefinition, Quaternion (+3 more)

### Community 12 - "Cabinet & Prize Lifecycle"
Cohesion: 0.26
Nodes (11): Color, GameObject, MachineDefinition, Material, MenuItem, PhysicsMaterial, PrizeDefinition, PrizeRarity (+3 more)

### Community 13 - "Cabinet & Prize Lifecycle"
Cohesion: 0.14
Nodes (9): bool, Button, GameObject, List, MachineDefinition, PrizeDefinition, Text, Transform (+1 more)

### Community 14 - "Module 14 (dependencies)"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 15 - "Module 15 (dependencies)"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, version, dependencies, depth, source, version (+7 more)

### Community 16 - "Cabinet & Prize Lifecycle"
Cohesion: 0.29
Nodes (9): Color, GameObject, MachineDefinition, Material, MenuItem, PrizeDefinition, PrizeRarity, AdditionalMachinesGenerator (+1 more)

### Community 17 - "Progression & Retention UI"
Cohesion: 0.15
Nodes (9): int, IReadOnlyList, CollectionManager, DailyQuestData, double, RetentionService, IRetentionService, MilestoneRewardData (+1 more)

### Community 18 - "Claw Mechanics & Physics"
Cohesion: 0.08
Nodes (18): ChuteDetector, ClawController, ClawState, Coroutine, float, IEnumerator, int, Light (+10 more)

### Community 19 - "Module 19 (dependencies)"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, version, dependencies, depth, source, version (+7 more)

### Community 20 - "Community 20"
Cohesion: 0.25
Nodes (6): float, Rigidbody, Transform, Vector2, Vector3, ClawPendulumSway

### Community 21 - "Claw Mechanics & Physics"
Cohesion: 0.13
Nodes (4): MachineDefinition, Prize, ClawMachine.Core.Services, ICollectionService

### Community 22 - "Module 22 (dependencies)"
Cohesion: 0.14
Nodes (15): dependencies, depth, source, version, dependencies, depth, source, version (+7 more)

### Community 23 - "Cabinet & Prize Lifecycle"
Cohesion: 0.11
Nodes (13): bool, Button, Camera, Coroutine, GameObject, IEnumerator, Image, Prize (+5 more)

### Community 24 - "Module 24 (Func)"
Cohesion: 0.23
Nodes (9): Func, int, long, string, T, ISaveService, ClawMachine.Core.Services, JsonFileSaveService (+1 more)

### Community 25 - "Claw Mechanics & Physics"
Cohesion: 0.16
Nodes (10): ClawMachine.Gameplay, ClawMachine.Gameplay, ClawMachine.Gameplay, ClawMachine.Gameplay, Dynamic Cinematic Camera Tracking, Active Claw Cable Pendulum Sway, 7-Day Streak & Daily Quest Retention, Claw Cable Pendulum Sway Kinematics (+2 more)

### Community 26 - "Module 26 (dependencies)"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.androidjni

### Community 27 - "Claw Mechanics & Physics"
Cohesion: 0.05
Nodes (32): ClawMachine.Data, ClawMachine.Gameplay, ClawMachine.Gameplay, Modular Machine Catalog Economy, Modular Catalog Economy & Skeuomorphic Arcade Deck Implementation Plan, Slim Skeuomorphic Control Deck, Task 1: Modular MachineCatalog & Scaled Tier Economy, Task 2: Skeuomorphic Ball-Top Arcade Joystick (+24 more)

### Community 28 - "Module 28 (dependencies)"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 29 - "Cabinet & Prize Lifecycle"
Cohesion: 0.18
Nodes (6): bool, Button, GameObject, Text, ClawMachine.UI, SettingsModalUI

### Community 30 - "Progression & Retention UI"
Cohesion: 0.33
Nodes (4): PrizeDefinition, PrizeRarity, ClawMachine.Gameplay, Prize

### Community 31 - "Module 31 (depth)"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 32 - "Claw Mechanics & Physics"
Cohesion: 0.18
Nodes (4): MachineDefinition, UpgradeType, ClawMachine.Core.Services, IEconomyService

### Community 33 - "Claw Mechanics & Physics"
Cohesion: 0.21
Nodes (7): bool, RectTransform, Rect, ScreenOrientation, ClawMachine.UI, SafeArea, Vector2Int

### Community 35 - "Community 35"
Cohesion: 0.33
Nodes (4): float, Collider, ChuteDetector, HashSet

### Community 36 - "Module 36 (packages-lock.json)"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.particlesystem

### Community 37 - "Claw Mechanics & Physics"
Cohesion: 0.24
Nodes (4): T, Dictionary, ClawMachine.Core.Services, ServiceLocator

### Community 38 - "Claw Mechanics & Physics"
Cohesion: 0.25
Nodes (6): float, Rigidbody, Transform, ClawArm, ClawMachine.Gameplay, HingeJoint

### Community 39 - "Community 39"
Cohesion: 0.27
Nodes (8): ChuteDetector, GameObject, int, MachineDefinition, MeshRenderer, PrizeSpawner, ClawMachine.Gameplay, MachineSlot

### Community 40 - "Module 40 (dependencies)"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 41 - "Module 41 (dependencies)"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.animation

### Community 42 - "Community 42"
Cohesion: 0.23
Nodes (11): Color, GameObject, Material, MenuItem, Quaternion, string, Transform, Vector3 (+3 more)

### Community 43 - "Community 43"
Cohesion: 0.31
Nodes (5): Button, GameObject, Text, TimeSpan, OfflineEarningsModalUI

### Community 44 - "Community 44"
Cohesion: 0.29
Nodes (7): Phase 5: Game Feel & Retention Implementation Plan, Task 1: Active Claw Cable Sway (`ClawPendulumSway.cs`), Task 2: Dynamic Cinematic Camera (`ClawCameraController.cs`), Task 3: Chute Guide Ramp & Physical Tray Delivery, Task 4: Retention & Daily Quest Engine (`IRetentionService.cs` + `RetentionService.cs`), Task 5: Unified Progress Modal UI (`ProgressModalUI.cs`), Task 6: Verification & Scene Regeneration

### Community 45 - "Community 45"
Cohesion: 0.33
Nodes (6): 1. Claw Cable Sway (Active Skill Timing), 2. Dynamic Cinematic Camera, 3. Physical Chute Ramp & Tray Delivery, 4. Retention & Progression Loop, 5. Combined Progress Modal UI, Phase 5: Game Feel & Retention Design

### Community 46 - "Claw Mechanics & Physics"
Cohesion: 0.25
Nodes (4): Func, T, ClawMachine.Core.Services, ISaveService

### Community 47 - "Claw Mechanics & Physics"
Cohesion: 0.07
Nodes (25): float, int, Vector2, MachineDefinition, Color, float, GameObject, int (+17 more)

### Community 48 - "Module 48 (float)"
Cohesion: 0.16
Nodes (8): Camera, ClawController, ClawState, float, Quaternion, Transform, Vector3, ClawCameraController

### Community 49 - "Community 49"
Cohesion: 0.29
Nodes (4): Transform, ClawCableVisual, ClawMachine.Gameplay, LineRenderer

### Community 50 - "Community 50"
Cohesion: 0.29
Nodes (6): Demo Presentation Refinement Implementation Plan, Task 1: URP Post-Processing, Lighting & Cabinet Glass Shader, Task 2: Smooth Machine Carousel Transition, Task 3: Claw Cable Visual & Chute Collection Delivery, Task 4: Juicy Win Celebration Modal (3D Rotating Prize Preview), Task 5: Clean Service Decoupling (IEconomyService vs ICollectionService)

### Community 51 - "Community 51"
Cohesion: 0.43
Nodes (6): bool, int, string, ClawMachine.Core.Services, DailyQuestData, MilestoneRewardData

### Community 52 - "Community 52"
Cohesion: 0.29
Nodes (4): PointerEventData, MonoBehaviour, ClawMachine.UI, OfflineCollectPointerClicker

### Community 54 - "Claw Mechanics & Physics"
Cohesion: 0.40
Nodes (3): MenuItem, ClawMachine.Editor, PrefabExporter

### Community 56 - "Module 56 (dependencies)"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.accessibility

### Community 57 - "Module 57 (dependencies)"
Cohesion: 0.29
Nodes (6): dependencies, depth, source, version, dependencies, com.unity.modules.ai

### Community 65 - "Community 65"
Cohesion: 0.40
Nodes (3): MenuItem, ClawMachine.Editor, MaterialStylizer

## Knowledge Gaps
- **377 isolated node(s):** `ClawMachine.Core.Services`, `AudioClip`, `ClawMachine.Core.Services`, `Prize`, `ClawMachine.Core.Services` (+372 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `CollectionManager` connect `Economy & Data Architecture` to `Claw Mechanics & Physics`, `Community 52`, `Claw Mechanics & Physics`?**
  _High betweenness centrality (0.106) - this node is a cross-community bridge._
- **Why does `ClawAudio` connect `Juice & Audio Effects` to `Community 52`?**
  _High betweenness centrality (0.066) - this node is a cross-community bridge._
- **Why does `MachineController` connect `Claw Mechanics & Physics` to `Claw Mechanics & Physics`, `Community 52`?**
  _High betweenness centrality (0.054) - this node is a cross-community bridge._
- **What connects `ClawMachine.Core.Services`, `AudioClip`, `ClawMachine.Core.Services` to the rest of the system?**
  _381 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Economy & Data Architecture` be split into smaller, more focused modules?**
  _Cohesion score 0.07756813417190776 - nodes in this community are weakly interconnected._
- **Should `Arcade Controls & Console` be split into smaller, more focused modules?**
  _Cohesion score 0.07755102040816327 - nodes in this community are weakly interconnected._
- **Should `Claw Mechanics & Physics` be split into smaller, more focused modules?**
  _Cohesion score 0.09782608695652174 - nodes in this community are weakly interconnected._