param vnetName string
param deployVnet bool

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

resource vnet 'Microsoft.Network/virtualNetworks@2022-07-01' = if(deployVnet) {
  name: vnetName
  location: location
  tags: {
    Application: tagApplication
    Environment: environment
    CreatedOnDate: baseTime
  }
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: []
    virtualNetworkPeerings: []
    enableDdosProtection: false
  }
}

var subnetName = '${vnetName}/default'

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2022-07-01' = {
  name: subnetName
  properties: {
    addressPrefix: '10.0.0.0/24'
    serviceEndpoints: [
      {
        service: 'Microsoft.Sql'
        locations: [
          location
        ]
      }
    ]
    delegations: [
      {
        name: 'delegation'
        properties: {
          serviceName: 'Microsoft.Web/serverfarms'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets/delegations'
      }
    ]
    privateEndpointNetworkPolicies: 'Disabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    vnet
  ]
}

output vnetName string = vnetName
output subnetName string = subnetName
