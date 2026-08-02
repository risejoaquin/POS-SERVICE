FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN if [ -f "PosServer/PosServer.csproj" ]; then \
        echo "Found in PosServer/"; \
        dotnet restore "PosServer/PosServer.csproj"; \
        dotnet build "PosServer/PosServer.csproj" -c Release -o /app/build; \
        dotnet publish "PosServer/PosServer.csproj" -c Release -o /app/publish /p:UseAppHost=false; \
    elif [ -f "PosServer.csproj" ]; then \
        echo "Found in root"; \
        dotnet restore "PosServer.csproj"; \
        dotnet build "PosServer.csproj" -c Release -o /app/build; \
        dotnet publish "PosServer.csproj" -c Release -o /app/publish /p:UseAppHost=false; \
    else \
        echo "Could not find PosServer.csproj"; \
        ls -la; \
        exit 1; \
    fi

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PosServer.dll"]
