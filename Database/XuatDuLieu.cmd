@echo off
chcp 65001 >nul
echo Dang xuat du lieu tu database FastFoodDb...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0XuatDuLieu.ps1"
echo.
pause
