# NareisLib

> RimWorld multi-layer rendering framework for complex character customization.

## Overview
NareisLib extends RimWorld's pawn renderer with a configurable multi-layer pipeline, letting mods draw custom textures on top of vanilla or Human Alien Race (HAR) assets without rewriting the core game logic.

## Current Status
Targeted for RimWorld 1.6; the library backports many concepts from 1.4/1.5 but new work should focus on 1.6 builds.

## Feature Highlights
- **Layered rendering** – Define meshes on precise layers such as `BottomOverlay`, `Body`, `Hair`, `Hat`, and more for every facing direction, matching the `TextureRenderLayer` enum.
- **MultiTexDef graphs** – Tie custom assets to vanilla defs, choose whether to keep the original textures, and list all additional layers from XML via `MultiTexDef`.
- **Action-driven behavior** – Use `ActionDef`, `Behavior`, and `ActionManager` to swap textures dynamically per job, posture, or timer, including synchronized updates across linked body parts.
- **Automatic comp injection** – Harmony patches attach `MultiRenderComp` to every pawn so custom render data is always available without manual Def edits.

## Requirements
RimWorld 1.6, Harmony, and Human Alien Race (HAR) with Extended Graphics support (as used by the framework).

## Repository Layout
- `NazunaLib/` – C# source for the library, including rendering components, behavior logic, and Harmony patches.
- `NazunaLib/Properties/` – Assembly metadata and compiler settings.
- `PatchOperation_AddDefaultSubWorker.cs` – Utility patch for XML workflows.

## Getting Started
1. **Clone & build** – Build the `NazunaLib.csproj` against RimWorld 1.6 assemblies and copy the resulting DLL into your mod's `Assemblies/` folder.
2. **Author MultiTexDef XML** – For each pawn body part (BodyDef, HeadDef, HairDef, Apparel, HandTypeDef), create `MultiTexDef` entries that map the target def to your texture folders and layered keys.
3. **Configure behaviors** – Define `ActionDef` assets that enumerate `Behavior` entries per job. Use posture-specific folders and randomization flags to control when textures change, or link multiple `ActionManager` instances for synchronized parts.
4. **Test in game** – Launch RimWorld with HAR and NareisLib, spawn the pawn(s), and verify that layers, overrides, and behaviors update as expected.

### Minimal MultiTexDef Example
```xml
<MultiTexDef>
  <defName>Example_MultiTex</defName>
  <originalDefClass>Verse.HairDef</originalDefClass>
  <originalDef>Example_Hair</originalDef>
  <renderOriginTex>false</renderOriginTex>
  <path>Textures/ExampleHair</path>
  <levels>
    <li>
      <textureLevelsName>Front</textureLevelsName>
      <layer>Hair</layer>
      <textures>
        <li>front</li>
      </textures>
    </li>
  </levels>
</MultiTexDef>
```

## Tips & Best Practices
- Prefer descriptive `textureLevelsName` values; they become unique identifiers during runtime linking.
- Keep texture atlases lightweight—`MultiRenderComp` caches per-facing batches, so smaller assets reduce recache cost.
- When overriding vanilla nodes, verify tag/debug labels via the in-game Action Manager gizmo before distributing your mod.

## Compatibility Notes
Other mods that deeply patch `PawnRenderer` or HAR's Extended Graphics may need manual integration.

## Contributing
Submit pull requests with clear descriptions and reference in-game results when altering rendering order or behavior logic.

## License
This project is distributed under the MIT License. See [`LICENSE`](LICENSE) for details.