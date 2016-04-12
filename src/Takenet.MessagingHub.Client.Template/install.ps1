param($installPath, $toolsPath, $package, $project)

function HasStartAction ($item)
{
    foreach ($property in $item.Properties)
    {
       if ($property.Name -eq "StartAction")
       {
           return $true
       }            
    } 

    return $false
}

function ModifyConfigurations
{
    $configurationManager = $project.ConfigurationManager

    foreach ($name in $configurationManager.ConfigurationRowNames)
    {
        $projectConfigurations = $configurationManager.ConfigurationRow($name)

        foreach ($projectConfiguration in $projectConfigurations)
        {                

            if (HasStartAction $projectConfiguration)
            {
                $newStartAction = 1
                [String]$newStartProgram = $toolsPath + "\mhh.exe"                
                Write-Host "Changing project start action to " $newStartAction
                Write-Host "Changing project start program to " $newStartProgram                
                $projectConfiguration.Properties.Item("StartAction").Value = $newStartAction
                $projectConfiguration.Properties.Item("StartProgram").Value = $newStartProgram                
            }
        }
    }

    $project.Save
}

function SetCopyToOutputDirectory
{
	$item = $project.ProjectItems.Item("application.json")
	$item.Properties.Item("CopyToOutputDirectory").Value = 2
}

Write-Host "Modifying Configurations..."
ModifyConfigurations

Write-Host "Setting copy to output directory..."
SetCopyToOutputDirectory