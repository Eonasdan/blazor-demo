@minLength(5)
@maxLength(40)
@description('Name of the Azure App Service Plan.')
param appServicePlanName string = uniqueString(resourceGroup().id)

@description('Provide the name of the associated application.')
param tagApplication string

@description('Enter the environment this resource be in (dev, stage, prod).')
@allowed([
  'dev'
  'stage'
  'prod'
])
param environment string

@description('Provide a location.')
param location string = resourceGroup().location
param created string = utcNow('d')

resource appServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
    size: 'B1'
    family: 'B'
    capacity: 1
  }
  properties: {
    reserved: true
  }
  kind: 'linux'
  tags: {
    Application: tagApplication
    CreatedOnDate: created
    Environment: environment
  }
}

@description('The Service Plan (farm) id')
output appServicePlanId string = appServicePlan.id

