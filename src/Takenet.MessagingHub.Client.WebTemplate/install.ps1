param($installPath, $toolsPath, $package, $project)

function SetCopyToOutputDirectory
{
	$item = $project.ProjectItems.Item("application.json")
	$item.Properties.Item("CopyToOutputDirectory").Value = 2
}

Write-Host "Setting copy to output directory..."
SetCopyToOutputDirectory