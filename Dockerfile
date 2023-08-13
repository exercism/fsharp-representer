FROM mcr.microsoft.com/dotnet/sdk:6.0.401-alpine3.16-amd64 AS build
WORKDIR /app

# Copy fsproj and restore as distinct layers
COPY src/Exercism.Representers.FSharp/Exercism.Representers.FSharp.fsproj ./
RUN dotnet restore -r linux-musl-x64

# Copy everything else and build
COPY src/Exercism.Representers.FSharp/ ./
RUN dotnet publish -r linux-musl-x64 -c Release -o /opt/representer --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.9-alpine3.16-amd64
WORKDIR /opt/representer

COPY --from=build /opt/representer/ .
COPY --from=build /usr/local/bin/ /usr/local/bin/

COPY bin/run.sh /opt/representer/bin/

ENTRYPOINT ["sh", "/opt/representer/bin/run.sh"]
