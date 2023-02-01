using Company.WebApplication1.Data;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Company.WebApplication1;

public static class OpenIddictExtensions
{
    public static IServiceCollection AddOpenIddictServer(this IServiceCollection services)
    {
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<ApplicationDbContext>();

            })
            .AddServer(options =>
            {
                options.SetAuthorizationEndpointUris("connect/authorize")
                       .SetDeviceEndpointUris("connect/device")
                       .SetIntrospectionEndpointUris("connect/introspect")
                       .SetLogoutEndpointUris("connect/logout")
                       .SetTokenEndpointUris("connect/token")
                       .SetUserinfoEndpointUris("connect/userinfo")
                       .SetVerificationEndpointUris("connect/verify");

                options.AllowAuthorizationCodeFlow()
                       .AllowDeviceCodeFlow()
                       .AllowPasswordFlow()
                       .AllowRefreshTokenFlow();

                options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();

                options.RequireProofKeyForCodeExchange();

                options.UseAspNetCore()
                       .EnableStatusCodePagesIntegration()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableLogoutEndpointPassthrough()
                       .EnableTokenEndpointPassthrough()
                       .EnableUserinfoEndpointPassthrough()
                       .EnableVerificationEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.AddAudiences("resource_server");

                options.UseLocalServer();

                options.UseAspNetCore();
            });
        #if (DataSeeding)

        builder.Services.AddScoped<OpenIddictInitializer>();
        #endif

        return services;
    }
}
