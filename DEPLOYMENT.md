# デプロイメントガイド

## 本番環境デプロイ手順

### 1. 事前準備

#### 必要なソフトウェア
- .NET 8 Runtime (サーバー)
- Node.js 18+ (ビルド環境)
- Webサーバー (Nginx, Apache, IIS等)
- リバースプロキシ設定 (推奨)

#### ドメインとSSL証明書
- フロントエンド用ドメイン
- バックエンドAPI用ドメイン (またはサブドメイン)
- SSL証明書の取得と設定

### 2. バックエンドデプロイ

#### 2.1 設定ファイルの更新

`appsettings.Production.json` を本番環境に合わせて更新:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/var/lib/accounting/accounting_production.db"
  },
  "AllowedHosts": "your-api-domain.com",
  "Cors": {
    "AllowedOrigins": [
      "https://your-frontend-domain.com"
    ]
  }
}
```

#### 2.2 アプリケーションのビルド

```bash
cd SimpleAccounting.API

# リリースビルド
dotnet publish -c Release -o ./publish --self-contained false

# または自己完結型デプロイ
dotnet publish -c Release -o ./publish --self-contained true -r linux-x64
```

#### 2.3 サーバーへのデプロイ

```bash
# ファイルをサーバーにコピー
scp -r ./publish user@server:/var/www/accounting-api/

# サーバーでの権限設定
sudo chown -R www-data:www-data /var/www/accounting-api/
sudo chmod +x /var/www/accounting-api/SimpleAccounting.API
```

#### 2.4 systemdサービス設定 (Linux)

`/etc/systemd/system/accounting-api.service`:

```ini
[Unit]
Description=Simple Accounting API
After=network.target

[Service]
Type=notify
User=www-data
WorkingDirectory=/var/www/accounting-api
ExecStart=/var/www/accounting-api/SimpleAccounting.API
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

サービスの有効化と開始:

```bash
sudo systemctl daemon-reload
sudo systemctl enable accounting-api
sudo systemctl start accounting-api
sudo systemctl status accounting-api
```

#### 2.5 Nginxリバースプロキシ設定

`/etc/nginx/sites-available/accounting-api`:

```nginx
server {
    listen 80;
    server_name your-api-domain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name your-api-domain.com;

    ssl_certificate /path/to/your/certificate.crt;
    ssl_certificate_key /path/to/your/private.key;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### 3. フロントエンドデプロイ

#### 3.1 環境設定

`.env.production`:

```env
NEXT_PUBLIC_API_URL=https://your-api-domain.com/api
NODE_ENV=production
```

#### 3.2 ビルド

```bash
cd frontend

# 依存関係のインストール
npm ci

# 本番ビルド
npm run build

# 静的エクスポート (オプション)
npm run export
```

#### 3.3 静的ファイルのデプロイ

```bash
# ビルド成果物をサーバーにコピー
scp -r ./out/* user@server:/var/www/accounting-frontend/

# または .next/standalone を使用する場合
scp -r ./.next/standalone/* user@server:/var/www/accounting-frontend/
```

#### 3.4 Nginx設定 (フロントエンド)

`/etc/nginx/sites-available/accounting-frontend`:

```nginx
server {
    listen 80;
    server_name your-frontend-domain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name your-frontend-domain.com;

    ssl_certificate /path/to/your/certificate.crt;
    ssl_certificate_key /path/to/your/private.key;

    root /var/www/accounting-frontend;
    index index.html;

    # Next.js静的ファイル
    location /_next/static/ {
        alias /var/www/accounting-frontend/_next/static/;
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # その他の静的ファイル
    location /static/ {
        alias /var/www/accounting-frontend/static/;
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # メインアプリケーション
    location / {
        try_files $uri $uri.html $uri/ /index.html;
    }

    # セキュリティヘッダー
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
}
```

### 4. データベース設定

#### 4.1 本番データベースの初期化

```bash
# データベースディレクトリの作成
sudo mkdir -p /var/lib/accounting
sudo chown www-data:www-data /var/lib/accounting

# 初期データベースの作成 (開発環境から)
cd SimpleAccounting.API
dotnet ef database update --environment Production
```

#### 4.2 バックアップ設定

```bash
#!/bin/bash
# /usr/local/bin/backup-accounting-db.sh

DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/var/backups/accounting"
DB_PATH="/var/lib/accounting/accounting_production.db"

mkdir -p $BACKUP_DIR
cp $DB_PATH $BACKUP_DIR/accounting_backup_$DATE.db

# 古いバックアップの削除 (30日以上)
find $BACKUP_DIR -name "accounting_backup_*.db" -mtime +30 -delete
```

crontabに追加:
```bash
# 毎日午前2時にバックアップ
0 2 * * * /usr/local/bin/backup-accounting-db.sh
```

### 5. セキュリティ設定

#### 5.1 ファイアウォール設定

```bash
# UFW (Ubuntu)
sudo ufw allow 22/tcp    # SSH
sudo ufw allow 80/tcp    # HTTP
sudo ufw allow 443/tcp   # HTTPS
sudo ufw enable
```

#### 5.2 SSL/TLS設定

Let's Encryptを使用する場合:

```bash
# Certbot のインストール
sudo apt install certbot python3-certbot-nginx

# 証明書の取得
sudo certbot --nginx -d your-api-domain.com -d your-frontend-domain.com

# 自動更新の設定
sudo crontab -e
# 以下を追加:
# 0 12 * * * /usr/bin/certbot renew --quiet
```

### 6. 監視とログ

#### 6.1 ログ設定

アプリケーションログの設定:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    },
    "File": {
      "Path": "/var/log/accounting/app.log",
      "FileSizeLimitBytes": 10485760,
      "MaxRollingFiles": 10
    }
  }
}
```

#### 6.2 ログローテーション

`/etc/logrotate.d/accounting`:

```
/var/log/accounting/*.log {
    daily
    missingok
    rotate 30
    compress
    delaycompress
    notifempty
    create 644 www-data www-data
}
```

### 7. パフォーマンス最適化

#### 7.1 Nginx設定の最適化

```nginx
# gzip圧縮
gzip on;
gzip_vary on;
gzip_min_length 1024;
gzip_types text/plain text/css text/xml text/javascript application/javascript application/xml+rss application/json;

# ブラウザキャッシュ
location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
    expires 1y;
    add_header Cache-Control "public, immutable";
}
```

#### 7.2 データベース最適化

```sql
-- インデックスの作成
CREATE INDEX IF NOT EXISTS IX_Transactions_Date ON Transactions(Date);
CREATE INDEX IF NOT EXISTS IX_Transactions_Type ON Transactions(Type);
```

### 8. トラブルシューティング

#### 8.1 よくある問題

1. **アプリケーションが起動しない**
   ```bash
   # ログの確認
   sudo journalctl -u accounting-api -f
   
   # ポートの確認
   sudo netstat -tlnp | grep :5000
   ```

2. **CORS エラー**
   - `appsettings.Production.json` のCORS設定を確認
   - Nginxのプロキシヘッダー設定を確認

3. **データベース権限エラー**
   ```bash
   # 権限の確認と修正
   sudo chown www-data:www-data /var/lib/accounting/accounting_production.db
   sudo chmod 664 /var/lib/accounting/accounting_production.db
   ```

#### 8.2 ヘルスチェック

```bash
# API ヘルスチェック
curl -f https://your-api-domain.com/api/transactions || echo "API Down"

# フロントエンド ヘルスチェック
curl -f https://your-frontend-domain.com || echo "Frontend Down"
```

### 9. 更新手順

#### 9.1 アプリケーション更新

```bash
# バックエンド更新
sudo systemctl stop accounting-api
cd SimpleAccounting.API
git pull
dotnet publish -c Release -o ./publish
sudo cp -r ./publish/* /var/www/accounting-api/
sudo systemctl start accounting-api

# フロントエンド更新
cd frontend
git pull
npm ci
npm run build
sudo cp -r ./out/* /var/www/accounting-frontend/
```

#### 9.2 データベースマイグレーション

```bash
cd SimpleAccounting.API
dotnet ef database update --environment Production
```

この手順に従って、Simple Accounting Appを本番環境に安全にデプロイできます。