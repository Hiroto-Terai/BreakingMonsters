# リリース手順（2026年12月 / 日本のみ / iOS・Android）

目標: 12月上旬に両ストアへ申請し、12月中旬に公開する。年末（12/23〜12/27ごろ）は審査が混むので避ける。

## いますぐ（本人がやること）

| やること | 目安 | メモ |
|---|---|---|
| Google Play デベロッパー登録（個人） | 本人確認に数日 | 25ドル。登録したアカウントで「12人×14日のクローズドテスト」が必須になる |
| Apple Developer Program に再登録 | 1〜2日 | 年額99ドル。以前アプリを出した Apple ID（hiro08051610）で登録すると、既存のアプリ枠と `com.hiro08051610.BreakingMonsters` をそのまま使える |
| Xcode 26 をインストール | 1時間〜 | App Store から。2026年4月28日以降、Xcode 26 / iOS 26 SDK でビルドしたものしか提出できない（今入っているのは 15.3） |
| AdMob に Android アプリを追加 | 当日 | アプリID と、バナー・インタースティシャル・リワードの広告ユニットIDを作る |
| AdMob の iOS アプリにリワード広告ユニットを追加 | 当日 | 「ふっかつ」「コイン2倍」で使う |
| テスター12人を集める | 〜テスト開始まで | Gmail アドレスを集める。全員がテスト期間の14日間ずっと参加している必要がある |

AdMob のIDができたら `Assets/Scripts/AdUnits.cs` と、Unity の `Assets > Google Mobile Ads > Settings` のアプリIDを差し替える（今は Android と iOS のリワードが Google のテスト用ID）。

## Android（Google Play）

1. 署名鍵を作る: Unity の `Project Settings > Player > Android > Publishing Settings > Keystore Manager` で新規作成。パスワードと `.keystore` ファイルはなくすと二度と同じアプリを更新できないので、リポジトリの外で保管する
2. AAB を作る: `File > Build Profiles > Android > Build`（AAB 出力は設定済み）
3. Play Console でアプリを作成 → `テスト > クローズド テスト` にトラックを作り、AAB をアップロード
4. テスターをメールリスト（または Google グループ）で追加し、参加用リンクを送る。各自がリンクから参加してインストールする
5. 12人以上が14日間続けて参加したら、ダッシュボードから「製品版へのアクセスを申請」（審査は最大7日ほど）
6. 製品版トラックに AAB を出して審査へ

逆算: 12月上旬に申請するには、クローズドテストを **遅くとも11月中旬** に始める。テスト中も新しい AAB を上げて更新できるので、早く始めるほど安全。

## iOS（App Store）

1. Xcode 26 を入れたら、Unity で iOS ビルド → 出てきた Xcode プロジェクトを開いて Signing の Team を設定
2. `Product > Archive` → App Store Connect にアップロード
3. TestFlight で自分とテスターに配る（Android のテスターと同じ人に頼むと感想が集めやすい）
4. App Store Connect で新しいバージョン 2.0.0 を作り、スクリーンショット・説明文・プライバシーの回答を入れて審査へ

## ストアに出すもの

- 説明文・キーワード: `docs/release/store-listing.md`
- プライバシーポリシー: 今の https://hiroto-terai.github.io/BreakingMonsters/ は AdMob の記載がないので、`docs/release/privacy-policy.md` の内容に差し替える
- スクリーンショット
  - App Store: iPhone 6.9インチ（1320×2868 など）を最低1枚、最大10枚
  - Google Play: スマホ用を2〜8枚、フィーチャーグラフィック 1024×500、アイコン 512×512
- 年齢レーティング: 両ストアのアンケートに答える（暴力表現は「軽いファンタジーの戦い」、広告あり、課金なし）
- データの扱い（Google Play の「データ セーフティ」/ App Store の「App のプライバシー」）: 自分では何も集めていないが、AdMob が広告ID・おおよその位置（IPアドレスから）・利用状況・クラッシュ情報を集める。Google の公式ガイドに沿って回答する
  - https://developers.google.com/admob/android/privacy/play-data-disclosure
  - https://developers.google.com/admob/ios/privacy/data-disclosure

## アプリ内で設定済みのもの

- バージョン 2.0.0（Android の versionCode 1 / iOS の build 1）
- Android: パッケージ名 `com.hiro08051610.BreakingMonsters`、IL2CPP、ARM64 + ARMv7、target API 36、AAB 出力、縦長画面で黒帯なし
- 広告: Google Mobile Ads 11.5.0、リワード広告（全滅時の「ふっかつ」1ステージ1回・結果画面の「コイン2倍」）
- Android の AAB と APK のビルドが通ることを確認済み（Pixel 10a で起動を確認）
