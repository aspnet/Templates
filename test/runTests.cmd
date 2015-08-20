@echo off

call %TemplatesTest%\sqltestUtil.cmd :create

%TemplatesRoot%\packages\Sake\tools\Sake.exe -I %TemplatesRoot%\packages\KoreBuild\build -f test\makefile.shade

call %TemplatesTest%\sqltestUtil.cmd :delete

exit /b 0