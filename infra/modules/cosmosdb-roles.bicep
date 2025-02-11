@description('The name of the Cosmos DB account')
param cosmosDbAccountName string

@description('Principal ID to assign the Cosmos DB role')
param principalId string

@description('Name of the role definition.')
param roleName string = 'Azure Cosmos DB for NoSQL Data Plane Owner'


resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' existing = {
  name: cosmosDbAccountName
}

resource definition 'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions@2024-05-15' = {
  name: guid(cosmosDbAccount.id, roleName)
  parent: cosmosDbAccount
  properties: {
    roleName: roleName
    type: 'CustomRole'
    assignableScopes: [
      cosmosDbAccount.id
    ]
    permissions: [
      {
        dataActions: [
          'Microsoft.DocumentDB/databaseAccounts/readMetadata'
          'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/*'
          'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/*'
        ]
      }
    ]
  }
}

resource assignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-05-15' = {
  name: guid(definition.id, principalId, cosmosDbAccount.id)
  parent: cosmosDbAccount
  properties: {
    principalId: principalId
    roleDefinitionId: definition.id
    scope: cosmosDbAccount.id
  }
}

