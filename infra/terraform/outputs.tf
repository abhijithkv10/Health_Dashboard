output "dashboard_url" {
  description = "Public URL of the dashboard"
  value       = "http://${aws_eip.dashboard.public_ip}"
}

output "dashboard_ssh" {
  description = "SSH command to connect"
  value       = "ssh ec2-user@${aws_eip.dashboard.public_ip}"
}

output "db_endpoint" {
  description = "RDS PostgreSQL endpoint"
  value       = aws_db_instance.postgres.address
}

output "db_port" {
  description = "RDS PostgreSQL port"
  value       = aws_db_instance.postgres.port
}

output "sns_topic_arn" {
  description = "SNS topic for alerts"
  value       = aws_sns_topic.alerts.arn
}
