FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /source
COPY HAProxy.StreamProcessingOffload.Agent/ ./HAProxy.StreamProcessingOffload.Agent/
COPY examples/agent/ ./examples/agent/
WORKDIR /source/examples/agent
RUN dotnet restore
RUN dotnet publish --configuration Debug -o /output


FROM mcr.microsoft.com/dotnet/core/runtime:2.2
WORKDIR /app
COPY --from=build /output .
ENTRYPOINT ["dotnet", "Agent.dll"]