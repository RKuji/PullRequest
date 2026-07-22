# Visual Studio 拡張 PullRequest
## 概要
Visual Studio からプルリクエストページを開くとき、マージ元が指定された状態でリンクを開く拡張機能です。

## インストール方法
1. [最新リリース](https://github.com/RKuji/PullRequest/releases/latest) から、```.vsix``` ファイルをダウンロードする。
   - ```PullRequest.vsix``` (Visual Studio 最新版)
   - ```PullRequest_VS2019.vsix``` (Visual Studio 2019 版)
     
2. ダウンロードしたインストーラをクリックすると、インストールが行われる。

## 基本動作
メニューバーから、**Git - プルリクエストページを開く** を選択すると、ブラウザでプルリクエストページが開かれる。

## 仕様
- 実行すると、次の URL が既定のブラウザで開かれる。\
  ```https://github.com/{オーナー名・リポジトリ名}/compare/{マージ元ブランチ}...{チェックアウト中のブランチ}?quick_pull=1```
- マージ元ブランチは、命名規則に従い、以下のロジックで決定している。
   - 現在のブランチ名に ```-``` が含まれる場合\
     末尾の ```-``` 以降を削除する。
	 ```
	 source: feature-209800-180145-rkuji
	 target: feature-209800-180145

	 source: feature-209800-180145
	 target: feature-209800
	 ```
   - 現在のブランチ名に ```-``` が含まれない場合\
     現在のブランチ名と同名とする。\
     実行すると、```Choose different branches...``` というメッセージが表示され、プルリクエストが作成できない状態のページに遷移する。
     （適切な作業ブランチでない可能性が高いため、機能の対象としていない。）
	 ```
	 source: main
	 target: main

	 source: release
	 target: release
	 ```

## アンインストール方法
1. Visual Studio を起動する。
2. メニューバーから、**拡張機能 - 拡張機能の管理** を選択し、拡張機能マネージャーを起動する。
3. **インストール済み** のタブから、PullRequest の項目を選び、アンインストールを実行する。
