set installDir="%ALLUSERSPROFILE%\%USERNAME%\TfsStates"
set appExe="%ALLUSERSPROFILE%\%USERNAME%\TfsStates\TfsStates.exe"

pushd Files
xcopy /s /i /y /r *.* %installDir%

powershell "$s=(New-Object -COM WScript.Shell).CreateShortcut('%userprofile%\Start Menu\Programs\TfsStates.lnk');$s.TargetPath='%appExe%';$s.Save()"

popd

echo "Press enter to exit"
prompt