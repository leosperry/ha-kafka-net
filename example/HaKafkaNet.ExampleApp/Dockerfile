FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY *.sln ./
COPY MyHome/MyHome.csproj ./MyHome/MyHome.csproj
COPY MyHome/appsettings.Production.json ./MyHome/appsettings.json
# Use the next line if setting up HaKafkaNet as a sub-module to your repo
# COPY ha-kafka-net/src/HaKafkaNet/*.csproj ./ha-kafka-net/src/HaKafkaNet/

RUN dotnet restore MyHome/MyHome.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out ./HaKafkaNet.ExampleApp/HaKafkaNet.ExampleApp.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
# Set your timezone
ENV TZ="US/Eastern" 
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "MyHome.dll"]