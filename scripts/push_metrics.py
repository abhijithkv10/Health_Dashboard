#!/usr/bin/env python3
"""
EC2 Monitoring Script
Sends CPU, memory and disk metrics to the .NET API every minute.
Install: sudo cp push_metrics.py /opt/scripts/ && sudo chmod +x /opt/scripts/push_metrics.py
Add cron: * * * * * /opt/scripts/push_metrics.py
"""

import json
import os
import subprocess
import time
import urllib.request
import urllib.error

# Configure these via environment variables or edit directly
API_URL = os.environ.get("MONITOR_API_URL", "http://localhost:5000/api/metrics/push")
API_KEY = os.environ.get("MONITOR_API_KEY", "")


def get_instance_id():
    try:
        with open("/var/lib/cloud/data/instance-id") as f:
            return f.read().strip()
    except FileNotFoundError:
        pass

    try:
        token = subprocess.run(
            ["curl", "-s", "-X", "PUT", "http://169.254.169.254/latest/api/token",
             "-H", "X-aws-ec2-metadata-token-ttl-seconds: 21600"],
            capture_output=True, text=True, timeout=5
        ).stdout.strip()

        if token:
            result = subprocess.run(
                ["curl", "-s", "-H", f"X-aws-ec2-metadata-token: {token}",
                 "http://169.254.169.254/latest/meta-data/instance-id"],
                capture_output=True, text=True, timeout=5
            )
            if result.returncode == 0 and result.stdout.strip():
                return result.stdout.strip()
    except Exception:
        pass

    try:
        result = subprocess.run(
            ["curl", "-s", "http://169.254.169.254/latest/meta-data/instance-id"],
            capture_output=True, text=True, timeout=5
        )
        if result.returncode == 0 and result.stdout.strip():
            return result.stdout.strip()
    except Exception:
        pass

    return os.uname().nodename


def get_cpu():
    try:
        with open("/proc/stat") as f:
            line = f.readline()
        parts = line.split()
        if len(parts) < 5:
            return None
        user = int(parts[1])
        nice = int(parts[2])
        system = int(parts[3])
        idle = int(parts[4])
        total1 = user + nice + system + idle
        idle1 = idle

        time.sleep(0.5)

        with open("/proc/stat") as f:
            line = f.readline()
        parts = line.split()
        user = int(parts[1])
        nice = int(parts[2])
        system = int(parts[3])
        idle = int(parts[4])
        total2 = user + nice + system + idle
        idle2 = idle

        total_delta = total2 - total1
        idle_delta = idle2 - idle1
        if total_delta == 0:
            return None
        return round((1 - idle_delta / total_delta) * 100, 1)
    except Exception:
        return None


def get_memory():
    with open("/proc/meminfo") as f:
        data = f.read()
    lines = {l.split(":")[0]: l.split(":")[1].strip() for l in data.split("\n") if ":" in l}

    total_kb = int(lines.get("MemTotal", "0").split()[0])
    avail_kb = int(lines.get("MemAvailable", "0").split()[0])

    if total_kb == 0:
        return 0.0
    return round((total_kb - avail_kb) / total_kb * 100, 1)


def get_disk():
    result = subprocess.run(
        ["df", "--output=size,used", "/"],
        capture_output=True, text=True, check=True
    )
    lines = result.stdout.strip().split("\n")
    if len(lines) < 2:
        return 0.0
    parts = lines[1].split()
    if len(parts) < 2:
        return 0.0
    total = int(parts[0])
    used = int(parts[1])
    if total == 0:
        return 0.0
    return round(used / total * 100, 1)


def main():
    instance_id = get_instance_id()
    cpu = get_cpu()
    memory = get_memory()
    disk = get_disk()

    payload = {
        "instanceId": instance_id,
        "cpuPercent": cpu,
        "memoryPercent": memory,
        "diskPercent": disk
    }

    body = json.dumps(payload).encode("utf-8")

    headers = {"Content-Type": "application/json"}
    if API_KEY:
        headers["X-API-Key"] = API_KEY

    req = urllib.request.Request(
        API_URL,
        data=body,
        headers=headers,
        method="POST"
    )

    try:
        with urllib.request.urlopen(req, timeout=10) as resp:
            if resp.status == 200:
                print(f"OK | CPU: {cpu}% | Memory: {memory}% | Disk: {disk}%")
            else:
                print(f"FAIL | HTTP {resp.status}")
    except urllib.error.HTTPError as e:
        print(f"FAIL | HTTP {e.code}: {e.reason}")
    except urllib.error.URLError as e:
        print(f"FAIL | {e.reason}")


if __name__ == "__main__":
    main()
