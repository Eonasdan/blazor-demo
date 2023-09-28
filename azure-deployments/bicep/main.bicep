targetScope = 'subscription'
param tagApplication string = 'Blazor Demo'
param location string
param baseName string
param storageAccountName string
param b2cName string

@description('Enter the environment this resource be in (dev, stage, prod).')
@allowed([
  'dev'
  'stage'
  'prod'
])
param environment string
param baseTime string = utcNow('d')

resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-${ baseName }-${ environment }-${ location }'
  location: location
  tags: {
    Application: tagApplication
    Environment: environment
    CreatedOnDate: baseTime
  }
}

module generatedNames './utility/generate-name.bicep' = {
  name: 'generateName'
  scope: rg
  params: {
    location: location
    environment: environment
    vnetName: baseName
    identityName: baseName
    sqlDbName: baseName
    sqlServerName: baseName
    staticSiteName: baseName
    storageAccountName: storageAccountName
    webSiteName: baseName
    appServicePlanName: baseName
    b2cName: b2cName
    billboardName: '${baseName}-billboard'
  }
}

module managedIdentity 'userAssignedIdentities.bicep' = {
  name: 'managedIdentity'
  scope: rg
  params: {
    identityName: generatedNames.outputs.identityName
    environment: environment
    tagApplication: tagApplication
    location: location
  }
}

module vnetExists 'utility/existing.bicep' = {
  name: 'vnetExists'
  scope: rg
  params: {
    userAssignedIdentity: managedIdentity.outputs.resourceId
    resourceName: generatedNames.outputs.vnetName
    environment: environment
    tagApplication: tagApplication
    location: location
  }
}

module vnet 'vnet.bicep' = {
  name: 'vnet'
  scope: rg
  params: {
    deployVnet: !vnetExists.outputs.exists
    vnetName: generatedNames.outputs.vnetName
    location: location
    environment: environment
    tagApplication: tagApplication
  }
}

module sqlDb 'azure-sql.bicep' = {
  name: 'sqlDb'
  scope: rg
  params:{
    location: location
    dbName: generatedNames.outputs.sqlDbName
    environment: environment
    managedIdentityResourceId: managedIdentity.outputs.resourceId
    serverName: generatedNames.outputs.sqlServerName
    subnetName: vnet.outputs.subnetName
    tagApplication: tagApplication
  }
}

module appServicePlan './appservice-linux.bicep' = {
  name: 'farmDeployment'
  scope: rg
  params: {
    appServicePlanName: generatedNames.outputs.appServicePlanName
    location: location
    environment: environment
    tagApplication: tagApplication
  }
}

module api './webapp-linux.bicep' = {
  name: 'appDeployment'
  scope: rg
  params: {
    webSiteName: generatedNames.outputs.webSiteName
    appServicePlanId: appServicePlan.outputs.appServicePlanId
    managedIdentityClientId: managedIdentity.outputs.clientId
    managedIdentityResourceId: managedIdentity.outputs.resourceId
    sqlDatabaseName: generatedNames.outputs.sqlDbName
    sqlFQDN: sqlDb.outputs.fullyQualifiedDomainName
    location: location
    environment: environment
    tagApplication: tagApplication
    vnetName: vnet.outputs.vnetName
    subnetName: vnet.outputs.subnetName
  }
}

module staticSite 'static.bicep' = {
  name: 'blazor-static'
  scope: rg
  params:{
    siteName: generatedNames.outputs.staticSiteName
    environment: environment
    tagApplication: tagApplication
    location: location
  }
}

module storage 'storage.bicep' = {
  name: 'b2c-storage'
  scope: rg
  params:{
    b2cCors: generatedNames.outputs.b2cName
    storageAccountName: generatedNames.outputs.storageAccountName
    environment: environment
    tagApplication: tagApplication
    location: location
  }
}