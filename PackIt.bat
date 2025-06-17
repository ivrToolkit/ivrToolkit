@echo off
setlocal
set FULL_VERSION=%2
for /f "tokens=1 delims=." %%a in ("%FULL_VERSION%") do set MAJOR=%%a
set ASSEMBLY_VERSION=%MAJOR%.0.0.0

for /f "tokens=1 delims=-" %%v in ("%FULL_VERSION%") do set NUMERIC_VERSION=%%v
set FILE_VERSION=%NUMERIC_VERSION%.0

echo FULL_VERSION = %FULL_VERSION%
echo ASSEMBLY_VERSION = %ASSEMBLY_VERSION%
echo FILE_VERSION = %FILE_VERSION%

dotnet pack -c %1 -o pkg\%1 -p:PackageVersion=%FULL_VERSION% -p:Version=%FULL_VERSION% -p:AssemblyVersion=%ASSEMBLY_VERSION% -p:FileVersion=%FILE_VERSION% ivrToolkit.Core
dotnet pack -c %1 -o pkg\%1 -p:PackageVersion=%FULL_VERSION% -p:Version=%FULL_VERSION% -p:AssemblyVersion=%ASSEMBLY_VERSION% -p:FileVersion=%FILE_VERSION% Plugins\Dialogic\ivrToolkit.Plugin.Dialogic.Common
dotnet pack -c %1 -o pkg\%1 -p:PackageVersion=%FULL_VERSION% -p:Version=%FULL_VERSION% -p:AssemblyVersion=%ASSEMBLY_VERSION% -p:FileVersion=%FILE_VERSION% Plugins\Dialogic\Sip\ivrToolkit.Plugin.Dialogic.Sip
dotnet pack -c %1 -o pkg\%1 -p:PackageVersion=%FULL_VERSION% -p:Version=%FULL_VERSION% -p:AssemblyVersion=%ASSEMBLY_VERSION% -p:FileVersion=%FILE_VERSION% Plugins\Dialogic\Analog\ivrToolkit.Plugin.Dialogic.Analog
dotnet pack -c %1 -o pkg\%1 -p:PackageVersion=%FULL_VERSION% -p:Version=%FULL_VERSION% -p:AssemblyVersion=%ASSEMBLY_VERSION% -p:FileVersion=%FILE_VERSION% Plugins\SipSorcery\ivrToolkit.Plugin.SipSorcery
