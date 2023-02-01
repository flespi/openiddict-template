#if (OrganizationalAuth || IndividualB2CAuth)
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
#endif
#if (WindowsAuth)
using Microsoft.AspNetCore.Authentication.Negotiate;
#endif
#if (IndividualLocalAuth)
using Microsoft.AspNetCore.Identity;
#endif
#if (OrganizationalAuth)
using Microsoft.AspNetCore.Mvc.Authorization;
#endif
#if (IndividualLocalAuth)
using Microsoft.EntityFrameworkCore;
#endif
#if (OrganizationalAuth || IndividualB2CAuth)
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
#endif
#if (MultiOrgAuth)
using Microsoft.IdentityModel.Tokens;
#endif
#if (GenerateGraph)
using Graph = Microsoft.Graph;
#endif
#if (IndividualLocalAuth)
using Company.WebApplication1.Data;
#endif
#if (OrganizationalAuth || IndividualB2CAuth || IndividualLocalAuth || MultiOrgAuth || GenerateGraph || WindowsAuth)

#endif
namespace Company.WebApplication1;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        #if (DataSeeding)
        builder.Configuration.AddJsonFile("oidc.json", optional: true, reloadOnChange: true);

        #endif
        // Add services to the container.
        #if (IndividualLocalAuth)
        var appConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options => {
        #if (UseLocalDB)
            options.UseSqlServer(appConnectionString);
        #else
            options.UseSqlite(appConnectionString);
        #endif
        });

        var idpConnectionString = builder.Configuration.GetConnectionString("IdentityProvider") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<IdentityProviderDbContext>(options => {
        #if (UseLocalDB)
            options.UseSqlServer(idpConnectionString);
        #else
            options.UseSqlite(idpConnectionString);
        #endif
            options.UseOpenIddict();
        });

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();
            
        builder.Services.AddOpenIddictServer();
        #elif (OrganizationalAuth)
        #if (GenerateApiOrGraph)
        var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ');

        #endif
        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        #if (GenerateApiOrGraph)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
                .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
        #if (GenerateApi)
                    .AddDownstreamWebApi("DownstreamApi", builder.Configuration.GetSection("DownstreamApi"))
        #endif
        #if (GenerateGraph)
                    .AddMicrosoftGraph(builder.Configuration.GetSection("DownstreamApi"))
        #endif
                    .AddInMemoryTokenCaches();
        #else
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
        #endif
        #elif (IndividualB2CAuth)
        #if (GenerateApi)
        var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ');

        #endif
        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        #if (GenerateApi)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdB2C"))
                .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
                    .AddDownstreamWebApi("DownstreamApi", builder.Configuration.GetSection("DownstreamApi"))
                    .AddInMemoryTokenCaches();
        #else
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdB2C"));
        #endif
        #endif
        #if (OrganizationalAuth)

        builder.Services.AddControllersWithViews(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });
        #else
        builder.Services.AddControllersWithViews();
        #endif
        #if (OrganizationalAuth || IndividualB2CAuth)
        builder.Services.AddRazorPages()
            .AddMicrosoftIdentityUI();
        #endif
        #if (WindowsAuth)

        builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
        .AddNegotiate();

        builder.Services.AddAuthorization(options =>
        {
            // By default, all incoming requests will be authorized according to the default policy.
            options.FallbackPolicy = options.DefaultPolicy;
        });
        builder.Services.AddRazorPages();
        #endif

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        #if (IndividualLocalAuth)
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
            #if (DataSeeding)
            using (var scope = app.Services.CreateScope())
            {
                var initialiser = scope.ServiceProvider.GetRequiredService<OpenIddictInitializer>();
                await initialiser.SeedAsync();
            }
            #endif
        }
        else
        #else
        if (!app.Environment.IsDevelopment())
        #endif
        {
            app.UseExceptionHandler("/Home/Error");
        #if (HasHttpsProfile)
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        #else
        }
        #endif
        app.UseStaticFiles();

        app.UseRouting();
        #if (IndividualLocalAuth)

		app.UseCors(options =>
	        options
		        .AllowAnyOrigin()
		        .AllowAnyMethod()
		        .AllowAnyHeader()
        );
        #endif

		app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
#if (OrganizationalAuth || IndividualAuth)
        app.MapRazorPages();
#endif

        app.Run();
    }
}
