:: Name:     DXWeb.cmd
:: Purpose:  Package the nuspec file and store it in the Deploy location
:: Author:   don@wibier.net
:: Revision: December 2016 Initial script for VisualStudio Post Build Event
:: Usage:    DXWeb "$(SolutionDir)" "$(ProjectName)" "$(ConfigurationName)"

@ECHO OFF
SETLOCAL ENABLEEXTENSIONS
SET me=%~n0
SET solution=%~1
SET project=%~2
SET config=%~3
SET nuSpec=%solution%%project%\%project%
SET output=%solution%.deploy\%config%
SET nugetArgs=pack "%nuSpec%.csproj" -Properties "Configuration=%config%" -OutputDirectory "%output%" -Symbols

ECHO Checking for NuGet
IF NOT EXIST "%solution%.nuget\nuget.exe" powershell -Command "Invoke-WebRequest https://www.nuget.org/nuget.exe -OutFile '%solution%.nuget\nuget.exe'"

ECHO Checking output folder
IF NOT EXIST "%output%" MKDIR "%output%"
ECHO Packing the nuget package with command:
ECHO [%solution%.nuget\nuget %nugetArgs%]
%solution%.nuget\nuget %nugetArgs%
IF ERRORLEVEL 1 (
    ECHO ERROR: Failed building [%nuSpec%]. Please check output window for errors!
    EXIT /B 1
)
ECHO Package [%output%\%project%.nupkg] created!
EXIT /B 0
		