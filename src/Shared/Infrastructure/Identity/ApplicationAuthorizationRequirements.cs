using Microsoft.AspNetCore.Authorization;

namespace DynamoLeagueBlazor.Shared.Infrastructure.Identity;

public static class PolicyRequirements
{
    public const string Admin = nameof(Admin);
    public const string User = nameof(User);
    public const string IsAdminApproved = nameof(IsAdminApproved);

    public static AuthorizationOptions AddApplicationAuthorizationPolicies(this AuthorizationOptions options)
    {
        // Add two policies - one for admins and one for all authenticated users.
        // We want to set the user policy as the default. 
        options.AddPolicy(Admin, builder =>
        {
            builder
                .RequireAuthenticatedUser()
                .RequireRole(RoleName.Admin);
        });

        options.AddPolicy(User, builder =>
        {
            builder.Combine(GetUserAuthorizationPolicy());
        });

        options.AddPolicy(IsAdminApproved,
            policy => policy.RequireClaim(nameof(IUser.Approved), bool.TrueString));

        options.DefaultPolicy = GetUserAuthorizationPolicy();

        return options;
    }

    public static AuthorizationPolicy GetUserAuthorizationPolicy()
    {
        return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
    }
}