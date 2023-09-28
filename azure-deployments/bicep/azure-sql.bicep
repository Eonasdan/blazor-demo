param serverName string
param dbName string

@description('The managed identity resource id')
param managedIdentityResourceId string

param subnetName string

@description('Enter the environment this resource be in (dev, stage, prod).')
@allowed([
  'dev'
  'stage'
  'prod'
])
param environment string
param tagApplication string

param location string = resourceGroup().location
param baseTime string = utcNow('d')

resource sqlServer 'Microsoft.Sql/servers@2022-08-01-preview' = {
  name: serverName
  location: location
  tags: {
    Application: tagApplication
    Environment: environment
    CreatedOnDate: baseTime
  }
  properties: {
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    restrictOutboundNetworkAccess: 'Disabled'
    primaryUserAssignedIdentityId: managedIdentityResourceId
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'Group'
      login: 'db-admin'
      sid: '08cb0808-1997-4a62-9d40-ff3d5fdb77f8' //AAD group object id
      tenantId: tenant().tenantId
      azureADOnlyAuthentication: true
    }
  }
}

resource database 'Microsoft.Sql/servers/databases@2022-08-01-preview' = {
  parent: sqlServer
  name: dbName
  location: location
  tags: {
    Application: tagApplication
    Environment: environment
    CreatedOnDate: baseTime
  }
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Geo'
    isLedgerOn: false
    availabilityZone: 'NoPreference'
  }
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2022-07-01' existing = {
  name: subnetName
}

resource vnetRule 'Microsoft.Sql/servers/virtualNetworkRules@2022-08-01-preview' = {
  parent: sqlServer
  name: 'vnetRule'
  properties: {
    virtualNetworkSubnetId: subnet.id
    ignoreMissingVnetServiceEndpoint: false
  }
}

output fullyQualifiedDomainName string = sqlServer.properties.fullyQualifiedDomainName
