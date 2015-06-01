param($installPath, $toolsPath, $package, $project)

# more info here: https://msdn.microsoft.com/en-us/library/envdte.projectitems(v=vs.120).aspx

$modelFolder = $project.ProjectItems | where Name -EQ "Models"
if($modelFolder){
	$genericModelGenerator = $modelFolder.ProjectItems | where Name -EQ "GenericModelGenerator.tt" 
	if($genericModelGenerator){
		$genericModelGenerator.Properties.Item("BuildAction").Value = [int]0
		$genericModelGenerator.Properties.Item("CustomTool").Value = ""
	}
	else{
		Write-Host "Could not patch the generic model generator..."
	}

	$sampleModelGeneratorConfig = $modelFolder.ProjectItems | where Name -EQ "SampleModelGeneratorConfig.tt" 
	if($sampleModelGeneratorConfig){
		$sampleModelGeneratorConfig.Properties.Item("BuildAction").Value = [int]0
	}
}
else{
		Write-Host "Model folder not found. Bypassing configuration..."
}