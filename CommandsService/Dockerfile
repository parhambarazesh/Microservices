FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o out

#This image is the runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
#build-env is the other image we got
COPY --from=build-env /app/out/ .
#Set the entry point for out image
ENTRYPOINT ["dotnet","CommandsService.dll"]