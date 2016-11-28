param($installPath, $toolsPath, $package, $project)

function Add-StartProgramIfNeeded {
	[xml] $prjXml = Get-Content $project.FullName
	foreach($PropertyGroup in $prjXml.project.ChildNodes)
	{
		if($PropertyGroup.StartAction -ne $null)
		{
			return
		}
	}

	$propertyGroupElement = $prjXml.CreateElement("PropertyGroup", $prjXml.Project.GetAttribute("xmlns"));
	$startActionElement = $prjXml.CreateElement("StartAction", $prjXml.Project.GetAttribute("xmlns"));
	$propertyGroupElement.AppendChild($startActionElement) | Out-Null
	$propertyGroupElement.StartAction = "Program"
	$startProgramElement = $prjXml.CreateElement("StartProgram", $prjXml.Project.GetAttribute("xmlns"));
	$propertyGroupElement.AppendChild($startProgramElement) | Out-Null
	$propertyGroupElement.StartProgram = "`$(ProjectDir)`$(OutputPath)mhh.exe"
	$prjXml.project.AppendChild($propertyGroupElement) | Out-Null
	$writerSettings = new-object System.Xml.XmlWriterSettings
	$writerSettings.OmitXmlDeclaration = $false
	$writerSettings.NewLineOnAttributes = $false
	$writerSettings.Indent = $true
	$projectFilePath = Resolve-Path -Path $project.FullName
	$writer = [System.Xml.XmlWriter]::Create($projectFilePath, $writerSettings)
	$prjXml.WriteTo($writer)
	$writer.Flush()
	$writer.Close()
}

write-host "Trying to update mhh.exe project reference (if one exists)"

$hostFile = "mhh.exe"
$hostPath = Join-Path (Join-Path $installPath "tools\net461\") $hostFile

$projectPathUri = [String](Split-Path -Path $project.FullName);

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

$project.Save()

Write-Host "Modifying Configurations..."
Add-StartProgramIfNeeded


