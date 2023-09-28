param resourceName string
param userAssignedIdentity string

@description('Enter the environment this resource be in (dev, stage, prod).')
@allowed([
  'dev'
  'stage'
  'prod'
])
param environment string

@description('Provide the name of the associated application.')
param tagApplication string
param created string = utcNow()
param location string = resourceGroup().location

resource resourceExists 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'resourceExists'
  location: location
  kind: 'AzurePowerShell'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentity}': {}
    }
  }
  tags: {
    Application: tagApplication
    CreatedOnDate: created
    Environment: environment
  }
  properties: {
    forceUpdateTag: created
    azPowerShellVersion: '9.1'
    timeout: 'PT10M'
    arguments: '-resourceName ${resourceName} -resourceGroupName ${resourceGroup().name}'
    scriptContent: '''
     param(
       [string] $resourceName, 
       [string] $resourceGroupName
       )
       $Resource = Get-AzResource -Name $resourceName -ResourceGroupName $resourceGroupName
       $ResourceExists = $null -ne $Resource
       $DeploymentScriptOutputs = @{}
       $DeploymentScriptOutputs['Exists'] = $ResourceExists
       $DeploymentScriptOutputs['Resource'] = {}
       if ($null -ne $Resource) {
        $DeploymentScriptOutputs['Resource'] = $Resource
       }
     '''
    cleanupPreference: 'OnSuccess'
    retentionInterval: 'P1D'
  }
}

output exists bool = resourceExists.properties.outputs.Exists
output resource object = resourceExists.properties.outputs.Resource
