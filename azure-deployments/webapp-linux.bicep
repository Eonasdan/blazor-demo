@description('Provide a location.')
param location string = resourceGroup().location

@minLength(5)
@maxLength(40)
@description('Name of the Azure Website.')
param webSiteName string = uniqueString(resourceGroup().id)

@description('Provide a App Service Plan Id.')
param appServicePlanId string

@description('Enter the environment this resource be in (dev, stage, prod).')
@allowed([
  'dev'
  'stage'
  'prod'
])
param environment string

param created string = utcNow('MM/dd/yyyy')

param kind string = 'linux'

var appName = 'ap-${webSiteName}-${environment}-${location}'

resource appService_resource 'Microsoft.Web/sites@2022-03-01' = {
  name: appName
  location: location
  kind: kind
  properties: {
    serverFarmId: appServicePlanId
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|7.0'
    }
    reserved: true
  }
  identity: {
    type:'SystemAssigned'
  }
  tags: {
    CreatedOnDate: created
    Environment: environment
  }
}

@description('The App Service resource name')
output appServiceName string = appName
