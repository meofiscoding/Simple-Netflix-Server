# https://hub.docker.com/_/microsoft-dotnet
# Reference : https://learn.microsoft.com/vi-vn/aspnet/core/host-and-deploy/docker/building-net-docker-images?view=aspnetcore-7.0
# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /App

# copy all file and folder to current directory
COPY . .
# set current directory to SimpleServer
WORKDIR /App/SimpleServer
RUN dotnet restore
RUN dotnet publish -c release -o out

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
COPY --from=build /App/SimpleServer/out .
ENTRYPOINT ["dotnet", "SimpleServer.dll"]