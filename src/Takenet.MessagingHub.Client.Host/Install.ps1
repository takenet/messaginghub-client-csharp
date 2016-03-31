param($installPath, $toolsPath, $package, $project)

write-host "Try to update mhh.exe project reference"

$hostFile = "mhh.exe"
$hostPath = Join-Path (Join-Path $installPath "tools\net461\") $hostFile

$projectUri = [uri]$project.FullName;
$hosttUri = [uri]$hostPath;
$hostRelativePath = $projectUri.MakeRelative($hosttUri) -replace "/","\"

$project.ProjectItems | ForEach-Object {
	if ($_.Name.EndsWith($hostFile)) { 
		$_.Remove()
		$project.ProjectItems.AddFromFile($hostPath)
		$project.ProjectItems.Item($hostFile).Properties.Item("CopyToOutputDirectory").Value = 2
		$project.Save()
		write-host "Updated reference to $hostPath"
		continue
	}
}

#if ($onProject) {
#	# Add content item as a link that is saved at higher folder.
#	# (This is normal case.)
#	if ($hostRelativePath -like "..\*") {
#		$project.ProjectItems.AddFromFile($hostPath)
#		$project.ProjectItems.Item($hostFile).Properties.Item("CopyToOutputDirectory").Value = 2
#	}
#	$project.Save()

#	# Treat the project file as a MSBuild script xml instead of DTEnv object model.
#	Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
#	$projectXml = ([Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | select -First 1).Xml

#	# Add content item as a link that is saved at lower folder.
#	# (This is the case that .csproj file and packages folder are in a same folder.)
#	if ($hostRelativePath -notlike "..\*") {
#		$itemGrp = $projectXml.CreateItemGroupElement()
#		$projectXml.AppendChild($itemGrp)
#		$item = $itemGrp.AddItem("Content", $hostRelativePath)
#		$item.AddMetadata("Link", $hostFile)
#		$item.AddMetadata("CopyToOutputDirectory", "PreserveNewest")
#	}

#	$project.Save()
#}