locals {
  budget_thresholds = toset([25, 50, 75, 90])
}

resource "azurerm_consumption_budget_subscription" "student" {
  count = var.enable_budget ? 1 : 0

  name            = "recesi-student-annual-budget"
  subscription_id = data.azurerm_subscription.current.id
  amount          = var.budget_amount
  time_grain      = "Annually"

  time_period {
    start_date = var.budget_start_date
  }

  dynamic "notification" {
    for_each = local.budget_thresholds

    content {
      enabled        = true
      threshold      = notification.value
      operator       = "GreaterThanOrEqualTo"
      threshold_type = "Actual"
      contact_emails = var.budget_contact_emails
    }
  }
}
