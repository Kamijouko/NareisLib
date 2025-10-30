# NareisLib

> 面向 RimWorld 的多层渲染框架，支持复杂的小人自定义效果。

## 概述
NareisLib 扩展了 RimWorld 的小人渲染流程，提供可配置的多层渲染框架，让模组作者能够在不修改核心逻辑的情况下为原版或 Human Alien Race (HAR) 族类叠加自定义贴图。

## 当前状态
当前主要支持 RimWorld 1.6。虽然项目沿用了 1.4/1.5 时代的诸多概念，但新的内容建议以 1.6 版本为标准。

## 功能亮点
- **多层渲染** —— 精确控制 `BottomOverlay`、`Body`、`Hair`、`Hat` 等 `TextureRenderLayer` 枚举中的图层，并为不同朝向定义对应的网格。
- **MultiTexDef 图层图** —— 通过 `MultiTexDef` 将自定义资源与原版 Def 绑定，可按需保留原贴图并在 XML 中列出所有附加图层。
- **行为驱动贴图** —— 利用 `ActionDef`、`Behavior` 与 `ActionManager`，可根据工作、姿势和定时器动态切换贴图，并支持多部位联动。
- **自动挂载渲染组件** —— Harmony 补丁会为所有小人自动挂载 `MultiRenderComp`，无需手动修改 Def 也能读取自定义渲染数据。

## 依赖项
需要 RimWorld 1.6、Harmony，以及启用了 Extended Graphics 的 Human Alien Race (HAR)。

## 仓库结构
```
NazunaLib.sln              Rider/Visual Studio 使用的解决方案文件
NazunaLib/
├── Core/                  行为系统、状态管理与公共工具方法
├── PawnRendering/         连接原版与 HAR 的渲染节点工作单元
├── Rendering/             多层渲染核心、Def 定义、运行时缓存与网格工具
│   └── Mesh/              网格池、绘制数据与共享几何体
├── Settings/              游戏内调试界面与模组配置入口
├── TextureLevels/         基于 XML 的图层定义与转换辅助
├── Properties/            程序集元数据与编译设置
└── NazunaLib.csproj       面向 RimWorld 1.6 的项目文件
```

## 快速上手
1. **克隆并编译** —— 参照 RimWorld 1.6 的程序集编译 `NazunaLib.csproj`（或直接打开 `NazunaLib.sln`），将生成的 DLL 复制到模组的 `Assemblies/` 目录。
2. **编写 MultiTexDef XML** —— 针对每个小人部位（BodyDef、HeadDef、HairDef、Apparel、HandTypeDef）创建 `MultiTexDef`，将目标 Def 映射到贴图文件夹和图层键值。
3. **配置行为系统** —— 定义 `ActionDef`，为不同的工作列出对应的 `Behavior`。结合姿势分类文件夹与随机化标记控制贴图切换，并可通过多个 `ActionManager` 实现部位同步。
4. **游戏内测试** —— 在加载 HAR 与 NareisLib 的情况下启动 RimWorld，生成目标小人，确认图层、覆盖与行为均按预期更新。

### 最简 MultiTexDef 示例
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

## 提示与实践
- 使用具描述性的 `textureLevelsName`，这些值会在运行时作为唯一标识使用。
- 保持贴图集尽量轻量：`MultiRenderComp` 会按朝向缓存批次，素材越小重建缓存的成本越低。
- 覆盖原版节点时，请在游戏内使用 Action Manager 小组件确认标签和调试信息后再发布模组。

## 兼容性说明
若其他模组大量改写 `PawnRenderer` 或 HAR 的 Extended Graphics，可能需要额外适配。

## 参与贡献
提交 Pull Request 时请写明改动内容，若涉及渲染顺序或行为逻辑，请提供游戏内的验证结果。

## 许可
本项目以 MIT License 发布，详情见 [`LICENSE`](LICENSE)。
