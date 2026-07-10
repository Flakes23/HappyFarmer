# Build context PHẢI là thư mục HappyFarmer_Backend/ (chứa .slnx) vì service này tham chiếu
# chéo tới src/Shared/HappyFarmer.Shared.Contracts — build: docker build -f <path này> .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Gateway/HappyFarmer.ApiGateway/HappyFarmer.ApiGateway.csproj", "src/Gateway/HappyFarmer.ApiGateway/"]
COPY ["src/Shared/HappyFarmer.Shared.Contracts/HappyFarmer.Shared.Contracts.csproj", "src/Shared/HappyFarmer.Shared.Contracts/"]
RUN dotnet restore "src/Gateway/HappyFarmer.ApiGateway/HappyFarmer.ApiGateway.csproj"

COPY src/Gateway/HappyFarmer.ApiGateway/ src/Gateway/HappyFarmer.ApiGateway/
COPY src/Shared/HappyFarmer.Shared.Contracts/ src/Shared/HappyFarmer.Shared.Contracts/

WORKDIR /src/src/Gateway/HappyFarmer.ApiGateway
RUN dotnet publish "HappyFarmer.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HappyFarmer.ApiGateway.dll"]
