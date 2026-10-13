@echo off
dotnet publish Minesweeper.csproj -c Release -o dist
if errorlevel 1 exit /b 1
echo.
echo Built: %~dp0dist\Minesweeper.exe
