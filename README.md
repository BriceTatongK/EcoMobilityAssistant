Eco Mobility Assistant is an AI-powered service that helps citizens choose sustainable transport solutions and provides general information about mobility facilities available in the Open Data Hub.

## Running the Server and CLI

Follow these steps to launch the MCP Server using Docker, then run the CLI client.

---

### 1. Build and run the MCP Server with Docker

From the root of the repository, navigate to the web app folder:

```bash
cd src/mcpServer/src/EcoMob.McpSseServer.WebApp
```

Then, build the container image:

```bash
docker build -t eco-mcpserver .
```

Run the container:

```bash
docker run -d -p 5000:80 --name eco-mcpserver eco-mcpserver
```

The server will now be available at http://localhost:5000.

### 2. run the interface cli for testing

```bash
chmod +x run.sh
./run.sh
```
