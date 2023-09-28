@minLength(5)
@maxLength(40)
@description('Name of the Azure Website.')
param webSiteName string = uniqueString(resourceGroup().id)

@description('Provide a App Service Plan Id.')
param appServicePlanId string

@description('The subnet name')
param subnetName string
@description('The subnet name')
param vnetName string

@description('The managed identity client id')
param managedIdentityClientId string
@description('The managed identity resource id')
param managedIdentityResourceId string

@description('The sql server\'s FQDN')
param sqlFQDN string
@description('The sql database name')
param sqlDatabaseName string

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

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2022-07-01' existing = {
    name: subnetName
}

var isProd = environment == 'prod'

resource appService 'Microsoft.Web/sites@2022-03-01' = {
    name: webSiteName
    location: location
    kind: 'linux,api'
    properties: {
        serverFarmId: appServicePlanId
        siteConfig: {
            vnetName: vnetName
            linuxFxVersion: 'DOTNETCORE|7.0'
            connectionStrings: [
                {
                    name: 'DefaultConnection'
                    connectionString: 'Data Source=tpc:${sqlFQDN},1433;Initial Catalog=${sqlDatabaseName};Authentication=Active Directory Managed Identity;Encrypt=True;Connection Timeout=60;User Id=${managedIdentityClientId}'
                    type: 'SQLAzure'
                }
            ]
            healthCheckPath: isProd ? '/health' : ''
            alwaysOn: isProd
        }
        reserved: true
        keyVaultReferenceIdentity: managedIdentityResourceId
        virtualNetworkSubnetId: subnet.id
    }
    identity: {
        type: 'UserAssigned'
        userAssignedIdentities: {
            '${managedIdentityResourceId}': {}
        }
    }
    tags: {
        Application: tagApplication
        CreatedOnDate: created
        Environment: environment
    }
}

// resource appServiceConfig 'Microsoft.Web/sites/config@2022-03-01' = {
//   parent: appService
//   name: 'web'
//   properties:{
//     vnetName: 'abdc3be4-3255-4a50-a49b-01f9b6bed538_default'
//     vnetRouteAllEnabled: true
//     vnetPrivatePortsCount: 0
//   }
// }

// resource vnetConnection 'Microsoft.Web/sites/virtualNetworkConnections@2022-03-01' = {
//   parent: appService
//   name: 'defaultVnet'
//   properties: {
//     vnetResourceId: subnet.id
//     isSwift: true
//   }
// }
