FROM mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.10 AS build
WORKDIR /app

COPY generate.sh /opt/representer/bin/

# Download exercism tooling webserver
RUN wget -P /usr/local/bin https://github.com/exercism/local-tooling-webserver/releases/latest/download/exercism_local_tooling_webserver && \
    chmod +x /usr/local/bin/exercism_local_tooling_webserver

# Copy fsproj and restore as distinct layers
COPY src/Exercism.Representers.FSharp/Exercism.Representers.FSharp.fsproj ./
RUN dotnet restore -r linux-musl-x64

# Copy everything else and build
COPY src/Exercism.Representers.FSharp/ ./
RUN dotnet publish -r linux-musl-x64 -c Release -o /opt/representer --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1.2-alpine3.10
WORKDIR /opt/representer

COPY --from=build /opt/representer/ .
COPY --from=build /usr/local/bin/ /usr/local/bin/

ENTRYPOINT ["sh", "/opt/representer/bin/generate.sh"]
