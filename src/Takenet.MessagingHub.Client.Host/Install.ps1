param($installPath, $toolsPath, $package, $project)

write-host "Trying to update mhh.exe project reference (if one exists)"

$hostFile = "mhh.exe -pause"
$hostPath = Join-Path (Join-Path $installPath "tools\net461\") $hostFile

$projectUri = [uri]$project.FullName;
$hosttUri = [uri]$hostPath;
$hostRelativePath = $projectUri.MakeRelative($hosttUri) -replace "/","\"

$project.ProjectItems | ForEach-Object {
	if ($_.FileNames(0).EndsWith($hostFile)) { 
		$_.Remove()
		$project.ProjectItems.AddFromFile($hostPath) | out-null
		$project.ProjectItems.Item($hostFile).Properties.Item("CopyToOutputDirectory").Value = 2
		$project.Save()
		write-host "Updated reference to $hostPath"
		continue
	}
}
