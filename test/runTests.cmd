@echo off
pushd %~dp0

..\packages\Sake\tools\Sake.exe -I ..\packages\KoreBuild\build -f makefile.shade
          
popd

exit /b 0