# AWS Server Monitor

A web dashboard for monitoring AWS EC2 server metrics with critical-level alerts.

## Architecture

- **Backend**: .NET 8 Web API (C#)
- **Frontend**: React + TypeScript + Tailwind CSS
- **Monitoring**: Python script running on EC2 instances
- **Alerts**: AWS SNS email notifications

## Project Structure

```
aws-monitor/
├── backend/MonitorApi/       # .NET Web API
│   ├── Controllers/          # API endpoints
│   ├── Models/               # Data models
│   ├── Services/             # Business logic
│   └── Program.cs            # Entry point
├── frontend/                 # React app
│   └── src/
│       ├── components/       # Reusable UI components
│       └── pages/            # Dashboard & Detail pages
├── scripts/
│   └── push_metrics.py       # EC2 monitoring script
└── deploy/
    ├── monitor-api.service   # systemd unit
    ├── nginx.conf            # Nginx reverse proxy config
    └── setup.sh              # Deployment script
```

## Thresholds

| Metric     | Warning | Critical |
|------------|---------|----------|
| CPU        | > 60%   | > 80% for 3 min |
| Memory     | > 70%   | > 80%    |
| Disk       | > 80%   | > 90%    |

## Development

```bash
# Run backend
cd backend/MonitorApi
dotnet run

# Run frontend
cd frontend
npm run dev
```

## Deployment

1. Copy project to EC2
2. Run `sudo bash deploy/setup.sh`
3. Configure instances in `backend/MonitorApi/appsettings.json`
4. Install push_metrics.py on monitored EC2 instances
