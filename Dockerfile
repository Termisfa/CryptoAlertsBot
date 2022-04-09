
FROM mcr.microsoft.com/dotnet/aspnet:latest

#It is needed to make 'OxyPlot.SkiaSharp' work (Used in ChartGenerator.cs)
RUN apt-get update && apt-get install -y libfontconfig1

COPY bin/Release/net6.0/publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "CryptoAlertsBot.dll"]


#docker buildx build --platform linux/amd64,linux/arm/v7 -t termisfa/cryptoalertsbot --push .
#docker run -d --name cryptoalerts termisfa/cryptoalertsbot