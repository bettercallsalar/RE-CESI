output "environment" {
  value = var.environment
}

output "resource_group_name" {
  value = azurerm_resource_group.application.name
}

output "virtual_machine_name" {
  value = azurerm_linux_virtual_machine.application.name
}

output "public_ip_address" {
  value = azurerm_public_ip.application.ip_address
}

output "public_fqdn" {
  value = azurerm_public_ip.application.fqdn
}
