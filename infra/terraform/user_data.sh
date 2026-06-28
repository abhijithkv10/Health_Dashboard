#!/bin/bash
set -euo pipefail

exec > /var/log/user_data.log 2>&1

export DEBIAN_FRONTEND=noninteractive

dnf update -y
dnf install -y git jq postgresql15

# Install .NET 8 SDK
dnf install -y dotnet-sdk-8.0

# Install Node.js 20
dnf module enable -y nodejs:20
dnf install -y nodejs

# Install Nginx
dnf install -y nginx

# Clone the repo
cd /home/ec2-user
git clone ${repo_url} ${project_name}
cd ${project_name}
chown -R ec2-user:ec2-user .

# Write production config
cat > /home/ec2-user/${project_name}/backend/MonitorApi/appsettings.Production.json << 'PRODJSON'
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=${db_host};Database=${db_name};Username=${db_username};Password=${db_password}"
  },
  "Auth": {
    "ApiKey": "${api_key}",
    "JwtSecret": "${jwt_secret}",
    "Google": {
      "ClientId": "${google_client_id}",
      "AllowedDomain": "${google_allowed_domain}"
    }
  },
  "Aws": {
    "Region": "${aws_region}",
    "SnsTopicArn": "${sns_topic_arn}"
  },
  "Monitoring": {
    "CpuCriticalThreshold": 80,
    "CpuCriticalDurationMinutes": 3,
    "CpuWarningThreshold": 60,
    "MemoryCriticalThreshold": 80,
    "MemoryWarningThreshold": 70,
    "DiskCriticalThreshold": 90,
    "DiskWarningThreshold": 80,
    "PollingIntervalSeconds": 60
  }
}
PRODJSON

# Create the database (ignore error if exists)
PGPASSWORD=${db_password} psql -h ${db_host} -U ${db_username} -d postgres \
  -c "CREATE DATABASE ${db_name};" || true

# Build and deploy backend
cd /home/ec2-user/${project_name}/backend/MonitorApi
dotnet publish -c Release -o /opt/monitor/backend/publish

# Build and deploy frontend
cd /home/ec2-user/${project_name}/frontend
npm ci
npm run build
cp -r dist /opt/monitor/frontend/dist

# Set up systemd service
cat > /etc/systemd/system/monitor-api.service << 'UNIT'
[Unit]
Description=AWS Monitor .NET API
After=network.target

[Service]
Type=simple
User=ec2-user
WorkingDirectory=/opt/monitor/backend
ExecStart=/usr/bin/dotnet /opt/monitor/backend/publish/MonitorApi.dll
Restart=always
RestartSec=10
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
UNIT

systemctl daemon-reload
systemctl enable monitor-api
systemctl start monitor-api

# Set up Nginx
cat > /etc/nginx/conf.d/monitor.conf << 'NGINX'
limit_req_zone $binary_remote_addr zone=push_api:10m rate=5r/m;

server {
    listen 80;
    server_name _;

    root /opt/monitor/frontend/dist;
    index index.html;

    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;

    location /api/ {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'keep-alive';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    location /api/metrics/push {
        limit_req zone=push_api burst=5 nodelay;
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'keep-alive';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    location / {
        try_files $uri $uri/ /index.html;
    }

    location ~ /\. {
        deny all;
        access_log off;
        log_not_found off;
    }
}
NGINX

systemctl enable nginx
systemctl start nginx

echo "=== Deployment complete ==="
