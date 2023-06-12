// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace BlazorDemo.Masl.MSALClient
{
    /// <summary>
    /// App settings, populated from appsettings.json file
    /// </summary>
    public class AzureADB2CConfig
    {
        /// <summary>
        /// Gets or sets the Azure AD authority.
        /// </summary>
        /// <value>
        /// The Azure AD authority URL.
        /// </value>
        /// <remarks>
        ///   - For Work or School account in your org, use your tenant ID, or domain
        ///   - for any Work or School accounts, use organizations
        ///   - for any Work or School accounts, or Microsoft personal account, use common
        ///   - for Microsoft Personal account, use consumers
        /// </remarks>
        public string? Authority { get; set; }

        /// <summary>
        /// Gets or sets the login instance for your B2C tenant. In B2C usually of the form https://<YOUR_DOMAIN>.b2clogin.com
        /// </summary>
        /// <value>
        /// The login instance for your B2C tenant
        /// </value>
        public string? Instance { get; set; }

        /// <summary>
        /// Gets or sets the domain of your B2C tenant
        /// </summary>
        /// <value>
        /// The login instance for your B2C tenant
        /// </value>
        public string? Domain { get; set; }


        /// <summary>
        /// Gets or sets the client Id (App Id) from the app registration in the Azure AD portal.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string? ClientId { get; set; }

        /// <summary>
        /// Gets or sets the tenant identifier (tenant Id/directory id) of the Azure AD tenant where the app registration exists.
        /// </summary>
        /// <value>
        /// The tenant identifier.
        /// </value>
        public string? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the sign-in/sign-up policy you wish to use for your application
        /// </summary>
        /// <value>
        /// The sign-in/sign-up policy to use with this application
        /// </value>
        public string? SignUpSignInPolicyId { get; set; }

        /// <summary>
        /// Gets or sets the reset password policy you wish to use for your application
        /// </summary>
        /// <value>
        /// The reset password policy to use with this application
        /// </value>
        public string? ResetPasswordPolicyId { get; set; }

        /// <summary>
        /// Gets or sets the edit user profile policy you wish to use for your application
        /// </summary>
        /// <value>
        /// The edit user profile policy to use with this application
        /// </value>
        public string? EditProfilePolicyId { get; set; }
    }
}