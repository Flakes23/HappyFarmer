# Build context PHẢI là thư mục HappyFarmer_Backend/ (chứa .slnx) vì service này tham chiếu
# chéo tới src/Shared/HappyFarmer.Shared.Contracts — build: docker build -f <path này> .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Services/MarketPriceService/HappyFarmer.MarketPriceService.Api/HappyFarmer.MarketPriceService.Api.csproj", "src/Services/MarketPriceService/HappyFarmer.MarketPriceService.Api/"]
COPY ["src/Shared/HappyFarmer.Shared.Contracts/HappyFarmer.Shared.Contracts.csproj", "src/Shared/HappyFarmer.Shared.Contracts/"]
RUN dotnet restore "src/Services/MarketPriceService/HappyFarmer.MarketPriceService.Api/HappyFarmer.MarketPriceService.Api.csproj"

COPY src/Services/MarketPriceService/HappyFarmer.MarketPriceService.Api/ src/Services/MarketPriceService/HappyFarmer.MarketPriceService.Api/
COPY src/Shared/HappyFarmer.Shared.Contracts/ src/Shared/HappyFarmer.Shared.Contracts/

WORKDIR /src/src/Services/MarketPriceService/HappyFarmer.MarketPriceService.Api
RUN dotnet publish "HappyFarmer.MarketPriceService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HappyFarmer.MarketPriceService.Api.dll"]
