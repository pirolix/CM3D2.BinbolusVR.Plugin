# CM3D2.BinbolusVR.Plugin
メイドさんエディット画面(level5)、夜伽画面(level14)において、画面を分割して[両眼視差立体視](https://ja.wikipedia.org/wiki/%E7%AB%8B%E4%BD%93%E8%A6%96)を可能にします。  
公式から Oculus VR 対応パッチが公開されていますが、そんな立派なモン持ってねーYO！　貧乏人ナメんなYO！　貧乏＋Oculus＝ビンボラスってことで。

![交差法の画像サンプル](https://raw.githubusercontent.com/pirolix/CM3D2.BinbolusVR.Plugin/master/sample_cross1.png) 

## 自前でコンパイル
[neguse11 さまのネットワークインストーラーもどき](https://github.com/neguse11/cm3d2_plugins_okiba)に居候しています。
`cm3d2_plugins_okiba-master`フォルダの下に`CM3D2.BinbolusVR.Plugin`ディレクトリを置いて`(ゲームのインストールパス)/cm3d2_plugins_okiba-master/CM3D2.BinbolusVR.Plugin/src/compile.bat`を実行してください。追加で ExIni ライブラリを利用しています。

## 使い方
0. 下準備
  0. [UnityInjector](http://www.hongfire.com/forum/showthread.php/444567-UnityInjector-Plugin-Powered-Unity-Code-Injector)が必要です。[neguse11 さまのネットワークインストーラーもどき](https://github.com/neguse11/cm3d2_plugins_okiba)を利用しての導入を強く推奨。ちょー楽チン。
  1. `(ゲームのインストールパス)/UnityInjector`以下に`CM3D2.BinbolusVR.Plugin.dll`をコピーします
1. CM3D2を起動してゲームをプレイ開始、メイド管理メニュー→エディットメニューと進んで、メイドさんのエディット画面を開くか、または夜伽プレイを始めます。
2. `K`キーを押すと、立体視モードのオン/オフを切り替えます。
3. `L`キーを押すと、立体視モードをを切り替えます。やりやすい方でお使いください。
  * 交差法 … 寄り目にして左右の目でそれぞれ反対の画像を見る方法
  * 平行法 … 遠くを見るようにしながら左右の目でそれぞれの画像を見る方法

### 設定ファイル
キー設定などを `UnityInjector/Config/BinbolusVR.ini` から読み込みます。 

* `TogglePower` … 立体視モードのオン/オフを切り替えに利用するキーを指定します。デフォルトで`K`キーが割り当てられていますが、他のプラグインと衝突する場合などに変更してください。
* `ToggleMode` … 立体視モードの交差法/平行法の切り替えに利用するキーを指定します。デフォルトで`L`キーが割り当てられていますが、他のプラグインと衝突する場合などに変更してください。
* `ParallaxScale` … 両目の距離を指定するスケールです。大きくすると画像のズレ(視差)が大きくなり、立体感が増すと思うんですが(自身なし)、あまり大きくしすぎると両目で結像しにくくなります。

## 仕様/未実装/今後の野望など
* 天頂付近などカメラアングルが±180付近では視差が安定しません。補正方法が良くわからない。
* 体勢に依って、これまた視差画像がうまく見えないことがあります。
* 夜伽前のプレイルーム選択画面から効きますが仕様です。

## いろいろ
* カメラを追加してレンダ出力を得るコードは、[k8PzhOFo0氏のFaceCamera.Plugin](https://github.com/k8PzhOFo0/CM3D2FaceCamera)を大変参考にさせて頂きました。ありがとうございます。
* .iniファイルから設定値を読むコードは、[neguse11氏のLogWindow](https://github.com/neguse11/cm3d2_plugins_okiba/tree/develop/LogWindow)を大変参考にさせて頂きました。ありがとうございます。

## 連絡先など
*  [@pirolix twitter](https://twitter.com/pirolix)
*  [カスタムメイド3D したらば談話室](http://jbbs.shitaraba.net/game/55179/)の改造スレッドをちょくちょく見ています。
