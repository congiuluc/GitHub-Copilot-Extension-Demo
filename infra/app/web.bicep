param name string
param location string = resourceGroup().location
param tags object = {}
param serviceName string = 'web'
param appCommandLine string = 'npm run start'
param applicationInsightsName string = ''
param appServicePlanId string
param managedIdentity bool = false
param scmDoBuildDuringDeployment bool = true
@secure()
param appSettings object = {}

module web '../core/host/appservice.bicep' = {
  name: '${name}-deployment'
  params: {
    name: name
    location: location
    appCommandLine: appCommandLine
    applicationInsightsName: applicationInsightsName
    appServicePlanId: appServicePlanId
    appSettings: appSettings
    runtimeName: 'node'
    runtimeVersion: '20-lts'
    runtimeNameAndVersion: 'NODE|20-lts'
    managedIdentity: managedIdentity
    scmDoBuildDuringDeployment: scmDoBuildDuringDeployment
    tags: union(tags, { 'azd-service-name': serviceName })
  }
}

output SERVICE_WEB_IDENTITY_PRINCIPAL_ID string = web.outputs.identityPrincipalId
output SERVICE_WEB_NAME string = web.outputs.name
output SERVICE_WEB_URI string = web.outputs.uri
