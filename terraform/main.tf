locals {
  common_tags = {
    environment = var.environment
    managed-by  = "terraform"
    project     = "RE-CESI"
  }
}

resource "azurerm_resource_group" "application" {
  name     = "${var.prefix}-group"
  location = var.resource_location
  tags     = local.common_tags
}

resource "azurerm_virtual_network" "application" {
  name                = "${var.prefix}-vnet"
  address_space       = ["10.0.0.0/16"]
  location            = azurerm_resource_group.application.location
  resource_group_name = azurerm_resource_group.application.name
  tags                = local.common_tags
}

resource "azurerm_subnet" "application" {
  name                 = "${var.prefix}-subnet"
  resource_group_name  = azurerm_resource_group.application.name
  virtual_network_name = azurerm_virtual_network.application.name
  address_prefixes     = ["10.0.1.0/24"]
}

resource "azurerm_public_ip" "application" {
  name                = "${var.prefix}-ip"
  location            = azurerm_resource_group.application.location
  resource_group_name = azurerm_resource_group.application.name
  allocation_method   = "Static"
  sku                 = "Standard"
  domain_name_label   = var.domain_name_label
  tags                = local.common_tags
}

resource "azurerm_network_security_group" "application" {
  name                = "${var.prefix}-nsg"
  location            = azurerm_resource_group.application.location
  resource_group_name = azurerm_resource_group.application.name
  tags                = local.common_tags

  security_rule {
    name                       = "Http"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "80"
    source_address_prefix      = "Internet"
    destination_address_prefix = "*"
  }

  dynamic "security_rule" {
    for_each = var.admin_ssh_source_cidr == null ? [] : [var.admin_ssh_source_cidr]

    content {
      name                       = "SshFromAdministrator"
      priority                   = 110
      direction                  = "Inbound"
      access                     = "Allow"
      protocol                   = "Tcp"
      source_port_range          = "*"
      destination_port_range     = "22"
      source_address_prefix      = security_rule.value
      destination_address_prefix = "*"
    }
  }
}

resource "azurerm_network_interface" "application" {
  name                = "${var.prefix}-nic"
  location            = azurerm_resource_group.application.location
  resource_group_name = azurerm_resource_group.application.name
  tags                = local.common_tags

  ip_configuration {
    name                          = "application"
    subnet_id                     = azurerm_subnet.application.id
    private_ip_address_allocation = "Dynamic"
    public_ip_address_id          = azurerm_public_ip.application.id
  }
}

resource "azurerm_network_interface_security_group_association" "application" {
  network_interface_id      = azurerm_network_interface.application.id
  network_security_group_id = azurerm_network_security_group.application.id
}

resource "azurerm_linux_virtual_machine" "application" {
  name                  = "${var.prefix}-vm"
  computer_name         = "recesi-${var.environment}"
  location              = azurerm_resource_group.application.location
  resource_group_name   = azurerm_resource_group.application.name
  network_interface_ids = [azurerm_network_interface.application.id]
  size                  = var.vm_size
  admin_username        = var.admin_username
  tags                  = local.common_tags

  disable_password_authentication = true

  admin_ssh_key {
    username   = var.admin_username
    public_key = var.admin_ssh_public_key
  }

  os_disk {
    name                 = "${var.prefix}-osdisk"
    caching              = "ReadWrite"
    storage_account_type = "StandardSSD_LRS"
  }

  source_image_reference {
    publisher = "Canonical"
    offer     = "0001-com-ubuntu-server-jammy"
    sku       = "22_04-lts-gen2"
    version   = "latest"
  }

  boot_diagnostics {}
}
