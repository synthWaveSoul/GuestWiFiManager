from azure.identity import ClientSecretCredential
from azure.keyvault.secrets import SecretClient

# this should be read from Os Environmental Variables or from Azure Environment Variables
TENANT_ID = "your_tenant_id"
CLIENT_ID = "your_client_id"
CLIENT_SECRET = "your_client_secret"
KEYVAULT_NAME = "your_keyvault_name"
KEYVAULT_URL = f"https://{KEYVAULT_NAME}.vault.azure.net"

# credentials for Key Vault in Azure
credentials = ClientSecretCredential(
    tenant_id=TENANT_ID,
    client_id=CLIENT_ID,
    client_secret=CLIENT_SECRET
)

# connect to Key Vault in Azure
app_key_vault = SecretClient(vault_url=KEYVAULT_URL, credential=credentials)

sql_connection_string = app_key_vault.get_secret("your_secret_sql_connection_string").value

# Meraki platform API key
API_KEY = app_key_vault.get_secret("your_secret_meraki_api_key").value

# key for encrypt/decrypt guest WiFi password
# some randomly created encryption key to encrypt and decrypt the passwords stored in the database
# this particular key must be used for encryption and decryption to work
encryption_key = app_key_vault.get_secret("your_secret_encryption_key").value

# your company Meraki ID
org_id = app_key_vault.get_secret("your_secret_company_id").value

# your id of SSID
network_id = app_key_vault.get_secret("your_secret_ssid_id").value