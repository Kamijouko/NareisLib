# NareisLib

> RimWorld のポーン向けに複雑なカスタマイズを実現する多層レンダリングフレームワーク。

## 概要
NareisLib は RimWorld のポーン描画を拡張し、カスタムテクスチャをバニラおよび Human Alien Race (HAR) 資産に重ね合わせられる多層レンダリング基盤を提供します。ゲーム本体のロジックを改造する必要はありません。

## 現在の対応バージョン
最新の開発対象は RimWorld 1.6 です。1.4/1.5 由来の概念も利用できますが、新規作業は 1.6 を前提にしてください。

## 主な機能
- **多層レンダリング** —— `BottomOverlay`、`Body`、`Hair`、`Hat` など十数種類の描画レイヤーを `TextureRenderLayer` 列挙体に基づき制御し、各向きに最適化されたメッシュを定義できます。
- **MultiTexDef レイヤー構成** —— `MultiTexDef` を使用してバニラ Def とカスタム資産を関連付け、元テクスチャを残すか置き換えるかを選択し、XML から追加レイヤーを列挙できます。
- **アクション連動テクスチャ** —— `ActionDef`・`Behavior`・`ActionManager` を組み合わせ、作業内容や姿勢、タイマーに応じたテクスチャ切り替えと部位間の同期を構築します。
- **自動コンポーネント付与** —— すべてのポーンに `MultiRenderComp` を自動追加する Harmony パッチにより、Def を個別に編集せずに拡張データを扱えます。

## 必要条件
RimWorld 1.6、Harmony、そして Extended Graphics 対応の Human Alien Race (HAR)。

## リポジトリ構成
- `NazunaLib/` —— レンダリングコンポーネント、挙動ロジック、Harmony パッチを含む C# ソースコード。
- `NazunaLib/Properties/` —— アセンブリメタデータとコンパイラ設定。
- `PatchOperation_AddDefaultSubWorker.cs` —— XML ワークフロー向けのユーティリティパッチ。

## 使い方ガイド
1. **クローンしてビルド** —— RimWorld 1.6 のアセンブリを参照して `NazunaLib.csproj` をビルドし、生成された DLL を Mod の `Assemblies/` フォルダーへコピーします。
2. **MultiTexDef XML を作成** —— 各ポーン部位（BodyDef、HeadDef、HairDef、Apparel、HandTypeDef）に対して `MultiTexDef` を作成し、対象 Def をテクスチャフォルダーとレイヤーキーに関連付けます。
3. **挙動を設定** —— `ActionDef` にジョブ別の `Behavior` を列挙し、姿勢別フォルダーやランダム化フラグで切り替え条件を制御します。複数の `ActionManager` を連携させて部位間の同期も可能です。
4. **ゲーム内テスト** —— HAR と NareisLib を読み込んだ状態で RimWorld を起動し、対象ポーンを生成してレイヤー、上書き、挙動の更新を確認します。

### 最小の MultiTexDef 例
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

## ヒントとベストプラクティス
- `textureLevelsName` にはわかりやすい名前を付けましょう。実行時のリンクで一意な識別子として使用されます。
- テクスチャアトラスは軽量に保ってください。`MultiRenderComp` は向きごとにバッチをキャッシュするため、小さなアセットほど再キャッシュのコストが下がります。
- バニラのノードを上書きする際は、リリース前にゲーム内の Action Manager ギズモでタグやデバッグラベルを確認してください。

## 互換性
`PawnRenderer` や HAR Extended Graphics を大幅に改造する他 Mod とは個別の調整が必要になる場合があります。

## コントリビュート
レンダリング順序や挙動ロジックを変更する際は、Pull Request にゲーム内での確認結果を添えてください。

## ライセンス
本プロジェクトは MIT License の下で公開されています。詳細は [`LICENSE`](LICENSE) を参照してください。