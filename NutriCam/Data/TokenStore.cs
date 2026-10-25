namespace NutriCam.Data;

/// <summary>Conservation du jeton d'accès. Jamais dans le fichier de données : stockage sécurisé du système.</summary>
public interface ITokenStore
{
    Task<string?> GetAsync();
    Task SetAsync(string token);
    void Clear();
}

public sealed class SecureTokenStore : ITokenStore
{
    private const string Key = "nutricam.jeton";
    private string? _memory;

    public async Task<string?> GetAsync()
    {
        if (_memory is not null) return _memory;
        try
        {
            _memory = await SecureStorage.Default.GetAsync(Key);
        }
        catch
        {
            // Stockage sécurisé indisponible (ex. Windows non empaqueté) : le jeton ne vit que pendant la session.
        }
        return _memory;
    }

    public async Task SetAsync(string token)
    {
        _memory = token;
        try
        {
            await SecureStorage.Default.SetAsync(Key, token);
        }
        catch
        {
            // voir GetAsync
        }
    }

    public void Clear()
    {
        _memory = null;
        try
        {
            SecureStorage.Default.Remove(Key);
        }
        catch
        {
            // rien à effacer
        }
    }
}
