/*// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace BlazorDemo.Masl.MSALClient
{
    /// <summary>
    /// This is a singleton implementation to wrap the MSALClient and associated classes to support static initialization model for platforms that need this.
    /// </summary>
    public class PublicClientSingleton
    {
        /// <summary>
        /// This is the singleton used by Ux. Since PublicClientWrapper constructor does not have perf or memory issue, it is instantiated directly.
        /// </summary>
        public static PublicClientSingleton Instance { get; private set; } = new PublicClientSingleton();

        /// <summary>
        /// Gets the instance of MSALClientHelper.
        /// </summary>
        public DownstreamApiHelper DownstreamApiHelper { get; }

        /// <summary>
        /// Gets the instance of MSALClientHelper.
        /// </summary>
        public MSALClientHelper MSALClientHelper { get; }

        /// <summary>
        /// This will determine if the Interactive Authentication should be Embedded or System view
        /// </summary>
        public bool UseEmbedded { get; set; } = false;

        //// Custom logger for sample
        //private readonly IdentityLogger _logger = new IdentityLogger();

        /// <summary>
        /// Prevents a default instance of the <see cref="PublicClientSingleton"/> class from being created. or a private constructor for singleton
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private PublicClientSingleton()
        {
            // Load config
            var assembly = Assembly.GetExecutingAssembly();
            var embeddedConfigFilename = $"{Assembly.GetCallingAssembly().GetName().Name}.appsettings.json";
            using var stream = assembly.GetManifestResourceStream(embeddedConfigFilename);
            IConfiguration appConfiguration = new ConfigurationBuilder()
                .AddJsonStream(stream!)
                .Build();

            var azureADConfig = appConfiguration.GetSection("AzureAdB2C").Get<AzureADB2CConfig>();
            MSALClientHelper = new MSALClientHelper(azureADConfig!);

            var downStreamApiConfig = appConfiguration.GetSection("DownstreamApi").Get<DownStreamApiConfig>();
            DownstreamApiHelper = new DownstreamApiHelper(downStreamApiConfig!, MSALClientHelper);
        }

        /// <summary>
        /// Acquire the token silently
        /// </summary>
        /// <returns>An access token</returns>
        public async Task<string?> AcquireTokenSilentAsync()
        {
            // Get accounts by policy
            return await AcquireTokenSilentAsync(GetScopes()).ConfigureAwait(false);
        }

        /// <summary>
        /// Acquire the token silently
        /// </summary>
        /// <param name="scopes">desired scopes</param>
        /// <returns>An access token</returns>
        public async Task<string?> AcquireTokenSilentAsync(string[]? scopes)
        {
            return await MSALClientHelper.SignInUserAndAcquireAccessToken(scopes).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform the interactive acquisition of the token for the given scope
        /// </summary>
        /// <param name="scopes">desired scopes</param>
        /// <returns></returns>
        internal async Task<AuthenticationResult?> AcquireTokenInteractiveAsync(string[] scopes)
        {
            if (scopes == null) throw new ArgumentNullException(nameof(scopes));
            MSALClientHelper.UseEmbedded = UseEmbedded;
            return await MSALClientHelper.SignInUserInteractivelyAsync(scopes).ConfigureAwait(false);
        }

        /// <summary>
        /// It will sign out the user.
        /// </summary>
        /// <returns></returns>
        internal async Task SignOutAsync()
        {
            await MSALClientHelper.SignOutUserAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Gets scopes for the application
        /// </summary>
        /// <returns>An array of all scopes</returns>
        internal string[]? GetScopes()
        {
            return DownstreamApiHelper.DownstreamApiConfig.ScopesArray;
        }
    }
}*/