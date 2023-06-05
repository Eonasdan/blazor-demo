Param (
    [Parameter(Mandatory=$true)]
    [string] $DeploymentOutputs
)

$outputs = ConvertFrom-Json $DeploymentOutputs

foreach ($key in $outputs.PSObject.Properties.Name) {
    $value = $outputs.$key.value
    if ($value -is [System.Security.SecureString]) {
        $value = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($value))
        Write-Host "##vso[task.setvariable variable=$key;issecret=true]$value"
    } else {
        Write-Host "##vso[task.setvariable variable=$key]$value"
    }
}