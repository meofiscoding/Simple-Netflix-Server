#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ARG CONNECTION_STRING
ARG DATABASE_NAME

ENV MongoDB_ConnectionURI=$CONNECTION_STRING
ENV MongoDB_DatabaseName=$DATABASE_NAME

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY . .
WORKDIR "/src/SimpleServer"
RUN dotnet restore "SimpleServer.csproj"
RUN dotnet build "SimpleServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleServer.dll"]
