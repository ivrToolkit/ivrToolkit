mkdir lib\net40
mkdir tools\net40
mkdir content

xcopy /E/Y "..\..\ivrToolkit.Core\System Recordings\*.*" "content\System Recordings\"
copy ..\..\examples\SimulatorTest\Voice.Properties content
copy ..\..\examples\SimulatorTest\NLog.config content
copy ..\..\README.md content
copy ..\..\ReleaseNotes.md content
copy ..\..\ivrToolkit.Core\install.ps1 tools\net40

copy ..\..\ivrToolkit.Core\bin\x86\debug\ivrToolkit.core.* lib\net40\
copy ..\..\ivrToolkit.DialogicPlugin\bin\x86\debug\ivrToolkit.dialogicPlugin.* lib\net40\
copy ..\..\ivrToolkit.SimulatorPlugin\bin\debug\ivrToolkit.SimulatorPlugin.* lib\net40\
copy ..\..\ivrToolkit.DialogicSipPlugin\bin\x86\Debug\ivrToolkit.DialogicSipPlugin.* lib\net40\
copy ..\..\Release\ivrToolkit.DialogicSipLibrary.* lib\net40\
copy ..\..\Release\ivrToolkit.DialogicSipWrapper.* lib\net40\

rem nuget pack