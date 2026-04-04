FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.sln .

COPY AlphaLising/AlphaLising.csproj ./AlphaLising/
COPY Core/Core.csproj ./Core/
COPY Application/Application.csproj ./Application/
COPY Infrastructure/Infrastructure.csproj ./Infrastructure/
COPY Orders/Orders.csproj ./Orders/

RUN dotnet restore AlphaLising.sln

COPY . .

# Собираем API-проект
WORKDIR /src/AlphaLising
RUN dotnet publish -c Release -o /app/publish

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080

ENTRYPOINT ["dotnet", "AlphaLising.dll"]
