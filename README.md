# ErrorFileWatcher  

- [KMC007SS] 製造実績受入エラー検出サービス  


## 概要  

- i-Reporter サーバー上に Windows サービスとして常駐する。  
- kemsvr2 サーバーの 自動受入エラーフォルダ を監視し検出する。  
- エラーフォルダ にファイルが作成された事を メールにて通知する。  
- 通知の際、kemsvr2 サーバーの 自動受入ログファイル と 受入エラーとなったファイルを添付。  


## 開発環境  

- Visual Studio Professional 2022 (Visual C# 2022)  


## アプリケーションの種類  

- Windows サービス (.NET Framework) C#  


## メンバー  

- y.watanabae  


## プロジェクト構成  

~~~
D:.
│  .gitignore					# ソース管理除外対象
│  ErrorFileWatcher.sln 			# プロジェクトファイル
│  README.md					# このファイル
│  ReleaseNote.txt				# リリース情報
│  
├─ErrorFileWatcher
│  │  Common.cs 				# 共通 クラス
│  │  DBAccess.cs				# データベースアクセ スクラス
│  │  FileAccess.cs				# ファイルアクセス クラス
│  │  FileWatch.cs				# サービス アプリケーション本体
│  │  Install.bat				# サービス インストーラー
│  │  Program.cs				# サービス コントロールファイル
│  │  ProjectInstaller.cs
│  │  ProjectInstaller.Designer.cs
│  │  ProjectInstaller.resx
│  │  Service1.cs
│  │  Service1.Designer.cs
│  │  Service1.resx
│  │  UnInstall.bat				# サービス アンインストーラー
│  │  [C#] このアプリは、ireposv の C ドライブにインストールしてください。
│  │          
│  └─Properties
│          AssemblyInfo.cs
│      
├─packages
│      DecryptPassword.dll			# パスワード復号化モジュール
│      Oracle.ManagedDataAccess.dll		# Oracle接続モジュール
│      
├─settingfiles
│      ConfigDB - KOKEN_1.xml			# データベース設定ファイル
│      ConfigDB - KOKEN_5.xml			# データベース設定ファイル (テスト用)
│      ConfigFS - kemsvr2.xml			# サーバー設定ファイル
│      ConfigFS - PC090N.xml			# サーバー設定ファイル (テスト用)
│      ConfigSMTP - gmail.xml			# SMTP設定ファイル (テスト用)
│      ConfigSMTP - Koken.xml			# SMTP設定ファイル
│      
└─specification
        [KMC007SS] 製造実績受入エラー検出サービス 機能仕様書_Ver.1.0.0.0.xlsx
~~~


## データベース参照テーブル  

| Table    | Name                      |  
| :------- | :------------------------ |  
| KM0040   | アドレス帳マスタ          |  
| KM1060   | i-Reporter 対象工程マスタ |  

