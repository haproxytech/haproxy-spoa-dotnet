# Examples

This project is a .NET Core console app, which was created with the command:

```
dotnet new console
```

Build an run the Docker Compose file:

```
docker-compose build
docker-compose up -d
```

View the web app at http://localhost:8000.

## Debug with VS Code

To debug the agent program running in a Docker container, add the following to Visual
Studio Code's *launch.json* file:

```
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Attach",
            "type": "coreclr",
            "request": "attach",
            "processId": "${command:pickRemoteProcess}",
            "cwd": "${workspaceRoot}",
            "pipeTransport": {
                "pipeProgram": "docker",
                "pipeArgs": [ "exec", "-i", "agent" ],
                "debuggerPath": "/vsdbg/vsdbg",
                "quoteArgs": false
            },
            "sourceFileMap": {
                "/source/examples/asynchronous-example/agent": "${workspaceRoot}/examples/asynchronous-example/agent",
                "/source/HAProxy.StreamProcessingOffload.Agent": "${workspaceRoot}/HAProxy.StreamProcessingOffload.Agent"
            }
        }
    ]
}
```

Then when you debug, you can select the *agent* process in the Docker container.