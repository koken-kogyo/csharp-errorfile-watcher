@echo off

::バッチファイル自身のフルパスへ移動
cd /d %~dp0

::管理者として実行されているかチェック
for /f "tokens=3 delims=\ " %%i in ('whoami /groups^|find "Mandatory"') do set LEVEL=%%i

::管理者でないときはバッチファイル自身を管理者として実行しなおす
if NOT "%LEVEL%"=="High" (
  @powershell -NoProfile -ExecutionPolicy unrestricted -Command "Start-Process %~f0 -Verb runas"
  exit
)

::サービスをインストールして開始する
start ErrorFileWatcher.exe /i

