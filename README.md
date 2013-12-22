KancolleSniffer
===============

[KancolleSniffer]は、艦これの通信プロトコルから得た情報を使ってプレイヤーを支援するツールです。時間の設定を自動で行うタイマーや、艦娘のパラメーターや遂行中の任務などを表示する機能があります。

[艦これやるとき便利なやつ]の更新が止まってしまい最近のプロトコルでは動かないので、よく似たものを作ってみたのがKancolleSnifferです。提督の経験値の表示など僕が必要としない機能は実装していません。

イベントの通知はウィンドウの点滅しかサポートしていませんし、音はWindowsの警告音だけです。通知領域でバルーンを表示するとか、警告音を設定できるようにするとかは、そのうち実装するかもしれません。

[KancolleSniffer]: http://kancollesniffer.sourceforge.jp/
[艦これやるとき便利なやつ]: http://ikbkr.blogspot.jp/p/kancolle.html

## 動作環境

Windows Vista以降です。[.NET Framework 4.5]を使っているのでWindows XPでは動きません。テストはWindows 7でしか行っていません。

[.NET Framework 4.5]: http://download.microsoft.com/download/B/A/4/BA4A7E71-2906-4B2D-A0E1-80CF16844F5F/dotNetFx45_Full_setup.exe

## ライセンス

KancolleSnifferは[GNU GPLv3][1]でライセンスします。ただしGPLv3の第7節に関する追加の許可として、FiddlerCore4.dllは[End User License Agreement for FiddlerCore][2]で、DynamicJson.dllは[Microsoft Public License (Ms-PL)][3]でライセンスされます。

[1]: http://sourceforge.jp/magazine/07/09/02/130237
[2]: https://sourceforge.jp/projects/kancollesniffer/wiki/FiddlerCoreLicense
[3]: http://dynamicjson.codeplex.com/license

## 注意

艦名を取得するために、一回はKancolleSnifferを起動した状態で艦これにログインしてください。終了するときに艦名を保存するので、次からはログイン後に起動しても大丈夫です。取得できない艦名は「不明」になります。

FiddlerCoreを使っているので、Firefoxのプロキシ設定で「システムのプロキシ設定を利用する」以外を指定していると動きません。

FiddlerCoreの障害でプロキシの設定がおかしくなり、ブラウザがインターネットに接続できなくなることがあります。もしそうなったらインターネットオプションの「接続」→「LANの設定」→「LANにプロキシ サーバーを使用する」のチェックを外したうえで、レジストリエディタ(regedit)で`HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ProxyServer`を削除してください。

## 変更点

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
