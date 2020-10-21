@echo off
if "%~1" == "" (
    echo Wrong arguments number
	echo Try build.cmd "version"
    exit /b -1
)

set DIR_NAME=build-v%~1

if exist "%DIR_NAME%" (
	echo This build already exists
	exit /b -1
)

MD "%DIR_NAME%"

cd src/BackendApi
dotnet publish --configuration Release
if %ERRORLEVEL% NEQ 0 (
	echo "Build failed"
    exit /b -1
)

cd ..
cd FrontendTask
dotnet publish --configuration Release
if %ERRORLEVEL% NEQ 0 (
	echo "Build failed"
    exit /b -1
)

cd ..
cd JobLogger
dotnet publish --configuration Release
if %ERRORLEVEL% NEQ 0 (
    echo "Build failed"
    exit /b -1
)

cd ..
cd ..

mkdir "%DIR_NAME%/BackendApi"
mkdir "%DIR_NAME%/FrontendTask"
mkdir "%DIR_NAME%/Config"
mkdir "%DIR_NAME%/JobLogger"

xcopy src\BackendApi\bin\Release\netcoreapp3.1\publish "%DIR_NAME%"\BackendApi\ /s /e
xcopy src\FrontendTask\bin\Release\netcoreapp3.1\publish "%DIR_NAME%"\FrontendTask\ /s /e
xcopy src\JobLogger\bin\Release\netcoreapp3.1\publish "%DIR_NAME%"\JobLogger\ /s /e
xcopy start.cmd "%DIR_NAME%"
xcopy stop.cmd "%DIR_NAME%"
xcopy src\config\Config.json "%DIR_NAME%\Config"

echo BUILD SUCCESS
echo GO TO %DIR_NAME% AND RUN "start.cmd"
