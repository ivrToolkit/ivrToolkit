dotnet pack -c %1 -o pkg\%1 -p:PackageVersion=%2 ivrToolkit.Core
dotnet pack -c %1 -o pkg\%1 -p:PackageVersion=%2 Plugins\Dialogic\ivrToolkit.Plugin.Dialogic.Common
dotnet pack -c %1 -o pkg\%1 -p:PackageVersion=%2 Plugins\Dialogic\Sip\ivrToolkit.Plugin.Dialogic.Sip
dotnet pack -c %1 -o pkg\%1 -p:PackageVersion=%2 Plugins\Dialogic\Analog\ivrToolkit.Plugin.Dialogic.Analog
dotnet pack -c %1 -o pkg\%1 -p:PackageVersion=%2 Plugins\SipSorcery\ivrToolkit.Plugin.SipSorcery
