@echo off

set installDir="%ALLUSERSPROFILE%\%USERNAME%\TfsStates"
set shortcut="%userprofile%\Start Menu\Programs\TfsStates.lnk"
set dataDir="%appdata%\TfsStates"

echo Install Dir: %installDir%
echo Data Dir: %dataDir%
echo Shortcut: %shortcut%

echo Uninstall starting

if exist %installDir% echo Removing %installDir% & rd /q /s %installDir%
if exist %dataDir% echo Removing %dataDir% & rd /q /s %dataDir%
if exist %shortcut% echo Removing %shortcut% & del /f %shortcut%

echo Done, press Enter to exit
pause
