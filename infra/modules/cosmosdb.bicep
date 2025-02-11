@description('The name of the Cosmos DB account')
param cosmosDbAccountName string

@description('Tags that will be applied to all resources')
param tags object = {}

@description('The location for deployment')
param location string = resourceGroup().location

resource cosmosdb 'Microsoft.DocumentDB/databaseAccounts@2024-02-15-preview' = {
  name: toLower(cosmosDbAccountName)
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    locations: [
      {
        locationName: location
        failoverPriority: 0
      }
    ]
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
    databaseAccountOfferType: 'Standard'
    disableKeyBasedMetadataWriteAccess: true
    enableFreeTier: false
  }
  tags: tags
}

output COSMOS_ENDPOINT string = cosmosdb.properties.documentEndpoint



