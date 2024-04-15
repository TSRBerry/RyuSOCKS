FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RyuSocks.Generator/RyuSocks.Generator.csproj", "RyuSocks.Generator/"]
COPY ["RyuSocks/RyuSocks.csproj", "RyuSocks/"]
COPY ["TestProject/TestProject.csproj", "TestProject/"]
RUN dotnet restore "RyuSocks/RyuSocks.csproj"
COPY ["RyuSocks", "RyuSocks/"]
COPY ["RyuSocks.Generator", "RyuSocks.Generator/"]
COPY ["TestProject", "TestProject/"]
WORKDIR "/src/TestProject"
RUN dotnet build "TestProject.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TestProject.csproj" --ucr -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestProject.dll"]
