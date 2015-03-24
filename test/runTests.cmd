@echo off
pushd %~dp0

call sqltestUtil.cmd :create

..\packages\Sake\tools\Sake.exe -I ..\packages\KoreBuild\build -f makefile.shade

call sqltestUtil.cmd :delete

popd

exit /b 0