@echo off
title Red Dragon - Start
echo ========================================
echo    Red Dragon - Uruchamianie serwera
echo ========================================
echo.

echo [0/2] Ubijam stary proces API (zwalniam zablokowany .exe)...
taskkill /F /IM RedDragonAPI.exe >nul 2>&1

echo [1/2] Uruchamiam API (.NET 8)...
start "RedDragon API" cmd /k "cd /d %~dp0RedDragonAPI && dotnet run"

echo [2/2] Uruchamiam Angular Client...
start "RedDragon Client" cmd /k "cd /d %~dp0red-dragon-client && ng serve --open"

echo.
echo ========================================
echo    API:    http://localhost:5177
echo    Client: http://localhost:4200
echo    Swagger: http://localhost:5177/swagger
echo ========================================
echo.
echo Oba serwery uruchomione w osobnych oknach.
pause
