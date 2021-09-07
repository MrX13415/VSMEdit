@echo off

:: Info: https://github.com/dotnet/designs/blob/main/accepted/2020/single-file/design.md#user-experience

cls

:: Get git abbreviated commit hash ...
for /f "delims=" %%i in ('git log -1 --pretty^=%%h') do set gitHash=%%i

echo.
echo. Publishing using commit %gitHash% ...
echo.

::set buildconfig=Debug
set buildconfig=Release
set "tag="

:: Clean ...
echo.   Clean ...
rd /S /Q bin\Publish\Current
dotnet clean -c %buildconfig%

:: Windows ...
echo.
echo.   Building for Windows ...
echo.
dotnet publish -r win-x64 -c %buildconfig% -o bin\Publish\Current /p:IncludeNativeLibrariesForSelfExtract=true /p:PublishSingleFile=true /p:PublishTrimmed=true /p:TrimMode=Link

:: Mac ...
echo.
echo.   Building for MacOS ...
echo.
dotnet publish -r osx-x64 -c %buildconfig% -o bin\Publish\Current /p:IncludeNativeLibrariesForSelfExtract=true /p:PublishSingleFile=true /p:PublishTrimmed=true /p:TrimMode=Link 

:: Detemine app version ...
set "appFile=bin\Publish\Current\vsmedit.exe"
for /f "USEBACKQ" %%i in (`powershell -NoLogo -NoProfile -Command ^(Get-Item "%appFile%"^).VersionInfo.FileVersion`) do set appVersion=%%i

:: Open output folder in explorer ...
explorer .\bin\Publish

:: Compress ...
set "zip=VSMEdit-v%appVersion%-%gitHash%%tag%.zip"
echo.
echo.   Compress "%zip%" ...
echo.
if exist "bin\Publish\%zip%" del "bin\Publish\%zip%"
powershell -Command "Compress-Archive -Path 'bin\Publish\Current\*' -DestinationPath 'bin\Publish\%zip%'"

echo. Done




