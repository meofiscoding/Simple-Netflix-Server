#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Services/Identity/Identity.Grpc/Identity.Grpc.csproj", "src/Services/Identity/Identity.Grpc/"]
RUN dotnet restore "src/Services/Identity/Identity.Grpc/Identity.Grpc.csproj"
COPY . .
WORKDIR "/src/src/Services/Identity/Identity.Grpc"
RUN dotnet build "Identity.Grpc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Identity.Grpc.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.Grpc.dll"]
