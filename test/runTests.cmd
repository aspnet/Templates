@echo off

pushd %~dp0
call sqltestUtil.cmd :create

dotnet restore %*
dotnet build
dotnet test Microsoft.Web.Templates.Web.Tests

call sqltestUtil.cmd :delete

popd

exit /b 0