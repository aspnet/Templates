@echo off
pushd %~dp0

sqllocaldb p "templateTests" 2> nul
sqllocaldb d "templateTests" 2> nul
del %userprofile%\StarterWeb*.Test*.* 2> nul
sqllocaldb c "templateTests"

..\packages\Sake\tools\Sake.exe -I ..\packages\KoreBuild\build -f makefile.shade

sqllocaldb p "templateTests"
sqllocaldb d "templateTests"
          
popd

exit /b 0