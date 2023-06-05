@description('Provide a location.')
param location string = resourceGroup().location

@minLength(5)
@maxLength(40)
@description('Name of the Azure App Service Plan.')
param appServicePlanName string = uniqueString(resourceGroup().id)

@description('Enter the environment this resource be in (dev, stage, prod).')
@allowed([
  'dev'
  'stage'
  'prod'
])
param environment string

param created string = utcNow('MM/dd/yyyy')

var aspName = 'asp-${appServicePlanName}-${environment}-${location}'

resource appServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: aspName
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
    CreatedOnDate: created
    Environment: environment
  }
}

@description('The Service Plan (farm) id')
output appServicePlanId string = appServicePlan.id

@description('The Service Plan resource name')
output appServicePlanName string = aspName

