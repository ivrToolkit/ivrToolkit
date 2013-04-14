param($installPath, $toolsPath, $package, $project)

$folder = $project.ProjectItems.Item("System Recordings")

foreach($myitem in $folder.ProjectItems) {
    $copyToOutput1 = $myitem.Properties.Item("CopyToOutputDirectory")
    $copyToOutput1.Value = 2
}
