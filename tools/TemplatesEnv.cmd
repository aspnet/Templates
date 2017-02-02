@echo off

if defined ProgramFiles(x86) (
    set "TemplatesProgramFiles=%ProgramFiles(x86)%"
) else (
    set "TemplatesProgramFiles=%ProgramFiles%"
)

if not defined DNX_PACKAGES (
    set DNX_PACKAGES=%~dp0\..\packages
)

set "TemplatesVSVersion=15.0"
set "TemplatesRoot=%~dp0"
set "TemplatesRoot=%TemplatesRoot:~0,-7%"
set "TemplatesBin=%TemplatesRoot%\artifacts\build"
set "TemplatesIntermediate=%TemplatesRoot%\intermediate"
set "TemplatesReferences=%TemplatesRoot%\references"
set "TemplatesSource=%TemplatesRoot%\src"
set "TemplatesTest=%TemplatesRoot%\test"
set "TemplatesTools=%TemplatesRoot%\tools"
set DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

if exist "%TemplatesProgramFiles%\MSBuild\14.0\Bin\msbuild.exe" (
   set TemplatesMSBuildPath="%TemplatesProgramFiles%\MSBuild\14.0\Bin"
)

set "PATH=%PATH%;%TemplatesMSBuildPath%"
set "PATH=%PATH%;%TemplatesTools%"