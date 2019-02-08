@echo OFF

rem thanks to http://stackoverflow.com/questions/15567809/batch-extract-path-and-filename-from-a-variable
rem thanks to http://stackoverflow.com/questions/5553040/batch-file-for-loop-with-spaces-in-dir-name
SETLOCAL
set file=%1
FOR /f "delims=" %%i IN ("%file%") DO (
set filepath=%%~di%%~pi
set filename=%%~ni%%~xi
)

set action=%3
set action=%action:~1,1%

pushd %filepath%
rename %filename% web.config
"%windir%\Microsoft.NET\Framework\v4.0.30319\aspnet_regiis.exe" -p%action%f %2 "%filepath:~0,-1%"
rename web.config %filename%
popd

rem usage: protect_config.bat <fullpathtoconfig> <sectionname> <action>
rem   <action> is -e for encrypt, or -d for decrypt
