# Console app, không expose port — chạy 1 lần rồi thoát (`docker compose run`), không phải
# service chạy liên tục như các Dockerfile khác trong repo. Build context PHẢI là thư mục
# HappyFarmer_Backend/ (khớp deploy.yml) dù project này không reference Shared.Contracts.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Tools/HappyFarmer.MarketPriceCrawler/HappyFarmer.MarketPriceCrawler.csproj", "src/Tools/HappyFarmer.MarketPriceCrawler/"]
RUN dotnet restore "src/Tools/HappyFarmer.MarketPriceCrawler/HappyFarmer.MarketPriceCrawler.csproj"

COPY src/Tools/HappyFarmer.MarketPriceCrawler/ src/Tools/HappyFarmer.MarketPriceCrawler/

WORKDIR /src/src/Tools/HappyFarmer.MarketPriceCrawler
RUN dotnet publish "HappyFarmer.MarketPriceCrawler.csproj" -c Release -o /app/publish /p:UseAppHost=false

# runtime (không phải aspnet) — console app thuần, không cần ASP.NET Core.
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HappyFarmer.MarketPriceCrawler.dll"]
