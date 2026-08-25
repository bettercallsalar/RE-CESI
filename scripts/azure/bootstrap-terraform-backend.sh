#!/usr/bin/env bash

set -Eeuo pipefail

readonly RESOURCE_GROUP_NAME="${TFSTATE_RESOURCE_GROUP_NAME:-recesi-tfstate-group}"
readonly STORAGE_ACCOUNT_NAME="${TFSTATE_STORAGE_ACCOUNT_NAME:-recesitfstate418cec1d}"
readonly CONTAINER_NAME="${TFSTATE_CONTAINER_NAME:-tfstate}"
readonly LOCATION="${AZURE_LOCATION:-polandcentral}"
readonly STORAGE_ROLE="Storage Blob Data Contributor"
readonly RETENTION_DAYS="${TFSTATE_RETENTION_DAYS:-7}"

require_command() {
  local command_name="$1"

  if command -v "$command_name" >/dev/null 2>&1; then
    return
  fi

  echo "Required command not found: $command_name" >&2
  exit 1
}

ensure_resource_group() {
  if [[ "$(az group exists --name "$RESOURCE_GROUP_NAME")" == "true" ]]; then
    return
  fi

  az group create \
    --name "$RESOURCE_GROUP_NAME" \
    --location "$LOCATION" \
    --tags project=RE-CESI purpose=terraform-state \
    --output none
}

ensure_storage_account() {
  if az storage account show \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$STORAGE_ACCOUNT_NAME" \
    --output none 2>/dev/null; then
    return
  fi

  az storage account create \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$STORAGE_ACCOUNT_NAME" \
    --location "$LOCATION" \
    --sku Standard_LRS \
    --kind StorageV2 \
    --https-only true \
    --min-tls-version TLS1_2 \
    --allow-blob-public-access false \
    --public-network-access Enabled \
    --tags project=RE-CESI purpose=terraform-state \
    --output none
}

ensure_storage_access() {
  local account_id
  local user_object_id

  account_id="$(az storage account show \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$STORAGE_ACCOUNT_NAME" \
    --query id \
    --output tsv)"
  user_object_id="$(az ad signed-in-user show --query id --output tsv)"

  if [[ -n "$(az role assignment list \
    --assignee "$user_object_id" \
    --role "$STORAGE_ROLE" \
    --scope "$account_id" \
    --query '[0].id' \
    --output tsv)" ]]; then
    return
  fi

  az role assignment create \
    --assignee-object-id "$user_object_id" \
    --assignee-principal-type User \
    --role "$STORAGE_ROLE" \
    --scope "$account_id" \
    --output none
}

ensure_container() {
  az storage container create \
    --name "$CONTAINER_NAME" \
    --account-name "$STORAGE_ACCOUNT_NAME" \
    --auth-mode login \
    --output none
}

harden_storage_account() {
  az storage account update \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$STORAGE_ACCOUNT_NAME" \
    --allow-shared-key-access false \
    --output none

  az storage account blob-service-properties update \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --account-name "$STORAGE_ACCOUNT_NAME" \
    --enable-versioning true \
    --enable-delete-retention true \
    --delete-retention-days "$RETENTION_DAYS" \
    --enable-container-delete-retention true \
    --container-delete-retention-days "$RETENTION_DAYS" \
    --output none
}

main() {
  require_command az
  ensure_resource_group
  ensure_storage_account
  ensure_storage_access
  ensure_container
  harden_storage_account

  echo "Terraform backend is ready: $STORAGE_ACCOUNT_NAME/$CONTAINER_NAME"
}

main "$@"
