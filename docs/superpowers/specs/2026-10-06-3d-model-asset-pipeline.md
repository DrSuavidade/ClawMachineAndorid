# 3D Model Asset Pipeline Specification

This document defines how to replace prototype primitive toys with custom 3D models (FBX/OBJ/GLB) created in Blender or purchased from asset stores.

## 1. Model Specifications
- **Scale:** 1 Unity unit = 1 meter. Toys should have bounding box dimensions between `0.25m` and `0.45m`.
- **Polygon Budget:** 300 to 1,500 triangles per prize (optimized for mobile batching and physics simulation).
- **Pivot Point:** Center of mass / volumetric center (crucial for realistic tumble and claw basket nesting).
- **Colliders:**
  - Prefer primitive compound colliders (Sphere, Box, Capsule) or a simplified Convex Mesh Collider (< 32 vertices).
  - Add `Rigidbody` (Mass: 0.35 - 0.90 kg).

## 2. Prefab Assembly
1. Import 3D model into `Assets/Models/Prizes/[MachineName]/`.
2. Create a Prefab in `Assets/Prefabs/Prizes/[MachineName]/`.
3. Add `Prize.cs` component to root.
4. Ensure root or children are on layer `Prize`.

## 3. Data Binding
Open the corresponding `PrizeDefinition` asset in `Assets/Data/Prizes/[MachineName]/`:
- Set `Prefab` field to the new 3D Model Prefab.
- Set `Mass`, `Bounciness`, `Friction`.
- Set `Rarity` (Normal, Rare, Secret).

The spawner, carousel, and 3D RenderTexture Win Modal will immediately display the new 3D model without code modifications.
