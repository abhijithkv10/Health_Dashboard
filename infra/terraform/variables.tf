variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}

variable "project_name" {
  description = "Project name used for resource naming"
  type        = string
  default     = "aws-monitor"
}

variable "vpc_id" {
  description = "VPC ID (leave null to use default)"
  type        = string
  default     = null
}

variable "ami_id" {
  description = "AMI ID for EC2 (Ubuntu 24.04 LTS)"
  type        = string
}

variable "ec2_instance_type" {
  description = "EC2 instance type"
  type        = string
  default     = "t3.small"
}

variable "rds_instance_class" {
  description = "RDS instance class"
  type        = string
  default     = "db.t3.micro"
}

variable "db_name" {
  description = "PostgreSQL database name"
  type        = string
  default     = "aws_monitor"
}

variable "db_username" {
  description = "PostgreSQL master username"
  type        = string
  sensitive   = true
}

variable "db_password" {
  description = "PostgreSQL master password"
  type        = string
  sensitive   = true
}

variable "monitor_api_key" {
  description = "API key for /api/metrics/push endpoint"
  type        = string
  sensitive   = true
}

variable "jwt_secret" {
  description = "JWT signing secret (32+ characters)"
  type        = string
  sensitive   = true
}

variable "google_client_id" {
  description = "Google OAuth client ID"
  type        = string
}

variable "google_allowed_domain" {
  description = "Allowed email domain for admin access"
  type        = string
  default     = ""
}

variable "repo_url" {
  description = "Git repo URL to clone the application"
  type        = string
}

variable "key_name" {
  description = "EC2 key pair name for SSH access"
  type        = string
}

variable "ssh_allowed_cidrs" {
  description = "CIDR blocks allowed for SSH access"
  type        = list(string)
  default     = ["0.0.0.0/0"]
}
