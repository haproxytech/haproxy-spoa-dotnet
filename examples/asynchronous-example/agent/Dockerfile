FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /source
COPY HAProxy.StreamProcessingOffload.Agent/ ./HAProxy.StreamProcessingOffload.Agent/
COPY examples/asynchronous-example/agent/ ./examples/asynchronous-example/agent/
WORKDIR /source/examples/asynchronous-example/agent
RUN dotnet build
RUN dotnet publish --configuration Debug -o /output


FROM mcr.microsoft.com/dotnet/runtime:3.1
RUN apt update && \
    apt install -y unzip procps && \
    curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg
WORKDIR /app
COPY --from=build /output .
ENTRYPOINT ["dotnet", "Agent.dll"]