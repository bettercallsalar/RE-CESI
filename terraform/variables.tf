variable "environment" {
  type        = string
  description = "Deployment environment"

  validation {
    condition     = contains(["prod", "dev"], var.environment)
    error_message = "Environment must be either prod or dev."
  }
}

variable "prefix" {
  type        = string
  description = "Prefix used for Azure resource names"
}

variable "domain_name_label" {
  type        = string
  description = "Globally unique DNS label assigned to the public IP"

  validation {
    condition = (
      length(var.domain_name_label) >= 3 &&
      length(var.domain_name_label) <= 63 &&
      can(regex("^[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$", var.domain_name_label))
    )
    error_message = "Domain name label must contain 3 to 63 lowercase letters, numbers, or hyphens."
  }
}

variable "resource_location" {
  type        = string
  default     = "polandcentral"
  description = "Azure region used by all application resources"
}

variable "admin_username" {
  type        = string
  default     = "recesi"
  description = "Linux administrator and application user"
}

variable "admin_ssh_public_key" {
  type        = string
  description = "SSH public key used by the VM administrator"
}

variable "admin_ssh_source_cidr" {
  type        = string
  default     = null
  nullable    = true
  description = "Optional CIDR allowed to reach SSH; null keeps port 22 closed"
}

variable "vm_size" {
  type        = string
  default     = "Standard_B2ls_v2"
  description = "Burstable VM size with 2 vCPU and 4 GiB RAM"
}

variable "enable_budget" {
  type        = bool
  description = "Whether Terraform manages the shared subscription budget"
}

variable "budget_amount" {
  type        = number
  default     = 100
  description = "Annual Azure budget in the subscription currency"
}

variable "budget_contact_emails" {
  type        = list(string)
  description = "Email addresses receiving Azure budget alerts"
}

variable "budget_start_date" {
  type        = string
  default     = "2026-08-01T00:00:00Z"
  description = "Budget start date on the first day of a month"
}
