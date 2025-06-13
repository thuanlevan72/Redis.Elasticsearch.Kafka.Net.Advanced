FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TodoApp.ReadApi/TodoApp.ReadApi.csproj", "TodoApp.ReadApi/"]
COPY ["TodoApp.Domain/TodoApp.Domain.csproj", "TodoApp.Domain/"]
COPY ["TodoApp.Infrastructure/TodoApp.Infrastructure.csproj", "TodoApp.Infrastructure/"]
COPY ["TodoApp.Application/TodoApp.Application.csproj", "TodoApp.Application/"]
RUN dotnet restore "TodoApp.ReadApi/TodoApp.ReadApi.csproj"
COPY . .
WORKDIR "/src/TodoApp.ReadApi"
RUN dotnet build "TodoApp.ReadApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TodoApp.ReadApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoApp.ReadApi.dll"]
