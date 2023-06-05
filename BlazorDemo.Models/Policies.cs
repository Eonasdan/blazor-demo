using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorDemo.Models;

public static class Policies
{
    public static class ApplicationClaimTypes
    {
        public const string RegisteredUser = nameof(RegisteredUser);
    }

    public static class PolicyRegisteredUser
    {
        public const string Name = nameof(PolicyRegisteredUser);

        public static void Policy(AuthorizationPolicyBuilder policyBuilder)
        {
            policyBuilder.RequireAssertion(context =>
                context.User.HasClaim(c => c.Type == ApplicationClaimTypes.RegisteredUser));
        }
    }

    public static void AddApplicationAuthorization(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddAuthorizationCore(options =>
        {
            options.AddPolicy(PolicyRegisteredUser.Name, PolicyRegisteredUser.Policy);
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
    }
}