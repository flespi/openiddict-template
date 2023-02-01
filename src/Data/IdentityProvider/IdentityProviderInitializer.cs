using OpenIddict.Abstractions;

namespace Company.WebApplication1.Data;

public class IdentityProviderInitializer
{
	private readonly ILogger<IdentityProviderInitializer> _logger;
	private readonly IConfiguration _configuration;

	private readonly IOpenIddictApplicationManager _clientManager;
	private readonly IOpenIddictScopeManager _scopeManager;

	public IdentityProviderInitializer(
		ILogger<IdentityProviderInitializer> logger,
		IConfiguration configuration,
		IOpenIddictApplicationManager clientManager,
		IOpenIddictScopeManager scopeManager
		)
	{
		_logger = logger;
		_configuration = configuration;
		_clientManager = clientManager;
		_scopeManager = scopeManager;
	}

	public async Task SeedAsync()
	{
		try
		{
			await TrySeedAsync();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while seeding the database.");
			throw;
		}
	}

	public async Task TrySeedAsync()
	{
		var scopes = _configuration.GetSection("Oidc:Scopes").Get<OpenIddictScopeDescriptor[]>();
		var clients = _configuration.GetSection("Oidc:Clients").Get<OpenIddictApplicationDescriptor[]>();

		if (scopes is not null)
		{
			if (!await _scopeManager.ListAsync().AnyAsync())
			{
				foreach (var scope in scopes)
				{
					await _scopeManager.CreateAsync(scope);
				}
			}
		}

		if (clients is not null)
		{
			if (!await _clientManager.ListAsync().AnyAsync())
			{
				foreach (var client in clients)
				{
					await _clientManager.CreateAsync(client);
				}
			}
		}
	}
}
