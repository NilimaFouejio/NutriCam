using System.Security.Cryptography;
using NutriCam.Models;

namespace NutriCam.Services;

/// <summary>
/// Authentification LOCALE (un compte par appareil) : suffisante pour le prototype.
/// À remplacer par l'API d'authentification (JWT) prévue au cahier des charges technique.
/// </summary>
public class AuthService
{
    private readonly ProgramStore _store;

    public AuthService(ProgramStore store) => _store = store;

    public async Task<(bool Ok, string? Error)> RegisterAsync(string firstName, string email, string password)
    {
        await Task.Delay(500); // simule l'appel réseau

        var existing = _store.Data.Account;
        if (existing is not null && existing.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase))
            return (false, "Un compte existe déjà avec cet e-mail. Connectez-vous.");

        var salt = RandomNumberGenerator.GetBytes(16);
        _store.ResetPersonalData(); // nouveau compte = nouveau parcours
        _store.Data.Account = new UserAccount
        {
            FirstName = firstName.Trim(),
            Email = email.Trim(),
            Salt = Convert.ToBase64String(salt),
            PasswordHash = Hash(password, salt)
        };
        _store.Data.SignedIn = true;
        _store.Save();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> LoginAsync(string email, string password)
    {
        await Task.Delay(500);

        var account = _store.Data.Account;
        if (account is null)
            return (false, "Aucun compte trouvé sur cet appareil. Créez un compte pour commencer.");

        var hash = Hash(password, Convert.FromBase64String(account.Salt));
        if (!account.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase) || hash != account.PasswordHash)
            return (false, "E-mail ou mot de passe incorrect.");

        _store.Data.SignedIn = true;
        _store.Save();
        return (true, null);
    }

    public void Logout()
    {
        _store.Data.SignedIn = false;
        _store.Save();
    }

    private static string Hash(string password, byte[] salt)
        => Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32));
}
