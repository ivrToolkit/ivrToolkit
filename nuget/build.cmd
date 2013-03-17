mkdir lib\net40
mkdir tools
mkdir content

xcopy /E/Y "..\ivrToolkit.Core\System Recordings\*.*" "content\System Recordings\"
copy ..\examples\SimulatorTest\Voice.Properties content

copy ..\ivrToolkit.Core\bin\debug\ivrToolkit.core.* lib\net40\
copy ..\ivrToolkit.DialogicPlugin\bin\debug\ivrToolkit.dialogicPlugin.* lib\net40\
copy ..\ivrToolkit.SimulatorPlugin\bin\debug\ivrToolkit.SimulatorPlugin.* lib\net40\

nuget pack