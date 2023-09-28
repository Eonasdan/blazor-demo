targetScope = 'subscription'
param resourceGroupName string
param location string
param environment string
param tagApplication string

param baseTime string = utcNow('d')

var rgName = 'rg-${ resourceGroupName }-${ environment }-${ location }'

resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: rgName
  location: location
  tags: {
    Application: tagApplication
    Environment: environment
    CreatedOnDate: baseTime
  }
}
