terraform {
  backend "azurerm" {
    resource_group_name  = "recesi-tfstate-group"
    storage_account_name = "recesitfstate418cec1d"
    container_name       = "tfstate"
    key                  = "recesi-prod.terraform.tfstate"
    use_azuread_auth     = true
  }
}
