#!/usr/bin/env python3
"""
EC2 Monitoring Script
Sends memory and disk metrics to the .NET API every minute.
Install: sudo cp push_metrics.py /opt/scripts/ && sudo chmod +x /opt/scripts/push_metrics.py
Add cron: * * * * * /opt/scripts/push_metrics.py
"""

import json
import os
import subprocess
import sys
import urllib.request
import urllib.error

# Configure these
API_URL = "http://<api-server-ip>:5000/api/metrics/push"

try:
    with open("/var/lib/cloud/data/instance-id") as f:
        INSTANCE_ID = f.read().strip()
except FileNotFoundError:
    try:
        import requests
        resp = requests.get("http://169.254.169.254/latest/meta-data/instance-id",
                            timeout=5)
        INSTANCE_ID = resp.text.strip()
    except Exception:
        INSTANCE_ID = os.uname().nodem
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
    memory = get_memory()
    disk = get_disk()

    payload = json.dumps({
        "instanceId": INSTANCE_ID,
        "memoryPercent": memory,
        "diskPercent": disk
    }).encode("utf-8")

    req = urllib.request.Request(
        API_URL,
        data=payload,
        headers={"Content-Type": "application/json"},
        method="POST"
    )

    try:
        with urllib.request.urlopen(req, timeout=10) as resp:
            if resp.status == 200:
                print(f"OK | Memory: {memory}% | Disk: {disk}%")
            else:
                print(f"FAIL | HTTP {resp.status}")
    except urllib.error.URLError as e:
        print(f"FAIL | {e.reason}")


if __name__ == "__main__":
    main()
