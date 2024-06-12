#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SWD.F-LocalBrand.API/SWD.F-LocalBrand.API.csproj", "SWD.F-LocalBrand.API/"]
COPY ["SWD.F-LocalBrand.Data/SWD.F-LocalBrand.Data.csproj", "SWD.F-LocalBrand.Data/"]
COPY ["SWD.F-LocalBrand.Services/SWD.F-LocalBrand.Business.csproj", "SWD.F-LocalBrand.Services/"]
RUN dotnet restore "./SWD.F-LocalBrand.API/./SWD.F-LocalBrand.API.csproj"
COPY . .
WORKDIR "/src/SWD.F-LocalBrand.API"
RUN dotnet build "./SWD.F-LocalBrand.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SWD.F-LocalBrand.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SWD.F-LocalBrand.API.dll"]