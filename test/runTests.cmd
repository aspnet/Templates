@echo off

pushd %~dp0
call sqltestUtil.cmd :create

dotnet restore %*
dotnet build

REM Create the database for the tests
pushd ..\intermediate\Test\StarterWeb.IndividualAuth
dotnet ef database update
popd

dotnet test Microsoft.Web.Templates.Web.Tests

call sqltestUtil.cmd :delete

popd

exit /b 0