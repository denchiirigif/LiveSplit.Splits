# LiveSplit.Splits - BS⊿ Tier Colors 改造版

LiveSplit公式の `LiveSplit.Splits` コンポーネントに、**列ごとに「Best Segmentとの差」に応じた段階色（Tier Colors）を設定できる機能**を追加したもの。

元の目的: BS⊿（Segment DeltaのBest Segments比較）列が、vsBSだとほぼ毎回赤くなってしまうのを避け、差の大きさに応じて5段階の色（新記録/+10秒/+20秒/+30秒/それ以上）で表示したい。

## 追加した機能

Layout Settings → Splits タブ → 各列の設定に、以下を追加。

- **Tier colors by delta** チェックボックス（列ごとにON/OFF可能）
- 上限秒数を3つ（初期値 10 / 20 / 30 秒）指定するボックス
- 5段階の色ボタン（新記録 / Tier1以内 / Tier2以内 / Tier3以内 / それ以上）
  - 色ボタンは本体の `SettingsHelper.ColorButtonClick` を使用しているため、他のLiveSplit標準の色設定と同じダイアログ（16進コード入力・スポイト対応）で選べる

対象は **終了済みスプリットの Segment Delta 系カラム**（`SegmentDelta` / `SegmentDeltaorSegmentTime`）。現在区間のライブデルタ表示は対象外（従来通りDeltasの色設定に従う）。

設定は通常のレイアウトファイル(.lsl)に保存される。未改造のLiveSplitでこのレイアウトを開いても、Tier Colors関連の項目は無視されるだけで、壊れることはない（後方互換あり）。

## 変更したファイル

`src/LiveSplit.Splits/` 以下の4ファイルのみ改造。

- `UI/ColumnData.cs` — `UseTierColors` / `TierThresholds` / `TierColors` プロパティ、`GetTierColor()` メソッド、XML保存・読込処理を追加
- `UI/Components/ColumnSettings.cs` — Tier Colors UIのイベント処理を追加
- `UI/Components/ColumnSettings.Designer.cs` — チェックボックス・秒数入力・色ボタンをデザイナーに追加
- `UI/Components/SplitComponent.cs` — `UpdateColumn()` 内、SegmentDelta分岐の色決定処理に `data.GetTierColor(segmentDelta)` を追加

## 別PCで再現する手順

### 1. 事前準備

- Visual Studio（.NET デスクトップ開発ワークロードを入れる）
- Git

### 2. 本体ソースを取得

```bash
git clone --recursive https://github.com/LiveSplit/LiveSplit.git
```

サブモジュールを含めるため `--recursive` は必須。

### 3. このリポジトリの改造を取り込む

クローンした `LiveSplit/components/LiveSplit.Splits` は公式の状態になっているので、これをこのフォーク＆ブランチに差し替える。

```bash
cd LiveSplit/components/LiveSplit.Splits
git remote add myfork https://github.com/denchiirigif/LiveSplit.Splits.git
git fetch myfork
git checkout myfork/bs-tier-colors
```

（もしくは、このリポジトリの `bs-tier-colors` ブランチを別途cloneして、中身を丸ごと `components/LiveSplit.Splits` に上書きしてもよい）

### 4. ビルド

`LiveSplit.sln` をVisual Studioで開き、構成を **Release** にしてソリューションをビルド。

出力先:
```
LiveSplit/artifacts/build/release/
```

### 5. 運用フォルダとして使う

`artifacts/build/release/` を丸ごと別フォルダにコピーして、そこを運用フォルダにする（`artifacts` は再ビルドのたびに作り直されるため、直接運用しない）。

普段使っているカスタムコンポーネント（例: PacePlacementなど）のDLLは、この運用フォルダの `Components/` にも手動でコピーする必要がある。

### 6. 注意点

- **自動更新を絶対に実行しない。** 公式の自動アップデートが走ると、この改造が消えて公式版に置き換わる。
- 元の（未改造の）LiveSplitは別途残しておき、何かあればすぐ戻せるようにしておく。

## 今後さらに改造するとき

```bash
cd LiveSplit/components/LiveSplit.Splits
# ファイルを編集後
git add -A
git commit -m "変更内容の説明"
git push myfork bs-tier-colors
```

## 公式の更新に追従したいとき

```bash
cd LiveSplit/components/LiveSplit.Splits
git fetch origin
git rebase origin/master
# コンフリクトが出たら該当ファイルを手動修正
git push myfork bs-tier-colors --force-with-lease
```
