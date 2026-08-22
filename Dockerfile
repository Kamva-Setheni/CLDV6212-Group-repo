#First step: Building the dock
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

#Then copying the csproj for better layer caching
COPY *.csproj ./
RUN dotnet restore

#Then copying the rest of the source and publishing the application
COPY . .
RUN dotnet publish CoffeeAndChill.csproj -c Release -o /app/publish

#Second step: Building the runtime image
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0
WORKDIR /home/site/wwwroot

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
	AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=build /app/publish .
