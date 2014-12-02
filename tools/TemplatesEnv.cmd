@echo off

if defined ProgramFiles(x86) (
    set "TemplatesProgramFiles=%ProgramFiles(x86)%"
) else (
    set "TemplatesProgramFiles=%ProgramFiles%"
)

set "TemplatesVSVersion=14.0"
set "TemplatesRoot=%~dp0"
set "TemplatesRoot=%TemplatesRoot:~0,-7%"
set "TemplatesBin=%TemplatesRoot%\bin"
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