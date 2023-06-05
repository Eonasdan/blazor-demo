param location string = resourceGroup().location
param siteName string
param created string = utcNow('MM/dd/yyyy')
param skuName string = 'Free'
param skuTier string = 'Free'

@description('Enter the environment this resource be in (dev, stage, prod).')
@allowed([
  'dev'
  'stage'
  'prod'
])
param environment string

var fullSiteName = 'stapp-${siteName}-${environment}-${location}'

resource staticSite 'Microsoft.Web/staticSites@2022-03-01' = {
  name: fullSiteName
  location: location
  tags: {
    CreatedOnDate: created
    Environment: environment
  }
  sku: {
    name: skuName
    tier: skuTier
  }
  properties: {
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
    provider: 'DevOps'
    enterpriseGradeCdnStatus: 'Disabled'
  }
}

@description('The Static Site resource name')
output staticSiteName string = fullSiteName
