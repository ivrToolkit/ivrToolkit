mkdir lib\net40
mkdir tools\net40
mkdir content

xcopy /E/Y "..\..\ivrToolkit.Core\System Recordings\*.*" "content\System Recordings\"
copy ..\..\examples\SimulatorTest\Voice.Properties content
copy ..\..\examples\SimulatorTest\NLog.config content
copy ..\..\README.md content
copy ..\..\ReleaseNotes.md content
copy ..\..\ivrToolkit.Core\install.ps1 tools\net40

copy ..\..\ivrToolkit.Core\bin\debug\ivrToolkit.core.* lib\net40\
copy ..\..\ivrToolkit.DialogicPlugin\bin\debug\ivrToolkit.dialogicPlugin.* lib\net40\
copy ..\..\ivrToolkit.SimulatorPlugin\bin\debug\ivrToolkit.SimulatorPlugin.* lib\net40\

nuget pack