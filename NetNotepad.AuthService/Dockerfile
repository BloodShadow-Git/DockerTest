FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS prebuild
WORKDIR /src
COPY NetNotepad.Contracts/*.csproj NetNotepad.Contracts/
COPY NetNotepad.AuthService/*.csproj NetNotepad.AuthService/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore NetNotepad.AuthService/NetNotepad.AuthService.csproj

COPY NetNotepad.Contracts/. NetNotepad.Contracts/
COPY NetNotepad.AuthService/. NetNotepad.AuthService/.

FROM prebuild AS migration
WORKDIR /src/NetNotepad.AuthService
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore NetNotepad.AuthService.csproj
RUN dotnet tool install --global dotnet-ef --version 10.*
ENV PATH="${PATH}:/root/.dotnet/tools"
ENTRYPOINT ["sh", "-c"]

FROM prebuild AS build
ARG BUILD_CONFIGURATION=Release
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish \
    NetNotepad.AuthService/NetNotepad.AuthService.csproj \
    -c ${BUILD_CONFIGURATION} \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/publish .
USER app
HEALTHCHECK --interval=10s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://127.0.0.1:8080/health || exit 1
ENTRYPOINT [ "dotnet", "NetNotepad.AuthService.dll" ]