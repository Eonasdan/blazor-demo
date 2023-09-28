$ErrorActionPreference = "Stop"
<#
    Copyright 2023 by Jonathan Peterson - https://jonathanpeterson.com
    License MIT
    It would be nice if you left this here.
#>

$graphApiUri = "https://graph.microsoft.com/v1.0"
$b2cUri = "https://main.b2cadmin.ext.azure.com"

<#
.DESCRIPTION
    Automatically creates an Azure B2C tenant. Use the configuration.[environment].json file
    to configure settings and resources to create.

.EXAMPLE
    . ./1.0-create.ps1
    New-B2CTenant -Environment local
#>
function New-B2CTenant
{
    [CmdletBinding()]
    param (
        [Parameter(HelpMessage = "Environment")]
        [ValidateSet('local', 'dev', 'stage', 'prod')]
        [string] $Environment = "local"
    )
    try
    {
        $configuration = Get-Content "configuration.$( $Environment ).json" | ConvertFrom-Json

        Write-Host "Checking if Resource Group `"$( $configuration.Settings.ResourceGroup )`" exists..."
        Get-AzResourceGroup -Name $configuration.Settings.ResourceGroup -ErrorVariable notPresent -ErrorAction SilentlyContinue > $null

        if ($notPresent)
        {
            throw "Resource Group `"$( $configuration.Settings.ResourceGroup )`" does not exist."
        }
        $Script:tokens = @{ }

        $Name = $configuration.Settings.Name
        $TenantId = "$( $Name ).onmicrosoft.com"

        $Script:tenantId = $TenantId

        # Create the B2C tenant resource in Azure
        New-AzureADB2CTenant `
            -Name $Name `
            -Location $configuration.Settings.Location `
            -CountryCode $configuration.Settings.CountryCode `
            -AzureResourceGroup $configuration.Settings.ResourceGroup

        #Create an API Connector
        $connectorId = New-ApiConnector  `
            -Configuration $configuration.ApiConnector

        # Add custom attributes
        New-CustomAttribute `
            -Attributes $configuration.UserAttributes

        #Create the apps
        $msApp = New-App `
            -Definition ((Get-Content ./app-ms-account.json).Replace("==B2CNAME==", $Name) | ConvertFrom-Json -Depth 100) `
            -AppSecret "IdpMicrosoftSecret" `
            -KeyVault $configuration.Settings.KeyVault

        $apiDefinition = (Get-Content -Raw $configuration.Apps.Api.config) `
              -replace "==B2CNAME==", $Name `
              -replace "==API=OAUTH=ID", $configuration.Apps.Api.oAuthID

        $apiDefinition = $apiDefinition | ConvertFrom-Json -Depth 100

        $apiApp = New-App -Definition $apiDefinition

        $webDefinition = (Get-Content -Raw $configuration.Apps.Web.config) `
              -replace "==B2CNAME==", $Name `
              -replace "==API=APP=ID==", "$( $apiApp.AppId )" `
              -replace "==API=PERMISSION=ID==", "$( $apiApp.api.oauth2PermissionScopes[0].Id )"

        $webDefinition = $webDefinition | ConvertFrom-Json
        $webDefinition.spa.redirectUris = $configuration.Apps.Web.redirectUris

        $webApp = New-App -Definition $webDefinition

        $nativeDefinition = (Get-Content -Raw $configuration.Apps.Native.config) `
              -replace "==B2CNAME==", $Name `
              -replace "==API=APP=ID==", "$( $apiApp.AppId )" `
              -replace "==API=PERMISSION=ID==", "$( $apiApp.api.oauth2PermissionScopes[0].Id )"

        $nativeDefinition = $nativeDefinition | ConvertFrom-Json
        $nativeDefinition.publicClient.redirectUris = $configuration.Apps.Native.redirectUris

        $nativeApp = New-App -Definition $nativeDefinition

        #Create IDPs
        $configuration.IdentityProviders | ForEach-Object {
            $provider = $_
            if ($provider.displayName -eq "Microsoft")
            {
                $provider.appId = $msApp.AppId
            }

            New-IdentityProvider `
                -Provider $provider `
                -KeyVault $configuration.Settings.KeyVault
        }

        #Add user signin user flow
        Add-UserFlow `
            -TenantName $Name `
            -DefinitionFilePath ./AdminUserJourneys.json `
            -ConnectorId $connectorId `
            -StorageAccountName $configuration.Settings.StorageAccount `
            -MicrosoftAppId $msApp.AppId

        return @{
            webAppId = $webApp.AppId
            apiAppId = $apiApp.AppId
        }
    }
    catch
    {
        Write-Host "Error completing" -Foreground "Red"
        Write-Host $_.Exception.Message -Foreground "Red"
        Write-Host $_.ScriptStackTrace -Foreground "DarkGray"
        #$_.Exception | ConvertTo-Json | Out-File -FilePath "./error.txt"
        exit 1
    }
}

<#
    .SYNOPSIS
        Create new Azure AD B2C tenant in a specific subscription and resource group.
        Must be followed by Invoke-TenantInit to finalize the creation of default apps.

        Required: Azure CLI authenticated for the target subscription.
        Required: Resource provider: "Microsoft.AzureActiveDirectory". The function will attempt to register if not done so yet.
        az provider register --namespace Microsoft.AzureActiveDirectory

        Azure PowerShell Alternative: Invoke-AzRestMethod
#>
function New-AzureADB2CTenant
{
    param(
    # Tenant name without the '.onmicrosoft.com' part.
        [string] $Name,

    # Can be one of 'United States', 'Europe', 'Asia Pacific', or 'Australia' (preview).
        [Parameter()]
        [ValidateSet('United States', 'Europe', 'Asia Pacific', 'Australia')]
        [string] $Location,

    # Where data resides. Two letter country code (e.g. 'US', 'CZ', 'DE').
    # Valid country codes are listed here: https://docs.microsoft.com/en-us/azure/active-directory-b2c/data-residency
        [string] $CountryCode,

    # Under which Azure resource group will this B2C tenant reside.
        [string] $AzureResourceGroup
    )
    
    $tries = 0

    # If any registration state comes back as not "Registered" then return false
    function Get-ProviderRegistered()
    {
        return (Get-AzResourceProvider -ProviderNamespace Microsoft.AzureActiveDirectory | Select-Object -ExpandProperty RegistrationState | Where-Object { $_ -ne "Registered" }).Count -eq 0
    }

    if ((Get-ProviderRegistered) -eq $false)
    {
        Write-Host "Resource Provider 'Microsoft.AzureActiveDirectory' not registered yet. Registering now..."
        Register-AzResourceProvider -ProviderNamespace Microsoft.AzureActiveDirectory

        while ((Get-ProviderRegistered) -eq $false)
        {
            Write-Host "Resource Provider registration not yet finished. Waiting..."
            Start-Sleep -Seconds 10
            $tries++
            if ($tries -ge 11)
            {
                throw "Tried 10 times waiting for AzureActiveDirectory provider to be registered"
            }
        }
        Write-Host "Resource Provider registration finished."
        $tries = 0
    }

    # Check if tenant already exists
    Write-Host "Checking if tenant `"$Name`" already exists..."

    $resource = Get-AzResource -Name "$( $Name ).onmicrosoft.com" -ResourceGroupName $AzureResourceGroup

    if ($null -ne $resource)
    {
        Write-Debug "Tenant '$Name' already exists. Not attempting to recreate it."
        return
    }

    Write-Host "Creating B2C tenant `"$( $Name )`"..."

    Write-Host "Getting subscription ID from the current account..."
    $AzureSubscriptionId = (Get-AzContext).Subscription.id

    $resourceId = "/subscriptions/$AzureSubscriptionId/resourceGroups/$AzureResourceGroup/providers/Microsoft.AzureActiveDirectory/b2cDirectories/$Name.onmicrosoft.com"

    $body = @{
        location = "$( $Location )"
        sku = @{
            name = "Standard"
            tier = "A0"
        }
        properties = @{
            createTenantProperties = @{
                displayName = "$( $Name )"
                countryCode = "$( $CountryCode )"
            }
        }
    }

    Invoke-Post `
         -Uri "https://management.azure.com$( $resourceId )?api-version=2023-01-18-preview" `
         -Method "Put" `
         -Body $body `
         -Token (Get-AzAccessToken).Token > $null

    Write-Host "*** B2C Tenant creation started. It can take a moment to complete."

    function Get-ResourceCreated()
    {
        $resource = Get-AzResource -Name "$( $Name ).onmicrosoft.com" -ResourceGroupName $AzureResourceGroup
        return $null -ne $resource
    }

    if ((Get-ResourceCreated) -eq $false)
    {
        while ((Get-ResourceCreated) -eq $false)
        {
            Write-Host "Waiting for 30 seconds for B2C tenant creation..."
            Start-Sleep -Seconds 30
            $tries++
            if ($tries -ge 11)
            {
                throw "Tried 10 times waiting for tenant to be created."
            }
        }
    }

    Write-Host "Invoking tenant initialization..."
    Invoke-Get `
        -Uri "$( $b2cUri )/api/tenants/GetAndInitializeTenantPolicy?tenantId=$( $Script:tenantId )&skipInitialization=false" `
        -Token (Get-Token -TokenType "management") `
        > $null
}

function New-IdentityProvider
{
    param (
        [string] $TenantId = $Script:tenantId,
        [object] $Provider,
        [string] $KeyVault
    )

    $token = Get-Token -TokenType "management"
    $idpUri = "$( $b2cUri )/api/idp"

    $existing = Invoke-Get `
        -Uri "$( $idpUri )/GetTenantIdpList?tenantId=$( $TenantId )&`$filter=displayName eq '$( $provider.displayName )'" `
        -Token $token

    $existing = ($existing)?[0]

    Write-Host "Getting IDP secret from Key Vault"
    $secret = Get-AzKeyVaultSecret -VaultName $KeyVault -Name $Provider.appSecret -AsPlainText

    $Provider.appSecret = $secret

    if ($existing)
    {
        Write-Host "Updating existing IDP for $( $provider.displayName )"
        Invoke-Post `
            -Uri "$( $idpUri )?tenantId=$( $TenantId )&technicalProfileId=$( $Provider.technicalProfileId )" `
            -Method "Put" `
            -Body $Provider `
            -Token $token > $null
    }
    else
    {
        Write-Host "Creating new IDP for $( $provider.displayName )"
        Invoke-Post `
            -Uri "$( $idpUri )?tenantId=$( $TenantId )&technicalProfileId=$( $Provider.technicalProfileId )" `
            -Method "Post" `
            -Body $Provider `
            -Token $token > $null
    }
}

function New-ApiConnector
{
    param (
        [object] $Configuration,
        [string] $TenantId = $Script:tenantId
    )

    $connectorUri = "$( $b2cUri )/graph/cpimApiConnectors"

    # Check for existing connector

    $token = Get-Token -TokenType "management"

    $result = Invoke-Get `
        -Uri "$( $connectorUri )?tenantId=$( $TenantId )&`$filter=displayName%20eq%20'$( $Configuration.displayName )'" `
        -Token  $token

    $existingItem = ($result.value)?[0]

    if ($existingItem)
    {
        Write-Host "Updating API Connector..."
        Invoke-Post `
            -Uri "$( $connectorUri )/$( $existingItem.id )?tenantId=$( $TenantId )" `
            -Method "Patch" `
            -Token $token `
            -Body $Configuration > $null

        return $existingItem.id
    }
    else
    {
        Write-Host "Creating API Connector..."
        $result = Invoke-Post `
            -Uri "$( $connectorUri )?tenantId=$( $TenantId )" `
            -Token  $token `
            -Body $Configuration

        return $result.id
    }
}

<#
    .DESCRIPTION
        Create a custom user attribute in the tenant.

        Requires: Azure CLI authenticated with owner permissions for the tenant.
        Alternatively, the /beta/identity/userFlowAttributes Graph endpoint can be used.
#>
function New-CustomAttribute
{
    param (
        [string] $TenantId = $Script:tenantId,
        [object] $Attributes
    )

    #Check for existing attribute
    $userAttributesUri = "$( $b2cUri )/api/userAttribute"

    $existing = @(Invoke-Get `
            -Uri "$( $userAttributesUri )/GetAllUserAttributes?tenantId=$( $TenantId )&`$filter=contains(id, 'extension') and allowEdit eq true" `
            -Token (Get-Token -TokenType "management") `
        | Select-Object -ExpandProperty label)

    $configuration.UserAttributes | ForEach-Object {
        $attribute = $_

        if ( $existing.Contains($attribute.label))
        {
            Write-Host "Attribute `"$( $attribute.label )`" exists. Skipping"
        }
        else
        {
            Write-Host "Creating custom attribute `"$( $attribute.label )`"..."
            Invoke-Post `
                -Uri "$( $userAttributesUri )?tenantId=$( $TenantId )" `
                -Token (Get-Token -TokenType "management") `
                -Body $attribute
        }
    }
}

<#
    .SYNOPSIS
        Create user flow based on JSON definition from a file.
        Requires: Azure CLI authenticated with owner permissions for the tenant.
#>
function Add-UserFlow
{
    param(
        [string] $TenantName,
        [string] $TenantId = $Script:tenantId,
        [string] $DefinitionFilePath,
        [string] $ConnectorId,
        [string] $StorageAccountName,
        [string] $MicrosoftAppId
    )

    Write-Host "Preparing user flow..."
    $signinFlowContent = (Get-Content $DefinitionFilePath) `
          -Replace "==SA=NAME==", $StorageAccountName `
          -Replace "==B2CNAME==", $TenantName `
    | ConvertFrom-Json

    $signinFlowContent.restfulOptions.restApis.preSendClaimsRestful.restfulProviderId = $ConnectorId

    if (![string]::IsNullOrEmpty($MicrosoftAppId))
    {
        ($signinFlowContent.idpData.idpSelection.Where({ $_.displayName -eq "Microsoft" }, "First"))[0].appId = $MicrosoftAppId
    }

    if ( [string]::IsNullOrEmpty($StorageAccountName))
    {
        Write-Warning "Storage Account Name was not provided so Custome User Flow pages will not be used"
        $signinFlowContent.pageCustomizationData = @{ }
    }

    $userJourneyUri = "$( $b2cUri )/api/AdminUserJourneys"

    $token = Get-Token -TokenType "management"

    $existing = Invoke-Get `
        -Uri "$( $userJourneyUri )/$( $signinFlowContent.id )?tenantId=$( $TenantId )" `
        -Token $token `
        -IgnoreErrors $true # if this flow doesn't exist it will throw an error that we don't care

    if ($null -eq $existing)
    {
        Write-Host "Creating a new user flow"
        Invoke-Post `
            -Uri "$( $userJourneyUri )?tenantId=$( $TenantId )" `
            -Token $token `
            -Body $signinFlowContent > $null
    }
    else
    {
        Write-Host "Updating existing user flow"
        Invoke-Post `
            -Uri "$( $userJourneyUri )/$( $signinFlowContent.id )?tenantId=$( $TenantId )" `
            -Token $token `
            -Method "Put" `
            -Body $signinFlowContent > $null
    }
}

<#
 .Description
    Creates a B2C application registration to be used for users to sign-in.
#>
function New-App
{
    param (
        [object] $Definition,
        [string] $AppSecret = "",
        [string] $KeyVault = ""
    )
    #$Definition | ConvertTo-Json | Write-Host

    Write-Host "New-App for `"$( $Definition.displayName )`""

    $results = Invoke-Get `
        -Uri "$( $graphApiUri )/applications/?`$filter=displayName%20eq%20'$( $Definition.displayName )'"

    $app = $( $results )?.value[0]

    if ($null -ne $app)
    {
        Write-Host "App `"$( $Definition.displayName )`" already exists, updating..."
        $app = Invoke-Post `
            -Uri "$( $graphApiUri )/applications/$( $app.id )" `
            -Body $Definition `
            -Method "Patch"
    }
    else
    {
        Write-Host "Creating `"$( $Definition.displayName )`"..."
        $app = Invoke-Post `
            -Uri "$( $graphApiUri )/applications" `
            -Body $Definition

        Write-Host "Successfully created app with applicationId $( $app.AppId )"
    }

    Write-Host "Checking for a Service Prinicipal"

    $servicePrincipal = Get-ServicePrincipal -AppId $app.AppId

    Write-Host "Updating admin consent for the app..."
    Invoke-GrantScopes -App $app -AppPrincipal $servicePrincipal

    if (![string]::IsNullOrEmpty($AppSecret))
    {
        # Create secret for the app. This has to be done after the app is created.

        #remove existing secret
        if ((($app.passwordCredentials)?.displayName)?.Contains("secret")) {
        Write-Host "Removing existing secret"
        $passwords = @($app.passwordCredentials | Where-Object {
        $_.displayName -ne "secret"
        })

        Invoke-Post `
                -Uri "$($graphApiUri)/applications/$($app.id)" `
                -Method "Patch" `
                -Body @{
        passwordCredentials = $passwords
        } > $null
        }

        Write-Host "Creating secret..."
        $startDate = Get-Date -AsUTC
        $secret = Invoke-Post `
            -Uri "$($graphApiUri)/applications/$($app.id)/addPassword" `
            -Body @{
        passwordCredential = @{
        displayName = "secret"
        startDateTime = $startDate.ToString("o")
        endDateTime   = $startDate.AddMonths(6).ToString("o")
        }
        }

        $secretValue = ConvertTo-SecureString $secret.secretText -AsPlainText -Force

        Set-AzKeyVaultSecret `
            -VaultName $KeyVault `
            -Name $AppSecret `
            -SecretValue $secretValue
    }

    Write-Host "*** Azure AD B2C Application '$( $app.DisplayName )' created."
    Write-Host "*** Client ID: '$( $app.AppId )'"

    return $app
}

function Get-ServicePrincipal()
{
    param (
        [string] $AppId
    )

    if ($null -eq $Script:principals)
    {
        $Script:principals = @{ }
    }

    $principal = $Script:principals[$AppId]

    if ($null -ne $principal)
    {
        return $principal
    }

    $principal = Invoke-Get `
        -Uri "$( $graphApiUri )/servicePrincipals(appId='{$( $AppId )}')" `
        -IgnoreErrors $true

    if ($null -ne $principal)
    {
        $Script:principals[$AppId] = $principal
        return $principal
    }

    Write-Host "Creating Service Prinicpal for $( $AppId )"

    $principal = Invoke-Post `
        -Uri "$( $graphApiUri )/servicePrincipals" `
        -Body @{ appId = $AppId }


    $Script:principals[$AppId] = $principal

    return $principal
}

function Invoke-GrantScopes()
{
    param (
        [object] $App,
        [object] $AppPrincipal
    )

    Remove-CurrentOauth2PermissionGrants -ServicePrincipalId $AppPrincipal.id
    $App.RequiredResourceAccess | ForEach-Object {
        $resource = $_
        $scopes = @()
        $servicePrincipal = Get-ServicePrincipal -AppId $resource.ResourceAppId

        $resource.ResourceAccess | Where-Object { $_.Type -eq "Scope" } | ForEach-Object {
            $permission = $_
            $resouceScopes = $servicePrincipal.Oauth2PermissionScopes | Where-Object Id -eq $permission.Id
            $scopes += $resouceScopes.Value
        }

        $scopes = $scopes -join " "
        if (![string]::IsNullOrEmpty($scopes))
        {
            Write-Host "Granting `""$( $scopes )"`" to $( $servicePrincipal.id )"
            Invoke-Post `
                -Uri "$( $graphApiUri )/oauth2PermissionGrants" `
                -Method "Post" `
                -Body @{
                clientId = $AppPrincipal.Id
                consentType = "AllPrincipals"
                resourceId = $servicePrincipal.Id
                scope = $scopes
            } > $null
        }
        else
        {
            Write-Host "No grants to give"
        }
    }
}

function Remove-CurrentServicePrincipalGrants
{
    param(
        [Parameter(Mandatory)]
        [string]
        $ServicePrincipalObjectId
    )

    Write-Host "Getting all existing Service Principal Grants for $ServicePrincipalObjectId..."

    $apiUrl = "$( $graphApiUri )/servicePrincipals/$ServicePrincipalObjectId/appRoleAssignments"

    $appRoleAssignmentCollection = Invoke-Get -Uri $apiUrl

    if ($appRoleAssignmentCollection.Count -eq 0)
    {
        return
    }

    $appRoleAssignmentCollection | ForEach-Object {
        $appRoleAssignment = $_

        Write-Host:"Removing `"$( $ServicePrincipalObjectId )`" grant for appRoleId: `"$( $appRoleAssignment.appRoleId )`" for resource: `"$( $appRoleAssignment.resourceDisplayName )`""
        $deleteApiUrl = "$apiUrl/$( $appRoleAssignment.id )"

        Invoke-Delete -Uri $deleteApiUrl
    }
}

function Remove-CurrentOauth2PermissionGrants
{
    param(
        [Parameter(Mandatory)]
        [string]
        $ServicePrincipalId
    )

    Write-Host "Removing current grants"
    $oauth2PermissionGrantsApiUrl = "$( $graphApiUri )/oauth2PermissionGrants"
    $uri = "$( $oauth2PermissionGrantsApiUrl )?`$filter=clientId eq '$( $ServicePrincipalId )' and consentType eq 'AllPrincipals'"

    $existingCollection = Invoke-Get -Uri $uri

    $existingCollection.value | ForEach-Object {
        $existing = $_

        Write-Host "Removing existing access for resourceId:$( $existing.resourceId )"

        Invoke-Delete -Uri "$( $oauth2PermissionGrantsApiUrl )/$( $existing.id )"
    }
}

function Get-Token
{
    param (
        [string] $TokenType
    )

    $token = $Script:tokens[$tokenType]

    if ($null -ne $token)
    {
        return $token
    }

    switch ($tokenType)
    {
        Graph {
            $token = (Get-AzAccessToken -TenantId $Script:tenantId -ResourceTypeName MSGraph -ErrorAction Stop).Token
        }
        Management {
            $token = (Get-AzAccessToken -TenantId $Script:tenantId -ErrorAction Stop).Token
        }
        Default {
            throw "Unknown Token Type"
        }
    }

    $Script:tokens[$tokenType] = $token
    return $token
}

function Invoke-Delete()
{
    param (
        [object] $Uri,
        [string] $Token
    )

    if ( [string]::IsNullOrEmpty($Token))
    {
        $Token = Get-Token -TokenType "graph"
    }

    try
    {
        Invoke-WebRequest `
            -Uri $Uri `
            -Method "Delete" `
            -Headers @{
            "Authorization" = "Bearer $( $Token )"
            "Accept" = "application/json"
            "Content-Type" = "application/json"
        } > $null #`
        #-SkipHttpErrorCheck
    }
    catch
    {
        Write-Host "Delete Uri: $( $Uri )"
        $_ | Write-Host
        throw
    }
}

function Invoke-Post()
{
    param (
        [object] $Uri,
        [string] $Token,
        [object] $Body,
        [string] $Method = "Post"
    )

    if ( [string]::IsNullOrEmpty($Token))
    {
        $Token = Get-Token -TokenType "graph"
    }

    $Body = $Body | ConvertTo-Json -Depth 100

    try
    {

        $result = Invoke-WebRequest `
            -Uri $Uri `
            -Method $Method `
            -Headers @{
            "Authorization" = "Bearer $( $Token )"
            "Accept" = "application/json"
            "Content-Type" = "application/json"
            "Prefer" = "return-content"
        } `
            -Body $Body

        return $result.Content | ConvertFrom-Json -Depth 100
    }
    catch
    {
        Write-Host "$( $Method ) Uri: $( $Uri )"
        $_ | Write-Host
        throw
    }
}

function Invoke-Get()
{
    param (
        [object] $Uri,
        [string] $Token,
        [bool] $IgnoreErrors
    )

    if ( [string]::IsNullOrEmpty($Token))
    {
        $Token = Get-Token -TokenType "graph"
    }

    try
    {
        $result = Invoke-WebRequest `
            -Uri $Uri `
            -Method "Get" `
            -Headers @{
            "Authorization" = "Bearer $( $Token )"
            "Accept" = "application/json"
        }

        return $result.Content | ConvertFrom-Json -Depth 100
    }
    catch
    {

        # If we get a 404 that resource doesn't exist, sometimes we don't care,
        # i.e. checking for an existing resource
        if ($_.Exception.Response.StatusCode.Value__ -eq 404 && $IgnoreErrors) {
        return
        }

        Write-Host "Get Uri: $($Uri)"
        $_ | Write-Host
        throw
    }
}
