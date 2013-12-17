KancolleSniffer
===============

[KancolleSniffer]は、艦これの通信プロトコルから得た情報を使ってプレイヤーを支援するツールです。時間の設定を自動で行う遠征や入渠のタイマーや、艦娘の次のレベルまでの経験値や遂行中の任務などを表示する機能があります。

[艦これやるとき便利なやつ]の更新が止まってしまい最近のプロトコルでは動かないので、よく似たものを作ってみたのがKancolleSnifferです。疲労に関する機能や提督の経験値の表示など、僕が必要としない機能は実装していません。

イベントの通知はウィンドウの点滅しかサポートしていませんし、音はWindowsの警告音だけです。通知領域でバルーンを表示するとか、警告音を設定できるようにするとかは、そのうち実装するかもしれません。

[KancolleSniffer]: http://kancollesniffer.sourceforge.jp/
[艦これやるとき便利なやつ]: http://ikbkr.blogspot.jp/p/kancolle.html

## 動作環境

Windows Vista以降です。.NET Framework 4.5を使っているのでWindows XPでは動きません。テストはWindows 7でしか行っていません。

## ライセンス

KancolleSnifferは[GNU GPLv3][1]でライセンスします。ただしGPLv3の第7節に関する追加の許可として、FiddlerCore4.dllは[End User License Agreement for FiddlerCore][2]で、DynamicJson.dllは[Microsoft Public License (Ms-PL)][3]でライセンスされます。

[1]: http://sourceforge.jp/magazine/07/09/02/130237
[2]: https://sourceforge.jp/projects/kancollesniffer/wiki/FiddlerCoreLicense
[3]: http://dynamicjson.codeplex.com/license

## 注意

艦名を取得するために、一回はKancolleSnifferを起動した状態で艦これにログインしてください。終了するときに艦名を保存するので、次からはログイン後に起動しても大丈夫です。取得できない艦名は「不明」になります。

FiddlerCoreを使っているので、Firefoxのプロキシ設定で「システムのプロキシ設定を利用する」以外を指定していると動きません。

FiddlerCoreの障害でプロキシの設定がおかしくなり、ブラウザがインターネットに接続できなくなることがあります。もしそうなったらインターネットオプションの「接続」→「LANの設定」→「LANにプロキシ サーバーを使用する」のチェックを外したうえで、レジストリエディタ(regedit)で`HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ProxyServer`を削除してください。
