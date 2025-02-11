@description('The name of the Cosmos DB account')
param cosmosDbAccountName string

@description('Database name')
param databaseName string

@description('Tags that will be applied to all resources')
param tags object = {}

resource cosmosdb 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' existing = {
  name: cosmosDbAccountName
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-02-15-preview' = {
  parent: cosmosdb
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
  }
  tags: tags
}


