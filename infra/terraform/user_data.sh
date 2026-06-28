#!/bin/bash
set -euo pipefail

exec > /var/log/user_data.log 2>&1

dnf update -y
dnf install -y docker git jq postgresql15

# Install docker-compose plugin
dnf install -y docker-compose-plugin

systemctl enable docker
systemctl start docker

# Clone the repo
cd /home/ec2-user
git clone ${repo_url} ${project_name}
cd ${project_name}
chown -R ec2-user:ec2-user .

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
