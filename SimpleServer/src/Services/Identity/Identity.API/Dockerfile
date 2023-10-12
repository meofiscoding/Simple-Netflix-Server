#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY "SimpleServer.sln" "SimpleServer.sln   
COPY "ApiGateways/Web.Bff.SimpleNetflix/OcelotApiGw/OcelotApiGw.csproj" "ApiGateways/Web.Bff.SimpleNetflix/OcelotApiGw/OcelotApiGw.csproj"
COPY ["Services/Identity/Identity.API/Identity.API.csproj", "Services/Identity/Identity.API/Identity.API.csproj"]
COPY "Services/Movie/Movie.API/Movie.API.csproj" "Services/Movie/Movie.API/Movie.API.csproj"
COPY "Services/Movie/tests/Movie.UnitTests/Movie.UnitTests.csproj" "Services/Movie/tests/Movie.UnitTests/Movie.UnitTests.csproj"
COPY "Services/Movie/tests/Movie.FunctionalTests/Movie.FunctionalTests.csproj" "Services/Movie/tests/Movie.FunctionalTests/Movie.FunctionalTests.csproj"
COPY "SimpleServer/docker-compose.dcproj" "SimpleServer/docker-compose.dcproj"

RUN dotnet restore "SimpleServer.sln"

COPY . .
WORKDIR "/src/src/Services/Identity/Identity.API"
RUN dotnet build "Identity.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Identity.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.API.dll"]
