FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0.103-alpine3.23 AS build
ARG TARGETARCH

WORKDIR /app

# Copy fsproj and restore as distinct layers
COPY src/Exercism.Representers.FSharp/Exercism.Representers.FSharp.fsproj ./
RUN dotnet restore -a $TARGETARCH

# Copy everything else and build
COPY src/Exercism.Representers.FSharp/ ./
RUN dotnet publish -a $TARGETARCH --no-restore --self-contained true --output /opt/representer

# Build runtime image
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/runtime-deps:10.0.3-alpine3.23 AS runtime
WORKDIR /opt/representer

COPY --from=build /opt/representer/ .
COPY --from=build /usr/local/bin/ /usr/local/bin/

COPY bin/run.sh /opt/representer/bin/

ENTRYPOINT ["sh", "/opt/representer/bin/run.sh"]
