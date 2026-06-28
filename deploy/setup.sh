#!/bin/bash
# Deployment script for AWS Monitor on Amazon Linux 2023 EC2
# Run: sudo bash setup.sh
set -euo pipefail

echo "=== Installing .NET 8 SDK ==="
sudo dnf install -y dotnet-sdk-8.0

echo "=== Installing Nginx ==="
sudo dnf install -y nginx

echo "=== Installing PostgreSQL client ==="
sudo dnf install -y postgresql15

echo "=== Building Backend ==="
cd /home/ec2-user/aws-monitor/backend/MonitorApi
dotnet publish -c Release -o /opt/monitor/backend/publish

echo "=== Building Frontend ==="
cd /home/ec2-user/aws-monitor/frontend
npm ci && npm run build
sudo cp -r dist /opt/monitor/frontend/dist

echo "=== Setting Up Backend Service ==="
sudo cp /home/ec2-user/aws-monitor/deploy/monitor-api.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable monitor-api
sudo systemctl start monitor-api

echo "=== Setting Up Nginx ==="
sudo cp /home/ec2-user/aws-monitor/deploy/nginx.conf /etc/nginx/conf.d/monitor.conf
sudo systemctl enable nginx
sudo systemctl start nginx

echo "=== Setup Complete ==="
echo "Dashboard: http://$(curl -s http://169.254.169.254/latest/meta-data/public-ipv4)"
echo "API: http://$(curl -s http://169.254.169.254/latest/meta-data/public-ipv4)/api/instances"
