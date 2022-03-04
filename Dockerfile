
FROM mcr.microsoft.com/dotnet/aspnet:latest
COPY bin/Release/net6.0/publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "CryptoAlertsBot.dll"]


#docker buildx build --platform linux/amd64,linux/arm/v7 -t termisfa/cryptoalertsbot --push .
#docker run -d --name cryptoalerts termisfa/cryptoalertsbot