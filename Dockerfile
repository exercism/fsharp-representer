FROM mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.10 AS build-env
WORKDIR /app

COPY generate.sh /opt/representer/bin/

# Copy fsproj and restore as distinct layers
COPY src/Exercism.Representers.FSharp/Exercism.Representers.FSharp.fsproj ./
RUN dotnet restore

# Copy everything else and build
COPY src/Exercism.Representers.FSharp/ ./
RUN dotnet publish -c Release -r linux-musl-x64 -o /opt/representer

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1.2-alpine3.10
WORKDIR /opt/representer
COPY --from=build-env /opt/representer/ .
ENTRYPOINT ["sh", "/opt/representer/bin/generate.sh"]
