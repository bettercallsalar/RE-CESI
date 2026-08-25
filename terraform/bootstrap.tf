locals {
  vm_bootstrap_script_path          = "${path.module}/../scripts/azure/bootstrap-vm.sh"
  vm_bootstrap_timestamp_hex_length = 7
}

resource "azurerm_virtual_machine_extension" "docker" {
  name                       = "bootstrap-docker"
  virtual_machine_id         = azurerm_linux_virtual_machine.application.id
  publisher                  = "Microsoft.Azure.Extensions"
  type                       = "CustomScript"
  type_handler_version       = "2.1"
  auto_upgrade_minor_version = true

  settings = jsonencode({
    timestamp = parseint(
      substr(
        filesha256(local.vm_bootstrap_script_path),
        0,
        local.vm_bootstrap_timestamp_hex_length
      ),
      16
    )
  })

  protected_settings = jsonencode({
    script = base64gzip(file(local.vm_bootstrap_script_path))
  })
}
