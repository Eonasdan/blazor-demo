// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace BlazorDemo.Masl
{
    /// <summary>
    /// This is a wrapper for PCA. It is singleton and can be utilized by both application and the MAM callback
    /// </summary>
    public class ExternalAuthService : IExternalAuthService
    {
        private readonly Settings? _settings;

        internal IPublicClientApplication? PublicClientApplication { get; }
        public string[] Scopes { get; set; } = Array.Empty<string>();

        public event Action<ClaimsPrincipal>? UserChanged;
        private ClaimsPrincipal? _currentUser;

        public ClaimsPrincipal CurrentUser
        {
            get => _currentUser ?? new ClaimsPrincipal();
            set
            {
                _currentUser = value;

                UserChanged?.Invoke(_currentUser);
            }
        }

        public string? Token { get; internal set; }

        // public constructor
        public ExternalAuthService(IConfiguration configuration)
        {
            _settings = configuration.GetRequiredSection("Settings").Get<Settings>();
            var scopes = configuration.GetSection("DownstreamApi:Scopes").Get<string[]>();

            if (scopes == null) return;

            Scopes = scopes;
            // Create PCA once. Make sure that all the config parameters below are passed
            PublicClientApplication = PublicClientApplicationBuilder
                .Create(_settings?.ClientId)
                .WithB2CAuthority(_settings?.Authority)
#if !ANDROID
                .WithRedirectUri("http://localhost:61571")
#endif
                .Build();
        }

        public async Task LoginAsync()
        {
            CurrentUser = new ClaimsPrincipal();
            Token = null;

            try
            {
                await AcquireTokenSilentAsync(Scopes);
            }
            catch (MsalUiRequiredException)
            {
                await AcquireTokenInteractiveAsync(Scopes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //todo log
            }
        }

        /// <summary>
        /// Acquire the token silently
        /// </summary>
        /// <param name="scopes">desired scopes</param>
        /// <returns>Authentication result</returns>
        public async Task AcquireTokenSilentAsync(string[] scopes)
        {
            if (PublicClientApplication == null) return;

            var account = (await PublicClientApplication
                    .GetAccountsAsync(_settings?.PolicySignUpSignIn)
                    .ConfigureAwait(false))
                .FirstOrDefault();

            var authResult = await PublicClientApplication
                .AcquireTokenSilent(scopes, account)
                .ExecuteAsync()
                .ConfigureAwait(false);

            if (authResult == null) return;

            CurrentUser = authResult.ClaimsPrincipal;

            Token = authResult.AccessToken;
        }

        /// <summary>
        /// Perform the interactive acquisition of the token for the given scope
        /// </summary>
        /// <param name="scopes">desired scopes</param>
        /// <returns></returns>
        public async Task AcquireTokenInteractiveAsync(string[] scopes)
        {
            if (PublicClientApplication == null)
                return;

            var accounts = await PublicClientApplication
                .GetAccountsAsync(_settings?.PolicySignUpSignIn)
                .ConfigureAwait(false);
            var account = accounts.FirstOrDefault();

            var authResult = await PublicClientApplication.AcquireTokenInteractive(scopes)
                .WithB2CAuthority(_settings?.Authority)
                //.WithAccount(account)
                .WithParentActivityOrWindow(PlatformConfig.Instance.ParentWindow)
                .WithUseEmbeddedWebView(false)
                .WithSystemWebViewOptions(new SystemWebViewOptions
                {
                    HtmlMessageError = "<b>Sign-in failed. You can close this tab...</b>",
                    HtmlMessageSuccess = "<b>Sign-in successful. You can close this tab...</b>"
                })
                .ExecuteAsync()
                .ConfigureAwait(false);

            if (authResult == null) return;

            var claimsIdentity = new ClaimsIdentity(authResult.ClaimsPrincipal.Claims, "Basic");


            CurrentUser = new ClaimsPrincipal(claimsIdentity);
            Token = authResult.AccessToken;
        }

        /// <summary>
        /// Sign out may not perform the complete sign out as company portal may hold
        /// the token.
        /// </summary>
        /// <returns></returns>
        public async Task SignOutAsync()
        {
            CurrentUser = new ClaimsPrincipal();
            Token = null;
            Scopes = Array.Empty<string>();
            if (PublicClientApplication == null)
                return;

            var accounts = await PublicClientApplication.GetAccountsAsync().ConfigureAwait(false);
            foreach (var acct in accounts)
            {
                await PublicClientApplication.RemoveAsync(acct).ConfigureAwait(false);
            }
        }
    }
}