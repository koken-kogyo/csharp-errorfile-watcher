@echo off

::�o�b�`�t�@�C�����g�̃t���p�X�ֈړ�
cd /d %~dp0

::�Ǘ��҂Ƃ��Ď��s����Ă��邩�`�F�b�N
for /f "tokens=3 delims=\ " %%i in ('whoami /groups^|find "Mandatory"') do set LEVEL=%%i

::�Ǘ��҂łȂ��Ƃ��̓o�b�`�t�@�C�����g���Ǘ��҂Ƃ��Ď��s���Ȃ���
if NOT "%LEVEL%"=="High" (
  @powershell -NoProfile -ExecutionPolicy unrestricted -Command "Start-Process %~f0 -Verb runas"
  exit
)

::�T�[�r�X���C���X�g�[�����ĊJ�n����
start ErrorFileWatcher.exe /i

