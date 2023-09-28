param vnetName string = ''
param identityName string = ''
param sqlServerName string = ''
param sqlDbName string = ''
param appServicePlanName string = ''
param webSiteName string = ''
param storageAccountName string = ''
param b2cName string = ''
param staticSiteName string = ''
param billboardName string = ''

@description('Enter the environment this resource be in (dev, stage, prod).')
@allowed([
  'dev'
  'stage'
  'prod'
])
param environment string

param location string = resourceGroup().location

output vnetName string = (!empty(vnetName) ? 'vnet-${vnetName}-${environment}-${location}' : '')

output identityName string = (!empty(identityName) ? 'id-${identityName}-${environment}-${location}' : '')

output sqlServerName string = (!empty(sqlServerName) ? 'sql-${sqlServerName}-${environment}-${location}' : '')

output sqlDbName string = (!empty(sqlDbName) ? 'sqldb-${sqlDbName}-${environment}-${location}' : '')

output appServicePlanName string = (!empty(appServicePlanName) ? 'asp-${appServicePlanName}-${environment}-${location}' : '')

output webSiteName string = (!empty(webSiteName) ? 'ap-${webSiteName}-${environment}-${location}' : '')

output storageAccountName string = (!empty(storageAccountName) ? 'st${storageAccountName}${environment}' : '')

output b2cName string = (!empty(b2cName) ? '${b2cName}${environment}' : '')

output staticSiteName string = (!empty(staticSiteName) ? 'stapp-${staticSiteName}-${environment}-${location}' : '')

output billboardSiteName string = (!empty(billboardName) ? 'stapp-${billboardName}-${environment}-${location}' : '')
