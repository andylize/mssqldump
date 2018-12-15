@ECHO OFF

setlocal

set Config=%1
if "%Config%" == "" set Config=Release

set BuildDir=%dp~0
echo %BuildDir%

msbuild mssqldump.sln /p:Configuration=%Config% /p:OutputPath=..\Build\%Config%