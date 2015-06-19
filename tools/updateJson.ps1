
function LogMessage {
    param (
    [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
    $message
    )

    $logFile = Join-Path $env:TEMP UpdateJsonScriptOutput.txt 
    Out-File -FilePath $logFile -InputObject $message -Append
}

# This function processes one .nupkg file
function ProcessFile {
    param (
    [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
    $sourceFile
    )

    $semanticRegEx = "(?<Version>\d+(\.\d+){0,3})(?<Release>-[a-z][0-9a-z-]*)?(\.nupkg)"
    
    $fileName = $sourceFile.Name
    
    if ($fileName -match $semanticRegEx)
    {
        $packageNameWithDot = $fileName.Remove($fileName.IndexOf($Matches.Version))
        $packageName = $packageNameWithDot.Substring(0, $packageNameWithDot.Length-1)
        $packageVersion = $Matches.Version
        $packageVersionWithRelease = $Matches.Version + $Matches.Release

        if ($PackageNameVersionHash.ContainsKey($packageName))
        {
            if (!$PackageNameVersionHash[$packageName].ContainsKey($packageVersion))
            {
                $PackageNameVersionHash[$packageName].Add($packageVersion, $packageVersionWithRelease)
            }
        }
        else
        {
            $PackageNameVersionHash.Add($packageName, @{})
            $PackageNameVersionHash[$packageName].Add($packageVersion, $packageVersionWithRelease)
        }
    }
}


# This function edits a given project.json file and modifies the package 
# versions with the new version number that is integrated. 
# This should be run after the above functions because we need the 
# $PackageNameVersionHash to be populated for this to work.
function ProcessProjectFile {
    param (
    [Parameter(Mandatory=$true)]
    $projectJsonFilePath
    )

    if (Test-Path $projectJsonFilePath)
    {
        $tempFile = Join-Path $env:TEMP "PackageIntegrationTempFile.txt"
        if (Test-Path $tempFile)
        {
            del $tempFile
        }

        foreach ($line in Get-Content $projectJsonFilePath)
        {
            $matchPattern = """(?<PackageName>\S*)""\s*:\s*""(?<PackageVersion>\S+)"".*"
            if ($line -match $matchPattern -and $PackageNameVersionHash.Contains($Matches.PackageName))
            {
                $packageName = $Matches.PackageName
                $packageVersion = $Matches.PackageVersion
                $packageVersionWithoutWildCard = $packageVersion
                $releaseIndex = $packageVersion.IndexOf("-")
                
                if ($releaseIndex -gt -1)
                {
                    $packageVersionWithoutWildCard = $packageVersion.SubString(0, $releaseIndex)
                }
                
                # if there is no match, we won't replace anything
                if ($PackageNameVersionHash[$packageName].ContainsKey($packageVersionWithoutWildCard))
                {
                    $line = $line.Replace($packageVersion, $PackageNameVersionHash[$packageName][$packageVersionWithoutWildCard])
                }
            }
            Out-File -FilePath $tempFile -InputObject $line -Encoding utf8 -Append
        }

        copy $tempFile $projectJsonFilePath
        del $tempFile
    }
    else
    {
        LogMessage "Could not find file: $projectJsonFilePath"
    }
}

#Beginning of the script execution

LogMessage "--------------------------------------------------------------------------------------------"
Get-Date | %{"                   Script Log : " + $_} | %{ LogMessage $_}
LogMessage "--------------------------------------------------------------------------------------------"

#Global variables for the script
$PackageNameVersionHash = @{}
$coherencePath = $args[0]

$ScriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$TemplatesRootDir = $ScriptDir | Split-Path -Parent

dir $coherencePath | % { ProcessFile $_; }

#Edit the template project files
$templateFolder = Get-Location
foreach($templateFile in Get-ChildItem -path $templateFolder -recurse -include Project.json) {
    ProcessProjectFile $templateFile.FullName
}