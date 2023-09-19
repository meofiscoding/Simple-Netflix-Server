#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
# https://docs.docker.com/language/dotnet/build-images/
FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env

ARG CONNECTION_STRING
ARG DATABASE_NAME

ENV MongoDB_ConnectionURI=$CONNECTION_STRING
ENV MongoDB_DatabaseName=$DATABASE_NAME

WORKDIR /src
COPY ["MongoConnector/*.csproj", "MongoConnector/"]
RUN dotnet restore "MongoConnector/MongoConnector.csproj"
COPY ["ServerTest/*.csproj", "ServerTest/"]
RUN dotnet restore "ServerTest/SimpleServer.Test.csproj"
COPY ["SimpleServer/*.csproj", "SimpleServer/"]
RUN dotnet restore "SimpleServer/SimpleServer.csproj"

COPY . .
RUN dotnet publish "SimpleServer/SimpleServer.csproj" -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 as runtime
WORKDIR /publish
COPY --from=build-env /publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "SimpleServer.dll"]
