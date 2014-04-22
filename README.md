KancolleSniffer
===============

[KancolleSniffer]は、艦これの通信プロトコルから得た情報を使ってプレイヤーを支援するツールです。以下の機能があります。

-  入渠、建造、遠征の終了を知らせる自動式タイマー
- 艦娘の耐久、コンディション値、レベル、次のレベルまでの経験値の表示
- 第一艦隊の疲労回復までの時間を知らせる自動式タイマー
  - 疲労アイコン消灯、間宮消灯、隠れ疲労回復の3種類
- 各艦隊の燃料と弾薬の消費レベルの表示(4段階)
- 遂行中の任務の表示

タイマーの終了などの通知は、ウィンドウの点滅、通知領域のバルーン表示、サウンドの再生から必要なものを選択できます。サウンドファイルにはWAV、WMA、MP3などを設定できます。既定では音声ファイルを再生します。

[艦これやるとき便利なやつ]の更新が止まってしまい最近のプロトコルでは動かないので、よく似たものを作ってみたのがKancolleSnifferです。提督の経験値の表示など僕が必要としない機能は実装していません。

[KancolleSniffer]: http://kancollesniffer.sourceforge.jp/
[艦これやるとき便利なやつ]: http://ikbkr.blogspot.jp/p/kancolle.html

## 動作環境

Windows Vista以降です。[.NET Framework 4.5]を使っているのでWindows XPでは動きません。テストはWindows 7でしか行っていません。

[.NET Framework 4.5]: http://download.microsoft.com/download/B/A/4/BA4A7E71-2906-4B2D-A0E1-80CF16844F5F/dotNetFx45_Full_setup.exe

## ライセンス

KancolleSnifferは[GNU GPLv3][1]でライセンスします。ただしGPLv3の第7節に関する追加の許可として、FiddlerCore4.dllは[End User License Agreement for FiddlerCore][2]で、DynamicJson.dllは[Microsoft Public License (Ms-PL)][3]でライセンスされます。音声ファイルは[Open JTalkオンラインデモページ][4]で生成しました。

[1]: http://sourceforge.jp/magazine/07/09/02/130237
[2]: https://sourceforge.jp/projects/kancollesniffer/wiki/FiddlerCoreLicense
[3]: http://dynamicjson.codeplex.com/license
[4]: http://open-jtalk.sp.nitech.ac.jp/index.php

## 注意

KancolleSnifferを起動してから艦これにログインしてください。ログイン中の場合はログインし直してください。ログイン時にしか取得できない情報が多いので、それを取得するまで何も表示しません。

FiddlerCoreを使っているので、Firefoxのプロキシ設定で「システムのプロキシ設定を利用する」以外を指定していると動きません。FiddlerCoreの障害でプロキシの設定がおかしくなり、ブラウザがインターネットに接続できなくなることがあります。もしそうなったらインターネットオプションの「接続」→「LANの設定」→「LANにプロキシ サーバーを使用する」のチェックを外したうえで、レジストリエディタ(regedit)で`HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ProxyServer`を削除してください。

## 変更点

#### バージョン1.6 (2014-04-22)

- ログイン時に流れる情報の取得が必須になります
- ログイン時に建造済みの艦娘がいてもわからないのが直ります
- 制空値を表示します
- 第二～第四艦隊の情報を表示できます
- 3桁のレベルを表示できないのが直ります

#### バージョン1.5 (2014-03-02)

- 次のレベルまでの経験値が5ケタしか表示されないのが直ります
- 小破のときに耐久の背景色がくすんだ黄色に変わります

#### バージョン1.4 (2014-01-12)

- 新規実装以外でも改造した艦娘の名前が不明になるのが直ります
- 各艦隊の燃料と弾薬の消費レベルを表示します
- 建造タイマーの通知を1分前ではなく完了したときにします
- ウィンドウを最前面に表示できます
- 耐久の背景色が文字の部分だけ変わります
- コンディション値が本当に0のときに背景色が赤にならないのが直ります
- 建造タイマーのラベルを工廠から建造に変更します

バージョン1.3以前でログイン時に取得したデータは使えないので、KancolleSnifferを起動してからログインしてデータを取得し直してください。

#### バージョン1.3 (2014-01-05)

- レベルの表示を3列目に移動します
- 3桁の耐久に対応します
- 入渠や間宮による回復が疲労タイマーに反映されます
- 通知領域のバルーンヒントでイベントを通知できます
- サウンドの再生でイベントを通知できます
- 通知方法を設定できます
- 既定のサウンドとして音声ファイルを添付します

#### バージョン1.2 (2013-12-22)

- 中破・大破時に耐久の文字色が変わって読めなくなるのが直ります
- 中破・大破した艦娘を艦隊から外すと耐久の文字色が残るのが直ります
- 疲労タイマーがゼロになったあと増えていくのが直ります
- 疲労タイマーが不必要に再設定されて3分単位に切り上がるのが直ります
- 隠れ疲労が取れるまでのタイマーを表示します

#### バージョン1.1 (2013-12-21)

- アイコンが変わります
- 艦娘のドロップや工廠での解体で艦娘数と装備数が増減します
- 中破・大破したときに耐久の文字の背景色が変わります
- 午前5時を過ぎたら前日のデイリーを消すために任務をリセットします
- 艦娘のコンディション値を表示します
- 疲労状態から回復するまでと間宮点滅の止まるまでのタイマーを表示します

#### バージョン1.0 (2013-12-18)

- 最初のリリース