FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Копируем только решения и проекты для restore
COPY *.sln ./
COPY Directory.Build.props ./

# 2. Копируем .csproj файлы в подпапки (используем массив для надёжности)
COPY ["AlphaLising/AlphaLising.csproj", "AlphaLising/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

RUN dotnet restore AlphaLising.sln

# 3. Устанавливаем dotnet-ef (опционально, если нужны миграции в образе)
RUN dotnet tool install --global dotnet-ef && \
    ln -s /root/.dotnet/tools/dotnet-ef /usr/local/bin/dotnet-ef

# 4. Копируем ВСЁ остальное (исходный код)
# 🔥 Ключевое: копируем в корень /src, а не в подпапки
COPY . .

# 5. Публикуем
WORKDIR /src/AlphaLising
RUN dotnet publish -c Release -o /app/publish

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "AlphaLising.dll"]