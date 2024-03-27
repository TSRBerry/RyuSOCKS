FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Install NativeAOT build prerequisites
RUN apk add --no-cache clang gcc musl-dev zlib-dev
COPY ["RyuSocks.Generator/RyuSocks.Generator.csproj", "RyuSocks.Generator/"]
COPY ["RyuSocks/RyuSocks.csproj", "RyuSocks/"]
RUN dotnet restore "RyuSocks/RyuSocks.csproj"
COPY ["RyuSocks", "RyuSocks/"]
COPY ["RyuSocks.Generator", "RyuSocks.Generator/"]
WORKDIR "/src/RyuSocks"
RUN dotnet build "RyuSocks.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RyuSocks.csproj" --ucr -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM alpine:latest
WORKDIR /app
COPY --from=publish /app/publish .
RUN chmod +x ./RyuSocks.so
ENTRYPOINT ["./RyuSocks.so"]
