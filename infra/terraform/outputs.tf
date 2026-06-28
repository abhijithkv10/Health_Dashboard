output "dashboard_url" {
  description = "Public URL of the dashboard"
  value       = "http://${aws_eip.dashboard.public_ip}"
}

output "dashboard_ssh" {
  description = "SSH command to connect"
  value       = "ssh ubuntu@${aws_eip.dashboard.public_ip}"
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

output "state_bucket" {
  description = "S3 bucket for Terraform state"
  value       = aws_s3_bucket.state.bucket
}

output "state_lock_table" {
  description = "DynamoDB table for Terraform state locking"
  value       = aws_dynamodb_table.state_lock.name
}

output "ec2_instance_id" {
  description = "EC2 instance ID"
  value       = aws_instance.dashboard.id
}
