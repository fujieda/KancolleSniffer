KancolleSniffer
===============

[KancolleSniffer]は、艦これの通信プロトコルから得た情報を使ってプレイヤーを支援するツールです。時間の設定を自動で行うタイマー機能があります。次のレベルまでの経験値やコンディション値などの、通常は見えないパラメーターを表示します。疲労回復までの時間を知らせるタイマーもあります。

タイマーの終了などの通知は、ウィンドウの点滅、通知領域のバルーン表示、サウンドの再生から、必要なものを選択できます。サウンドファイルにはWAV、WMA、MP3などを設定できます。既定では音声ファイルを再生します。

[艦これやるとき便利なやつ]の更新が止まってしまい最近のプロトコルでは動かないので、よく似たものを作ってみたのがKancolleSnifferです。提督の経験値の表示など僕が必要としない機能は実装していません。

[KancolleSniffer]: http://kancollesniffer.sourceforge.jp/
[艦これやるとき便利なやつ]: http://ikbkr.blogspot.jp/p/kancolle.html

## 動作環境

Windows Vista以降です。[.NET Framework 4.5]を使っているのでWindows XPでは動きません。テストはWindows 7でしか行っていません。

[.NET Framework 4.5]: http://download.microsoft.com/download/B/A/4/BA4A7E71-2906-4B2D-A0E1-80CF16844F5F/dotNetFx45_Full_setup.exe

## ライセンス

KancolleSnifferは[GNU GPLv3][1]でライセンスします。ただしGPLv3の第7節に関する追加の許可として、FiddlerCore4.dllは[End User License Agreement for FiddlerCore][2]で、DynamicJson.dllは[Microsoft Public License (Ms-PL)][3]でライセンスされます。

音声ファイルは[Open JTalkオンラインデモページ](http://open-jtalk.sp.nitech.ac.jp/index.php)で生成しました。

[1]: http://sourceforge.jp/magazine/07/09/02/130237
[2]: https://sourceforge.jp/projects/kancollesniffer/wiki/FiddlerCoreLicense
[3]: http://dynamicjson.codeplex.com/license

## 注意

艦名を取得するために、一回はKancolleSnifferを起動した状態で艦これにログインしてください。終了するときに艦名を保存するので、次からはログイン後に起動しても大丈夫です。取得できない艦名は「不明」になります。改造した艦娘の改造後の名前が「不明」になるので、KancolleSnifferを起動したままログインし直してください。

FiddlerCoreを使っているので、Firefoxのプロキシ設定で「システムのプロキシ設定を利用する」以外を指定していると動きません。

FiddlerCoreの障害でプロキシの設定がおかしくなり、ブラウザがインターネットに接続できなくなることがあります。もしそうなったらインターネットオプションの「接続」→「LANの設定」→「LANにプロキシ サーバーを使用する」のチェックを外したうえで、レジストリエディタ(regedit)で`HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ProxyServer`を削除してください。

## 変更点

#### バージョン1.3
- レベルの表示を3列目に移動します
- 3桁の耐久に対応します
- 入渠や間宮による回復が疲労タイマーに反映されます
- 通知領域のバルーンヒントでイベントを通知できます
- サウンドの再生でイベントを通知できます
- 通知方法を設定できます
- 既定のサウンドとして音声ファイルを添付します

#### バージョン1.2

- 中破・大破時に耐久の文字色が変わって読めなくなるのが直ります
- 中破・大破した艦娘を艦隊から外すと耐久の文字色が残るのが直ります
- 疲労タイマーがゼロになったあと増えていくのが直ります
- 疲労タイマーが不必要に再設定されて3分単位に切り上がるのが直ります
- 隠れ疲労が取れるまでのタイマーを表示します

#### バージョン1.1

- アイコンが変わります
- 艦娘のドロップや工廠での解体で艦娘数と装備数が増減します
- 中破・大破したときに耐久の文字の背景色が変わります
- 午前5時を過ぎたら前日のデイリーを消すために任務をリセットします
- 艦娘のコンディション値を表示します
- 疲労状態から回復するまでと間宮点滅の止まるまでのタイマーを表示します
