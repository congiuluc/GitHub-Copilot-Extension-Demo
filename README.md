# GitHub Copilot Extension Demo

This is a GitHub Copilot Extension built in TypeScript.

## Architecture

The project consists of two main components:

- **Backend API**: A .NET 9.0 Web API with:
  - Azure Cosmos DB integration
  - OpenAPI/Swagger documentation (NSwag)
  - Application Insights telemetry
  
- **Frontend Application**: A TypeScript/Node.js application with:
  - Express.js server
  - OpenAI integration via `@copilot-extensions/preview-sdk`
  - Axios for HTTP requests
  - TypeScript for type safety

## Infrastructure

The application infrastructure is defined using Azure Bicep and includes:

- Azure App Service for hosting
- Azure Cosmos DB for data storage
- Azure Application Insights for monitoring

## Features

- Todo management API with Cosmos DB backend
- Weather data API integration
- GitHub user integration
- Monitoring and telemetry
- Infrastructure as Code with Azure Bicep

## Prerequisites

- Node.js 18.x or later
- .NET 9.0 SDK
- Azure Developer CLI (azd)
- Azure subscription
- Visual Studio 2022 or VS Code

## Getting Started

1. **Clone the repository**

```bash
git clone <repository-url>
cd <repository-name>
```

2. **Set up the API**

```bash
cd src/api
dotnet build
dotnet run
```

The API will be available at `https://localhost:7295`

3. **Set up the Frontend**

```bash
cd src/app
npm install
npm run dev
```

The frontend development server will start using tsx for TypeScript execution.
The frontend will be available at `http://localhost:3000`

## Configuration

### API Configuration
Configure the following in `appsettings.json`:

- `COSMOS_ENDPOINT`: Your Azure Cosmos DB endpoint
- `OPENWEATHERMAP_API_KEY`: Your OpenWeatherMap API key (https://openweathermap.org/)
- `ApplicationInsights`: Application Insights instrumentation key

### Frontend Configuration
The frontend uses the following npm packages:
- `@copilot-extensions/preview-sdk`: For Copilot integration
- `axios`: For HTTP requests
- `express`: For the web server
- `openai`: For OpenAI API integration

## Infrastructure Deployment

The project uses Azure Developer CLI (azd) for simplified deployment and management. The infrastructure is defined using Azure Bicep templates.

### Prerequisites
1. Install Azure Developer CLI:
```bash
winget install Microsoft.AzureDeveloperCLI
# or
brew install azure/azd/azd
```

2. Login to Azure:
```bash
azd auth login
```

### Deploy the Application

- Provision infrastructure and deploy the application:
```bash
azd up
```

This single command will:
- Create required Azure resources using Bicep templates
- Build both the API and frontend applications
- Deploy the applications to Azure
- Set up necessary configurations and connections

### Additional Commands

- Deploy infrastructure changes:
```bash
azd provision
```

- Deploy application updates:
```bash
azd deploy
```

- Show deployed service endpoints:
```bash
azd show endpoints
```

- Monitor the application:
```bash
azd monitor
```

- Clean up resources:
```bash
azd down
```

### Core Infrastructure
- `/infra/core/` - Core infrastructure components:
  - AI and Cognitive Services
  - Database (Cosmos DB, MySQL, PostgreSQL, SQL Server)
  - API Management Gateway
  - Hosting (App Service, AKS, Container Apps)
  - Monitoring (Application Insights)
  - Networking (CDN)
  - Security (Key Vault)
  - Storage

### Application Infrastructure
- `/infra/app/` - Application-specific resources:
  - `api.bicep` - API infrastructure
  - `web.bicep` - Web app infrastructure
  - `db.bicep` - Database infrastructure
  - `apim-api.bicep` - API Management configuration
  - `apim-api-policy.xml` - API Management policies

## Project Structure

```
├── azure.yaml            # Azure deployment configuration
├── infra/               # Infrastructure as Code (Bicep)
│   ├── app/            # Application-specific resources
│   ├── core/           # Core infrastructure components
│   └── modules/        # Reusable Bicep modules
├── src/
│   ├── api/           # .NET Backend API
│   │   ├── Endpoints/  # API endpoints
│   │   ├── Models/     # Data models
│   │   ├── Services/   # Business logic
│   │   └── Middleware/ # Custom middleware
│   └── app/           # TypeScript Frontend
       ├── src/        # Source code
       └── dist/       # Compiled output
```

## Development

### Backend API
- .NET 9.0 Web API with minimal API endpoints
- Entity Framework Core with Cosmos DB
- Custom middleware for user ID handling
- Models for Todo items, Weather data, and GitHub integration
- NSwag for OpenAPI documentation

### Frontend
- TypeScript with strict configuration
- Node.js with Express
- ES2020 target
- Source map support for debugging
- Docker containerization support

## Contributing

1. Fork the repository
2. Create a feature branch
3. Submit a pull request

## License

MIT License - see LICENSE file for details