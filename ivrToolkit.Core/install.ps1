param($installPath, $toolsPath, $package, $project)

$folder = $project.ProjectItems.Item("System Recordings")

foreach($myitem in $folder.ProjectItems) {
    $myitem.Properties.Item("CopyToOutputDirectory").Value = 1
}

$project.ProjectItems.Item("voice.properties").Properties.Item("CopyToOutputDirectory").Value = 1
$project.ProjectItems.Item("ivrToolkit.Core.nlog").Properties.Item("CopyToOutputDirectory").Value = 1