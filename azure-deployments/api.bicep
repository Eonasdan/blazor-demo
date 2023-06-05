param location string = resourceGroup().location
param siteName string
param appServicePlanName string

@description('Enter the environment this resource be in (dev, stage, prod).')
@allowed([
  'dev'
  'stage'
  'prod'
])
param environment string

module appServicePlan './appservice-linux.bicep' = {
  name: 'farmDeployment'
  params: {
    appServicePlanName: appServicePlanName
    location: location
    environment: environment
  }
}

module app './webapp-linux.bicep' = {
  name: 'appDeployment'
  params: {
    webSiteName: siteName
    appServicePlanId: appServicePlan.outputs.appServicePlanId
    location: location
    kind: 'linux,api'
    environment: environment
  }
}

@description('The Service Plan (farm) id')
output appServicePlanId string = appServicePlan.outputs.appServicePlanId

@description('The Service Plan resource name')
output appServicePlanName string = appServicePlan.outputs.appServicePlanName

@description('The App Service resource name')
output appServiceName string = app.outputs.appServiceName
