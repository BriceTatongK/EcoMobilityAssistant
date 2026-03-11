#!/bin/bash

# -------------------------------
# EcoMobilityAssistant CLI Runner
# -------------------------------
# Build and run the EcoMob.Cli project
# Usage: ./run.sh [options]
#
# Options:
#   -c, --config CONFIG    Build configuration (Debug/Release) [default: Release]
#   -s, --skip-clean       Skip the clean step
#   -v, --verbose          Enable verbose output
#   -h, --help            Show this help message
# -------------------------------

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default values
CONFIG="Release"
SKIP_CLEAN=false
VERBOSE=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--config)
            CONFIG="$2"
            shift 2
            ;;
        -s|--skip-clean)
            SKIP_CLEAN=true
            shift
            ;;
        -v|--verbose)
            VERBOSE=true
            shift
            ;;
        -h|--help)
            echo "EcoMobilityAssistant CLI Runner"
            echo ""
            echo "Usage: ./run.sh [options]"
            echo ""
            echo "Options:"
            echo "  -c, --config CONFIG    Build configuration (Debug/Release) [default: Release]"
            echo "  -s, --skip-clean       Skip the clean step"
            echo "  -v, --verbose          Enable verbose output"
            echo "  -h, --help            Show this help message"
            exit 0
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            echo "Use -h or --help for usage information."
            exit 1
            ;;
    esac
done

# Function to print colored output
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Pre-flight checks
print_info "Performing pre-flight checks..."

# Check if .NET CLI is installed
if ! command_exists dotnet; then
    print_error ".NET CLI is not installed or not in PATH."
    print_error "Please install .NET 8.0 or later from https://dotnet.microsoft.com/download"
    exit 1
fi

# Check .NET version (basic check)
DOTNET_VERSION=$(dotnet --version 2>/dev/null || echo "unknown")
print_info ".NET version: $DOTNET_VERSION"

# Navigate to the solution root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION_ROOT="$SCRIPT_DIR/.."
cd "$SOLUTION_ROOT" || {
    print_error "Failed to navigate to solution root: $SOLUTION_ROOT"
    exit 1
}

print_info "Working directory: $(pwd)"

# Check if solution file exists
if [[ ! -f "EcoMobilityAssistant.sln" ]]; then
    print_error "Solution file 'EcoMobilityAssistant.sln' not found in current directory."
    exit 1
fi

# Check if CLI project exists
CLI_PROJECT="./src/UI/EcoMob.Cli/EcoMob.Cli.csproj"
if [[ ! -f "$CLI_PROJECT" ]]; then
    print_error "CLI project file not found: $CLI_PROJECT"
    exit 1
fi

print_success "Pre-flight checks completed."

# Clean step (optional)
if [[ "$SKIP_CLEAN" != true ]]; then
    print_info "Cleaning previous builds..."
    if [[ "$VERBOSE" == true ]]; then
        dotnet clean "$CLI_PROJECT"
    else
        dotnet clean "$CLI_PROJECT" >/dev/null 2>&1
    fi

    if [[ $? -eq 0 ]]; then
        print_success "Clean completed."
    else
        print_warning "Clean failed, but continuing..."
    fi
else
    print_info "Skipping clean step as requested."
fi

# Build the CLI project
print_info "Building EcoMob.Cli project in $CONFIG configuration..."
BUILD_CMD="dotnet build $CLI_PROJECT -c $CONFIG"
if [[ "$VERBOSE" != true ]]; then
    BUILD_CMD="$BUILD_CMD --verbosity quiet"
fi

if eval "$BUILD_CMD"; then
    print_success "Build completed successfully."
else
    print_error "Build failed."
    exit 1
fi

# Run the CLI
print_info "Starting EcoMobilityAssistant CLI..."
print_info "Press Ctrl+C to stop the application."
echo ""

RUN_CMD="dotnet run --project $CLI_PROJECT --configuration $CONFIG"
if [[ "$VERBOSE" != true ]]; then
    RUN_CMD="$RUN_CMD --verbosity quiet"
fi

# Execute the run command
exec $RUN_CMD