set destDir=%USERPROFILE%\Desktop\TfsStates-Setup
if exist %destDir% rmdir /s /q %destDir%
if not exist %destDir% mkdir %destDir%

REM backup json files before build.bat changes
echo Backing up json files
xcopy electron-builder.json %destDir%
xcopy package.json %destDir%

echo packaging
call build.bat "./" "./package.json" "./electron-builder.json" "--windows"

REM restore json files
echo restoring json files
xcopy /y %destDir%\electron-builder.json .
xcopy /y %destDir%\package.json .

echo Removing backup json files
del/f /q %destDir%\*.json

REM move created setup out of project dir
pushd dist
xcopy tfsstates*.exe %destDir%
popd

REM cleanup
echo Removing dist folder
rmdir /s /q dist

echo Removing node_modules folder
rmdir /s /q node_modules

echo Removing package-lock.json
del /f package-lock.json

REM open setup location
explorer %destDir%