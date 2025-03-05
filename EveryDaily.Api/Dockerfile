FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EveryDaily.Api/EveryDaily.Api.csproj", "EveryDaily.Api/"]
COPY ["EveryDaily.Application/EveryDaily.Application.csproj", "EveryDaily.Application/"]
COPY ["EveryDaily.Core/EveryDaily.Core.csproj", "EveryDaily.Core/"]
COPY ["EveryDaily.Domain/EveryDaily.Domain.csproj", "EveryDaily.Domain/"]
COPY ["EveryDaily.Persistence/EveryDaily.Persistence.csproj", "EveryDaily.Persistence/"]
RUN dotnet restore "EveryDaily.Api/EveryDaily.Api.csproj"
COPY . .
WORKDIR "/src/EveryDaily.Api"
RUN dotnet build "EveryDaily.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EveryDaily.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EveryDaily.Api.dll"]
