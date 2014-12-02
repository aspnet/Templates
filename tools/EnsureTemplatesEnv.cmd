@echo off

if not defined TemplatesRoot (
    echo Initializing Templates environment
    call %~dp0\TemplatesEnv.cmd
)
