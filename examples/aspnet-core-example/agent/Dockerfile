FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /source
COPY HAProxy.StreamProcessingOffload.Agent/ ./HAProxy.StreamProcessingOffload.Agent/
COPY examples/aspnet-core-example/agent/ ./examples/aspnet-core-example/agent/
WORKDIR /source/examples/aspnet-core-example/agent
RUN dotnet restore
RUN dotnet build
RUN dotnet publish --configuration Debug -o /output


FROM mcr.microsoft.com/dotnet/aspnet:3.1
EXPOSE 12345
# RUN apt update && \
#     apt install -y unzip procps && \
#     curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg
WORKDIR /app
COPY --from=build /output .
ENTRYPOINT ["dotnet", "agent.dll"]