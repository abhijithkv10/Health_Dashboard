#!/bin/bash
set -euo pipefail

exec > /var/log/user_data.log 2>&1

apt update -y
apt install -y docker.io docker-compose-v2 git postgresql-client

systemctl enable docker
systemctl start docker
usermod -aG docker ubuntu

# Clone the repo
cd /home/ubuntu
git clone ${repo_url} ${project_name}
cd ${project_name}
chown -R ubuntu:ubuntu .

# Write production config
cat > .env << ENVFILE
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=${db_host};Database=${db_name};Username=${db_username};Password=${db_password}
Auth__ApiKey=${api_key}
Auth__JwtSecret=${jwt_secret}
Auth__Google__ClientId=${google_client_id}
Auth__Google__AllowedDomain=${google_allowed_domain}
Aws__Region=${aws_region}
Aws__SnsTopicArn=${sns_topic_arn}
Monitoring__CpuCriticalThreshold=80
Monitoring__CpuCriticalDurationMinutes=3
Monitoring__CpuWarningThreshold=60
Monitoring__MemoryCriticalThreshold=80
Monitoring__MemoryWarningThreshold=70
Monitoring__DiskCriticalThreshold=90
Monitoring__DiskWarningThreshold=80
Monitoring__PollingIntervalSeconds=60
ENVFILE

# Create the database (ignore error if exists)
PGPASSWORD=${db_password} psql -h ${db_host} -U ${db_username} -d postgres \
  -c "CREATE DATABASE ${db_name};" || true

# Start everything
docker compose up -d

echo "=== Deployment complete ==="
