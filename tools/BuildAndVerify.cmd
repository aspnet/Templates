@echo off

call %~dp0\EnsureTemplatesEnv.cmd
msbuild %TemplatesRoot%\Templates.msbuild