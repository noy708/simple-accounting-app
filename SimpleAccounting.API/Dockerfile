# 1. ビルドステージ: アプリケーションのビルドと公開
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SimpleAccounting.API.csproj", "."]
RUN dotnet restore "./SimpleAccounting.API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "SimpleAccounting.API.csproj" -c Release -o /app/build

# 2. 公開ステージ: アプリケーションの公開
FROM build AS publish
RUN dotnet publish "SimpleAccounting.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 3. 最終ステージ: Playwrightと.NETランタイムが含まれるイメージを使用
FROM mcr.microsoft.com/playwright/dotnet:v1.48.0-jammy AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# タイムゾーンを日本に設定
ENV TZ=Asia/Tokyo

# Install fonts and update font cache
# フォントはベースイメージに含まれていないため、手動でインストールします
RUN apt-get update && apt-get install -y \
    fonts-noto-cjk \
    fonts-ipaexfont \
    && rm -rf /var/lib/apt/lists/*
    
# フォントキャッシュを更新
RUN fc-cache -f -v

# Copy published application from the 'publish' stage
COPY --from=publish /app/publish .

# Create a non-root user for running the application
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Set proper ownership for the application directory
RUN chown -R appuser:appuser /app

# Ensure SQLite database directory exists and has correct permissions
RUN mkdir -p /app/data && chown -R appuser:appuser /app/data

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/accounting.db"

# Switch to the non-root user
USER appuser

ENTRYPOINT ["dotnet", "SimpleAccounting.API.dll"]