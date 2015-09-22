# CM3D2.BinbolusVR.Plugin
　メイドさんのエディット(level5)、夜伽シーン(level14)、ダンスシーン(level4,20)において、画面を分割して[両眼視差立体視](https://ja.wikipedia.org/wiki/%E7%AB%8B%E4%BD%93%E8%A6%96)を可能にする UnityInjector プラグインです。公式から [Oculus VR 対応パッチ](http://kissdiary.blog11.fc2.com/blog-entry-571.html)が公開されていますが、Oculus なんて立派なモン持ってねーYO！　貧乏人ナメんなYO！　貧乏＋Oculus VR＝ビンボラスVR ってことで。

　下のサンプル画像で立体視できない人には、導入しても裸眼での立体視モードは意味がありません。がんばって練習しよう。裸眼での立体視が苦手だったり、どっぷり没入したい人向けに Head Mount Display でも使えるようになりました (New@0.0.1.5)
![裸眼での交差法立体視の画像サンプル](sample_cross1.png) 

## 機能
 * 右目と左目ごとに視差を考慮した画面を表示して両眼視差立体視を可能にします
 * 裸眼での立体視では、交差法または平行法の切り替えが可能です
 * Head Mount Display などで使えるように SideBySide または TopAndBottom 表示方式に対応しています (New@0.0.1.5) #1

## 使い方
 0. 下準備
   0. [UnityInjector](http://www.hongfire.com/forum/showthread.php/444567-UnityInjector-Plugin-Powered-Unity-Code-Injector)が必要です。[neguse11 さまのネットワークインストーラーもどき](https://github.com/neguse11/cm3d2_plugins_okiba)を利用しての導入を強く推奨。ちょー楽チン。
   1. `(ゲームのインストールパス)/UnityInjector`以下に`CM3D2.BinbolusVR.Plugin.dll`をコピーします。
 1. CM3D2を起動してゲームをプレイ開始、以下のシーンで立体視モードに切り替えられます。
   * メイドさんのエディット
   * 夜伽シーン
   * ダンス/ドキドキ☆Fallin' Love (New@0.0.1.4)
   * ダンス/entracne to you (New@0.0.1.4)
 2. `K` キーを押すと、立体視モードを切り替えます。
   * [Head Mount Display で使うには？](HeadMountDisplay.md)
 3. `L` キーを押すと、視差モードを切り替えます。または Up-And-Bottom 表示モードで、左右どちらの画像を画面の上下に表示するか切り替えます。見やすい方でお使いください。
   * 交差法 … 寄り目にして左右の目でそれぞれ反対の画像を見る方法
   * 平行法 … 遠くを見るようにしながら左右の目でそれぞれの画像を見る方法

### 設定ファイル
ゲームの**起動時**に、キー設定などの設定値を `UnityInjector/Config/BinbolusVR.ini` から読み込みます。ゲームを起動した後に設定ファイルを変更しても反映されません。

#### `SceneEnable` (New@0.0.1.6)
　立体視モードに切り替え可能なシーンの level を空白/カンマ区切りで列挙します。`ALL` とすると、(実用性はともかくとして)全てのシーンが対象になります。デフォルトでは、エディット・夜伽・ダンスシーンで有効(`SceneEnable=5,14,4,20`)になっています。 

#### `TogglePower`
　立体視モードの切り替えに利用するキーを指定します。デフォルトで `K` キーが割り当てられていますが、他のプラグインと衝突する場合などに変更してください。

#### `ToggleMode`
　視差モードの交差法/平行法の切り替えに利用するキーを指定します。デフォルトで `L` キーが割り当てられていますが、他のプラグインと衝突する場合などに変更してください。

#### `ParallaxScale`
　両目の距離を指定するスケール値です。大きくすると画像のズレ(視差)が大きくなり立体感が増しますが、あまり大きくしすぎると結像しにくくなります。

#### `Powers`
　使用する立体視モードを空白/カンマ区切りで列挙します。デフォルト設定は `Powers=NAKED_EYES` のみ有効になっています。  
　手持ちの Head Mount Display でご利用の際には、`SIDEBYSIDE` または `TOPANDBOTTOM` を追加してください。たとえば `Powers=NAKED_EYES,SIDEBYSIDE` と設定した場合、`K` キーを押すたびに、オフ→NAKED_EYES モード→SIDEBYSIDE モード→オフ→...と切り替わります。もし、Head Mount Display を SIDEBYSIDE モードでしか使わないのであれば、`Powers=SIDEBYSIDE` と設定すると余計なモードに切り替わりません。

 * `NAKED_EYES` … 裸眼による立体視モードを有効にします。
 * `SIDEBYSIDE` … 横方向に2倍に圧縮された画像を左右に並べて表示する Side-By-Side 表示モードを有効にします。Head Mount Display などで利用できます。
 * `TOPANDBOTTOM` … 縦方向に2倍に圧縮された画像を上下に並べて表示する Up-And-Bottom 表示モードを有効にします。Head Mount Display などで利用できます。

#### `DefaultPower` (New@0.0.1.6)
 　`SceneEnable` で指定されたシーンに切り替わった際に、`DefaultPower` で指定された立体視モードに自動的に切り替わるようにします。Head Mount Display を利用されている場合など、シーン切り替えの度に立体視モードを手動で切り替える必要がなくなります。デフォルトの設定値は `OFF` です。

 * `OFF`
 * `NAKED_EYES`
 * `SIDEBYSIDE`
 * `TOPANDBOTTOM`

#### `DefaultMode` (New@0.0.1.6)
立体視モードを有効にした際の標準の視差モードを指定できます。

 * `RL` … 交差法
 * `LR` … 平行法

#### `DebugMode` (New@0.0.1.6)
動作確認用の情報表示の出力を制御します。

 * 0 = 画面上のキャプションを表示しません。キー操作も覚えたし、視差スケールの調整もこれでバッチリ、という方に。
 * 1 = 画面上に立体視モードやキー操作の情報を表示します。
 * 2 = 視差スケール調整モード(後述)を有効にします。

### 視差スケール調整モード (New@0.0.1.6)
　`DebugMode=2` と設定した場合、視差スケール調整モードが有効になります。ここで、立体視モードを切り替えると、`ParallaxScale` 値を `PageUp`キー/`PageDown`キーで増減しつつ、視差スケールの調整に利用できます。ただし、変更後の値は設定ファイルに書き戻されませんので、**ゲームを再起動すると元に戻ってしまいます。**画面上に表示された値を控えておいて、設定ファイルに書き戻してください。  
　最適な視差スケールを決定するための一時的な機能ですので操作キーは変更できません。他プラグインなどとキーバインドが衝突しても泣かない。


## 自前でコンパイルする
[neguse11 さまのネットワークインストーラーもどき](https://github.com/neguse11/cm3d2_plugins_okiba)に居候しています。`cm3d2_plugins_okiba-master`フォルダの下に`CM3D2.BinbolusVR.Plugin`ディレクトリを置いて`(ゲームのインストールパス)/cm3d2_plugins_okiba-master/CM3D2.BinbolusVR.Plugin/src/compile.bat`を実行してください。追加で ExIni ライブラリを利用しています。

## 動作確認
* [neguse11/SkillCommandShortCut](/neguse11/cm3d2_plugins_okiba) プラグイン併用可
* [neguse11/MaidVoicePitch](/neguse11/cm3d2_plugins_okiba) プラグイン併用可
* [k8PzhOFo0/CM3D2CameraUtility.Plugin](/k8PzhOFo0/CM3D2CameraUtility.Plugin) 併用可

## 仕様/未実装/今後の野望など
* ~~天頂付近などカメラアングルが±180付近では視差が安定しません。補正方法が良くわからない。~~ 0.0.1.3 で修正
* ~~体勢に依って、これまた視差画像がうまく見えないことがあります。~~ 0.0.1.3 で修正
* 夜伽前のプレイルーム選択画面から効きますが仕様です。

## いろいろ
* カメラを追加してレンダ出力を得るコードは、[k8PzhOFo0氏のFaceCamera.Plugin](https://github.com/k8PzhOFo0/CM3D2FaceCamera)を大変参考にさせて頂きました。ありがとうございます。
* .iniファイルから設定値を読むコードは、[neguse11氏のLogWindow](https://github.com/neguse11/cm3d2_plugins_okiba/tree/develop/LogWindow)を大変参考にさせて頂きました。ありがとうございます。
* [Amazon.co.jp で Head Mount Display の商品一覧](http://www.amazon.co.jp/gp/search/ref=as_li_ss_tl?ie=UTF8&camp=247&creative=7399&field-keywords=%E3%83%98%E3%83%83%E3%83%89%E3%83%9E%E3%82%A6%E3%83%B3%E3%83%88%E3%83%87%E3%82%A3%E3%82%B9%E3%83%97%E3%83%AC%E3%82%A4&index=blended&linkCode=ur2&tag=openmagicvox-22)

## 連絡先など
*  [@pirolix twitter](https://twitter.com/pirolix)
*  [カスタムメイド3D したらば談話室](http://jbbs.shitaraba.net/game/55179/)の改造スレッドをちょくちょく見ています。
