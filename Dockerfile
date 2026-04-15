FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.sln .
COPY AlphaLising/AlphaLising.csproj ./AlphaLising/
COPY Core/Core.csproj ./Core/
COPY Application/Application.csproj ./Application/
COPY Infrastructure/Infrastructure.csproj ./Infrastructure/

RUN dotnet restore AlphaLising.sln

# Устанавливаем dotnet-ef на этапе build
RUN dotnet tool install --global dotnet-ef

COPY . .
WORKDIR /src/AlphaLising
RUN dotnet publish -c Release -o /app/publish

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# 🔥 Копируем dotnet-ef из build-этапа в runtime
COPY --from=build /root/.dotnet/tools /root/.dotnet/tools
ENV PATH="$PATH:/root/.dotnet/tools"

COPY --from=build /app/publish .
EXPOSE 8080

ENTRYPOINT ["dotnet", "AlphaLising.dll"]