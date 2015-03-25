@echo off

if not defined BUILD_BRANCH (
  echo You must set BUILD_BRANCH to dev or release before running this command
  exit /b 0
)

if defined ProgramFiles(x86) (
    set "TemplatesProgramFiles=%ProgramFiles(x86)%"
) else (
    set "TemplatesProgramFiles=%ProgramFiles%"
)

if defined PACKAGE_DROP_SHARE (
    set "PackageDropShare=%PACKAGE_DROP_SHARE%"
) else (
    set "PackageDropShare=\\projectk-tc\drops\latest-packages\%BUILD_BRANCH%"
)

set "TemplatesVSVersion=14.0"
set "TemplatesRoot=%~dp0"
set "TemplatesRoot=%TemplatesRoot:~0,-7%"
set "TemplatesBin=%TemplatesRoot%\artifacts\build"
set "TemplatesIntermediate=%TemplatesRoot%\intermediate"
set "TemplatesReferences=%TemplatesRoot%\references"
set "TemplatesSource=%TemplatesRoot%\src"
set "TemplatesTest=%TemplatesRoot%\test"
set "TemplatesTools=%TemplatesRoot%\tools"

if exist "%TemplatesProgramFiles%\MSBuild\14.0\Bin\msbuild.exe" (
   set TemplatesMSBuildPath="%TemplatesProgramFiles%\MSBuild\14.0\Bin"
)

set "PATH=%PATH%;%TemplatesMSBuildPath%"
set "PATH=%PATH%;%TemplatesTools%"