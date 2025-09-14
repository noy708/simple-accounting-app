# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SimpleAccounting.API.csproj", "."]
RUN dotnet restore "./SimpleAccounting.API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "SimpleAccounting.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleAccounting.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Install Node.js and Playwright browsers with specific path
RUN apt-get update && apt-get install -y nodejs npm && rm -rf /var/lib/apt/lists/*

# Set Playwright browsers path and install
ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
RUN npx playwright@latest install chromium --with-deps

# Use the official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# タイムゾーンを日本に設定
ENV TZ=Asia/Tokyo

# Install dependencies for Playwright/Chromium
RUN apt-get update && apt-get install -y \
    # 日本語フォント関連パッケージ
    fonts-noto \
    fonts-ipaexfont \
    fonts-noto-cjk \
    fonts-ipafont-gothic \
    fonts-ipafont-mincho \
    fonts-takao-gothic \
    fonts-takao-mincho \
    # Playwright/Chromium用の依存関係
    wget \
    gnupg \
    ca-certificates \
    procps \
    libxss1 \
    libgconf-2-4 \
    libxrandr2 \
    libasound2 \
    libpangocairo-1.0-0 \
    libatk1.0-0 \
    libcairo-gobject2 \
    libgtk-3-0 \
    libgdk-pixbuf2.0-0 \
    libxcomposite1 \
    libxcursor1 \
    libxdamage1 \
    libxext6 \
    libxfixes3 \
    libxi6 \
    libxrender1 \
    libxtst6 \
    libdrm2 \
    libgbm1 \
    fonts-liberation \
    libnss3 \
    lsb-release \
    xdg-utils \
    libappindicator3-1 \
    libatk-bridge2.0-0 \
    libatspi2.0-0 \
    libnspr4 \
    libx11-xcb1 \
    libxcb-dri3-0 \
    # Node.js (ランタイムでも必要)
    nodejs \
    npm \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Install Playwright browsers directly in final stage
ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
RUN npx playwright@latest install chromium --with-deps

# Create a non-root user for running the application
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Create directory for SQLite database with proper permissions
RUN mkdir -p /app/data && \
    chown -R appuser:appuser /app && \
    chown -R appuser:appuser /ms-playwright && \
    chmod -R 755 /app/data

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/accounting.db"

# Switch to non-root user
USER appuser

ENTRYPOINT ["dotnet", "SimpleAccounting.API.dll"]