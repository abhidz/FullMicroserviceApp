FROM mcr.microsoft.com/dotnet/sdk:7.0 as build	
WORKDIR /app
EXPOSE 80

#copy all .csproj file and restore as distinct layers
COPY Carsties.sln Carsties.sln
COPY src/AuctionService/AuctionService.csproj src/AuctionService/AuctionService.csproj
COPY src/SearchService/SearchService.csproj src/SearchService/SearchService.csproj
COPY src/GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj
COPY src/Contracts/Contracts.csproj src/Contracts/Contracts.csproj
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj
COPY src/BiddingService/BiddingService.csproj src/BiddingService/BiddingService.csproj

#restore package
RUN dotnet restore Carsties.sln

#copy the app folders or other project codes over
COPY src/GatewayService src/GatewayService

WORKDIR /app/src/GatewayService
RUN dotnet publish -c Release -o /app/src/out

#build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 
WORKDIR /app
COPY --from=build /app/src/out .
ENTRYPOINT ["dotnet","GatewayService.dll"]




