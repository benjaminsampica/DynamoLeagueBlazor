using Microsoft.AspNetCore.Authorization;

namespace DynamoLeagueBlazor.Shared.Infastructure.Identity;

public class AdminApprovedAuthorizationRequirement : IAuthorizationHandler, IAuthorizationRequirement
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var adminApproved = context.User.HasClaim("Approved", bool.TrueString);

        if (adminApproved) context.Succeed(this);

        return Task.CompletedTask;
    }
}

public static class PolicyRequirements
{
    public const string Admin = "Admin";
    public const string User = "User";

    public static AuthorizationOptions AddApplicationAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(Admin, builder =>
        {
            builder
                .RequireAuthenticatedUser()
                .AddRequirements(new AdminApprovedAuthorizationRequirement())
                .RequireRole(RoleName.Admin);
        });

        options.AddPolicy(User, builder =>
        {
            builder.Combine(GetUserAuthorizationPolicy());
        });

        options.DefaultPolicy = GetUserAuthorizationPolicy();

        return options;
    }

    public static AuthorizationPolicy GetUserAuthorizationPolicy()
    {
        return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new AdminApprovedAuthorizationRequirement())
                .RequireRole(RoleName.User, RoleName.Admin)
                .Build();
    }
}