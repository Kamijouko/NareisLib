# NareisLib

> RimWorld multi-layer rendering framework for complex pawn customization.

## Overview
NareisLib extends RimWorld's pawn renderer with a configurable multi-layer pipeline. Mods can inject custom textures on top of
vanilla assets or Human Alien Race (HAR) graphics without rewriting the game's rendering logic.

## Current Status
Actively targeting RimWorld 1.6. Many concepts originated in 1.4/1.5 builds, but new development should focus on 1.6.

## Feature Highlights
- **Layered rendering** – Define meshes for precise stages such as `BottomOverlay`, `Body`, `Hair`, `Hat`, and other
  `TextureRenderLayer` enum values for every facing direction.
- **MultiTexDef graphs** – Connect custom assets to vanilla defs, choose whether to retain the original textures, and list all
  additional layers directly in XML.
- **Action-driven behavior** – Use `ActionDef`, `Behavior`, and `ActionManager` to swap textures per job, posture, or timer,
  including synchronized updates across linked body parts.
- **Automatic comp injection** – Harmony patches attach `MultiRenderComp` to every pawn so custom render data is always
  available without manual Def edits.

## Requirements
RimWorld 1.6, Harmony, and Human Alien Race (HAR) with Extended Graphics support.

## Repository Layout
```
NazunaLib.sln              Solution file configured for Rider/Visual Studio builds
NazunaLib/
├── Core/                  Action system, behavior state, and shared utilities
├── PawnRendering/         Render node workers that bridge vanilla & HAR pipelines
├── Rendering/             Multi-layer renderer, defs, caches, and mesh helpers
│   └── Mesh/              Mesh pooling, draw data, and shared geometry
├── Settings/              In-game debug page and mod configuration glue
├── TextureLevels/         XML-driven layer definitions and conversion helpers
├── Properties/            Assembly metadata and compiler settings
└── NazunaLib.csproj       Project file targeting RimWorld 1.6 assemblies
```

## Getting Started
1. **Clone & build** – Build `NazunaLib.csproj` (or open `NazunaLib.sln`) against RimWorld 1.6 assemblies and copy the resulting
   DLL into your mod's `Assemblies/` folder.
2. **Author MultiTexDef XML** – For each pawn body part (BodyDef, HeadDef, HairDef, Apparel, HandTypeDef), create `MultiTexDef`
   entries that map the target def to your texture folders and layered keys.
3. **Configure behaviors** – Define `ActionDef` assets that enumerate `Behavior` entries per job. Use posture-specific folders and
   randomization flags to control when textures change, or link multiple `ActionManager` instances for synchronized parts.
4. **Test in game** – Launch RimWorld with HAR and NareisLib, spawn the pawn(s), and verify that layers, overrides, and behaviors
   update as expected.

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
