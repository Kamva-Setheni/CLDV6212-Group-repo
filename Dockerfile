#First step: Building the dock
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

#Then copying the csproj for better layer caching
COPY *.csproj ./
RUN dotnet restore

#Then copying the rest of the source and publishing the application
COPY . .
RUN dotnet publish CoffeeAndChill.csproj -c Release -o /app/publish

#Second step: Building the runtime image
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0
WORKDIR /home/site/wwwroot
EXPOSE 80

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
	AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
	AzureWebJobsStorage="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://coffeenchill-azurite:10000/devstoreaccount1;QueueEndpoint=http://coffeenchill-azurite:10001/devstoreaccount1;TableEndpoint=http://coffeenchill-azurite:10002/devstoreaccount1;"

COPY --from=build /app/publish .
