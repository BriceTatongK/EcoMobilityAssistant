# Contributing to EcoMobilityAssistant

Thank you for your interest in contributing to EcoMobilityAssistant! We welcome contributions from the community to help improve this AI-powered sustainable mobility assistant.

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/your-username/EcoMobilityAssistant.git
   cd EcoMobilityAssistant
   ```
3. **Set up the development environment**:
   - Ensure you have .NET 8.0 or later installed
   - Install Docker and Docker Compose for full testing
   - Run `dotnet build EcoMobilityAssistant.sln` to verify everything builds

## Development Setup

### Prerequisites

- .NET 8.0+
- Docker and Docker Compose
- Git

### Building and Running

- Build: `dotnet build EcoMobilityAssistant.sln`
- Run tests: `dotnet test EcoMobilityAssistant.sln`
- Run CLI locally: `./run.sh` (Linux/Mac) or `run.sh` (Windows)
- Run full system: `docker-compose up --build`

## Coding Standards

- Follow C# coding conventions and use consistent naming
- Write clear, concise commit messages
- Add XML documentation comments to public APIs
- Ensure code is thread-safe where applicable
- Use dependency injection for services

## Testing

- Write unit tests for new functionality
- Ensure all tests pass before submitting PRs
- Test both locally and with Docker Compose setup
- Include integration tests for AI agent interactions

## Submitting Changes

1. **Create a feature branch**:

   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** and ensure:
   - Code builds successfully
   - All tests pass
   - No linting errors

3. **Commit your changes**:

   ```bash
   git commit -m "Add: Brief description of changes"
   ```

4. **Push to your fork**:

   ```bash
   git push origin feature/your-feature-name
   ```

5. **Create a Pull Request** on GitHub with:
   - Clear title and description
   - Reference any related issues
   - Screenshots/demo for UI changes

## Pull Request Guidelines

- Keep PRs focused on a single feature or bug fix
- Update documentation if needed
- Ensure CI/CD checks pass
- Request review from maintainers

## Reporting Issues

- Use GitHub Issues to report bugs or request features
- Provide detailed steps to reproduce bugs
- Include environment details (.NET version, OS, etc.)

## Code of Conduct

Please be respectful and inclusive in all interactions. We follow a code of conduct to ensure a positive community environment.

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.

Thank you for contributing to EcoMobilityAssistant! 🚀
