param siteName string
param skuName string = 'Free'
param skuTier string = 'Free'

@description('Provide the name of the associated application.')
param tagApplication string

@description('Enter the environment this resource be in (dev, stage, prod).')
@allowed([
  'dev'
  'stage'
  'prod'
])
param environment string

param created string = utcNow('d')
param location string = resourceGroup().location

resource staticSite 'Microsoft.Web/staticSites@2022-03-01' = {
  name: siteName
  location: location
  tags: {
    Application: tagApplication
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
