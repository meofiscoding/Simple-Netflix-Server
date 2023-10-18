#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Services/Movie/Movie.API/Movie.API.csproj", "src/Services/Movie/Movie.API/"]
COPY ["src/Services/Common/Services.Common/Services.Common.csproj", "src/Services/Common/Services.Common/"]
RUN dotnet restore "src/Services/Movie/Movie.API/Movie.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Movie/Movie.API"
RUN dotnet build "Movie.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Movie.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Movie.API.dll"]
