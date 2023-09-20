#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
# https://docs.docker.com/language/dotnet/build-images/
FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env

WORKDIR /src
COPY ["MongoConnector/*.csproj", "MongoConnector/"]
RUN dotnet restore "MongoConnector/MongoConnector.csproj"
COPY ["ServerTest/*.csproj", "ServerTest/"]
RUN dotnet restore "ServerTest/SimpleServer.Test.csproj"
COPY ["SimpleServer/*.csproj", "SimpleServer/"]
RUN dotnet restore "SimpleServer/SimpleServer.csproj"

COPY . .
RUN dotnet build "SimpleServer/SimpleServer.csproj" 

FROM build-env AS testrunner
WORKDIR /src/ServerTest
CMD [ "dotnet", "test", "--logger:trx" ]

FROM build-env AS test 
WORKDIR /src
ARG CONNECTION_STRING
ARG DATABASE_NAME

ENV MongoDB_ConnectionURI=$CONNECTION_STRING
ENV MongoDB_DatabaseName=$DATABASE_NAME
RUN dotnet test "ServerTest/SimpleServer.Test.csproj" --logger:trx

# publish
FROM build-env AS publish
WORKDIR /src
RUN dotnet publish "SimpleServer/SimpleServer.csproj" -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 as runtime
WORKDIR /publish
COPY --from=publish /publish .
EXPOSE 80

ARG CONNECTION_STRING
ARG DATABASE_NAME

ENV MongoDB_ConnectionURI=$CONNECTION_STRING
ENV MongoDB_DatabaseName=$DATABASE_NAME
ENTRYPOINT ["dotnet", "SimpleServer.dll"]
