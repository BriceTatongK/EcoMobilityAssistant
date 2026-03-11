# EcoMobilityAssistant

EcoMobilityAssistant is an AI-powered service that helps citizens choose sustainable transport solutions and provides general information about mobility facilities available in the Open Data Hub. The project leverages AI agents to assist users in making informed decisions about eco-friendly transportation options, promoting sustainable mobility practices.

## Project Goal

The primary goal of EcoMobilityAssistant is to empower individuals and communities to adopt sustainable transportation habits by providing intelligent, data-driven recommendations. By integrating with Open Data Hub mobility facilities and utilizing advanced AI reasoning agents, the system offers personalized transport suggestions that consider environmental impact, efficiency, and user preferences.

## Architecture and Modules Structure

The project follows Clean Architecture principles, organized into several layers and modules:

### Core Modules

- **Contracts** (`src/Contracts/EcoMob.Contracts`): Defines shared interfaces, enums, and models used across the application. This includes service contracts like `IIntentAgent`, `IReasoningAgent`, and `IValidatorAgent`, as well as data models for intent context and validation results.

- **Core** (`src/Core/EcoMob.Core`): Contains the core business logic and domain services. The main service here is `EcoMobilityService`, which orchestrates the eco-mobility assistance functionality.

- **Infrastructure** (`src/Infrastructure/EcoMob.Infra`): Handles external dependencies and infrastructure concerns. This includes:
  - AI agents: `ContextValidatorAgent`, `IntentClassifierAgent`, `ReasoningAgent`
  - Logging extensions
  - Configuration settings
  - Helper utilities

- **UI** (`src/UI/EcoMob.Cli`): Provides the command-line interface for interacting with the EcoMobilityAssistant service.

### MCP Server Modules

The MCP (Model Context Protocol) Server components are located under `src/McpServer/src/`:

- **EcoMob.McpServer.Contracts**: Server-specific contracts and interfaces
- **EcoMob.McpServer.Core**: Core server logic and configuration
- **EcoMob.McpServer.Infra**: Server infrastructure components
- **EcoMob.McpServer.Entry**: Server entry point
- **EcoMob.McpSseServer.WebApp**: Web application for the MCP server with SSE (Server-Sent Events) support

### Testing

- **Tests** (`src/Tests/EcoMob.Tests`): Contains unit tests and architectural tests to ensure code quality and adherence to design principles.

## Prerequisites

- .NET 8.0 or later
- Docker and Docker Compose (for containerized deployment)
- Git

## Running the Project

### Option 1: Local Development (CLI)

To run the EcoMobilityAssistant CLI locally:

1. Ensure you have .NET 8.0+ installed
2. Make the run script executable (Linux/Mac):
   ```bash
   chmod +x run.sh
   ```
3. Execute the script:
   ```bash
   ./run.sh
   ```

The script will:

- Clean previous builds
- Build the CLI project in Release configuration
- Run the EcoMobilityAssistant CLI

### Option 2: Docker Compose (Full Server Setup)

To run the complete system with MCP server and Redis using Docker Compose:

1. Ensure Docker and Docker Compose are installed
2. From the repository root, run:
   ```bash
   docker-compose up --build
   ```

This will:

- Build and start a Redis container (port 6379)
- Build and start the MCP server container (port 5000)
- The MCP server will be available at http://localhost:5000

To run in detached mode:

```bash
docker-compose up -d --build
```

To stop the services:

```bash
docker-compose down
```

### Manual Docker Build (Alternative)

If you prefer to build manually:

1. Navigate to the web app directory:

   ```bash
   cd src/McpServer/src/EcoMob.McpSseServer.WebApp
   ```

2. Build the Docker image:

   ```bash
   docker build -t eco-mcpserver .
   ```

3. Run the container:
   ```bash
   docker run -d -p 5000:80 --name eco-mcpserver eco-mcpserver
   ```

## Development

### Building the Solution

To build all projects:

```bash
dotnet build EcoMobilityAssistant.sln
```

### Running Tests

```bash
dotnet test EcoMobilityAssistant.sln
```

### Code Structure

The project follows Clean Architecture with clear separation of concerns:

- **Contracts**: Define what the system does
- **Core**: Implement the business logic
- **Infrastructure**: Handle external concerns (AI agents, logging, etc.)
- **UI**: Provide user interfaces (CLI, Web)

The MCP Server extends this architecture to provide AI-powered mobility assistance through a web interface.

## Contributing

Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on how to contribute to this project.

## License

[Add license information here]
