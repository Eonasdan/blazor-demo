@minLength(3)
@maxLength(100)
@description('Name of the User Identity.')
param identityName string

@description('Enter the environment this resource be in (dev, stage, prod).')
@allowed([
  'dev'
  'stage'
  'prod'
])
param environment string

@description('Provide the name of the associated application.')
param tagApplication string

@description('Provide a location.')
param location string = resourceGroup().location
param created string = utcNow('d')

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
  tags: {
    Application: tagApplication
    CreatedOnDate: created
    Environment: environment
  }
}

output principalId string = managedIdentity.properties.principalId
output resourceId string = managedIdentity.id
output clientId string = managedIdentity.properties.clientId
