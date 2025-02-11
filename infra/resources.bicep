@description('The location used for all deployed resources')
param location string = resourceGroup().location

@description('Principal Id')
param principalId string

@description('Tags that will be applied to all resources')
param tags object = {}

var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = uniqueString(subscription().id, resourceGroup().id, location)

var appServicePlanName = '${abbrs.webServerFarms}${resourceToken}'
var appServiceNameApi = '${abbrs.webSitesAppService}${resourceToken}-api'
var appServiceName = '${abbrs.webSitesAppService}${resourceToken}-app'
var cosmosDbAccountName = '${abbrs.documentDBDatabaseAccounts}${resourceToken}'
// Monitor application with Azure Monitor
module monitoring 'br/public:avm/ptn/azd/monitoring:0.1.0' = {
  name: 'monitoring'
  params: {
    logAnalyticsName: '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
    applicationInsightsName: '${abbrs.insightsComponents}${resourceToken}'
    applicationInsightsDashboardName: '${abbrs.portalDashboards}${resourceToken}'
    location: location
    tags: tags
  }
}

module appserviceplan 'core/host/appserviceplan.bicep' = {
  name: 'appserviceplan'
  params: {
    name: !empty(appServicePlanName) ? appServicePlanName : '${abbrs.webServerFarms}${resourceToken}'
    location: location
    tags: tags
    sku: {
      name: 'B3'
    }
  }
}
/*
module appserviceplan 'br/public:avm/res/web/serverfarm:0.4.1' = {
  name: 'appserviceplan'

  params: {
    name: appServicePlanName
    location: location
    skuName: 'B1'
    kind: 'app'

    tags: tags
  }
}
*/

module cosmosdb 'modules/cosmosdb.bicep' = {
  name: 'cosmosdb'
  params: {
    cosmosDbAccountName: cosmosDbAccountName
    tags: tags
    location: location
  }
}
/*
module appserviceApi 'br/public:avm/res/web/site:0.13.1' = {
  name: 'webapp-api'
  params: {
    name: appServiceNameApi
    location: location
    serverFarmResourceId: appserviceplan.outputs.id
    appInsightResourceId: monitoring.outputs.applicationInsightsResourceId
    kind: 'app,linux'
    managedIdentities: {
      systemAssigned: true
    }
    appSettingsKeyValuePairs: {
      APPINSIGHTS_INSTRUMENTATIONKEY: monitoring.outputs.applicationInsightsInstrumentationKey
      COSMOS_ENDPOINT: cosmosdb.outputs.COSMOS_ENDPOINT
    }
    tags: union(tags, { 'azd-service-name': 'todo-api' })
  }
}
*/
module appserviceApi 'app/api.bicep' = {
  name: 'webapp-api'
  params: {
    name: appServiceNameApi
    location: location
    appServicePlanId: appserviceplan.outputs.id
    applicationInsightsName: monitoring.outputs.applicationInsightsName
    serviceName: 'todo-api'
    appSettings: {
      COSMOS_ENDPOINT: cosmosdb.outputs.COSMOS_ENDPOINT
      APPINSIGHTS_INSTRUMENTATIONKEY: monitoring.outputs.applicationInsightsInstrumentationKey
      OPENWEATHERMAP_API_KEY: '1234567890'
    }
    tags: tags
  }
}

module appserviceApp 'app/web.bicep' = {
  name: 'webapp-copilot'
  params: {
    name: appServiceName
    location: location
    appServicePlanId: appserviceplan.outputs.id
    applicationInsightsName: monitoring.outputs.applicationInsightsName
    serviceName: 'copilot-app'
    appSettings: {
      APPINSIGHTS_INSTRUMENTATIONKEY: monitoring.outputs.applicationInsightsInstrumentationKey
      SCM_DO_BUILD_DURING_DEPLOYMENT: 'false'
      API_BASE_URL: appserviceApi.outputs.SERVICE_API_URI
      OPENWEATHERMAP_API_KEY: '1234567890'
    }
    tags: tags
  }
}
/*
module appserviceApp 'br/public:avm/res/web/site:0.13.1' = {
  name: 'webapp-copilot'
  params: {
    name: appServiceName
    location: location
    serverFarmResourceId: appserviceplan.outputs.resourceId
    appInsightResourceId: monitoring.outputs.applicationInsightsResourceId
    kind: 'app,linux'
    managedIdentities: {
      systemAssigned: true
    }
    appSettingsKeyValuePairs: {
      APPINSIGHTS_INSTRUMENTATIONKEY: monitoring.outputs.applicationInsightsInstrumentationKey
      SCM_DO_BUILD_DURING_DEPLOYMENT: 'true'
      
    }
    tags: union(tags, { 'azd-service-name': 'copilot-app' })
  }
}
*/

module cosmosdbDb 'modules/cosmosdb-db.bicep' = {
  name: 'cosmosdb-Db'
  dependsOn: [
    cosmosdb
  ]
  params: {
    cosmosDbAccountName: cosmosDbAccountName
    databaseName: 'TodoDb'
    tags: tags
  }
}

module cosmosdbRoles 'modules/cosmosdb-roles.bicep' = {
  name: 'cosmosdb-roles'
  params: {
    cosmosDbAccountName: cosmosDbAccountName
    principalId: principalId
  }
}


var principalIdAppService = appserviceApi.outputs.SERVICE_API_IDENTITY_PRINCIPAL_ID ?? ''
module cosmosdbRolesApp 'modules/cosmosdb-roles.bicep' = {
  name: 'cosmosdb-roles-app'
  params: {
    cosmosDbAccountName: cosmosDbAccountName
    principalId: principalIdAppService
  }
}



output WEBAPI_ENDPOINT string = appserviceApi.outputs.SERVICE_API_URI
output WEBAPP_ENDPOINT string = appserviceApp.outputs.SERVICE_WEB_URI
